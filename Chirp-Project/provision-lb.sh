#!/bin/bash

# Load balancer provisioning script
# Args:
#   $1 = hostname
#   $2 = state (MASTER/BACKUP)
#   $3 = priority
#   $4 = minitwit1 private IP
#   $5 = minitwit2 private IP
#   $6 = this LB private IP
#   $7 = peer LB private IP

if [ "$#" -lt 7 ]; then
    echo "Usage: $0 <lb_name> <role> <priority> <minitwit1_ip> <minitwit2_ip> <lb_private_ip> <peer_lb_private_ip>" >&2
    exit 1
fi

HOSTNAME=$1
STATE=$2
PRIORITY=$3
BACKEND_1=$4
BACKEND_2=$5
SRC_IP=$6
PEER_IP=$7
VIRTUAL_IP="129.212.253.237" #"157.245.27.199"
# Ports exposed on the reserved IP and forwarded to the same backend port.
FORWARDED_PORTS="8080" # 3000 9090 3100"

echo "Provisioning load balancer: $HOSTNAME as $STATE with priority $PRIORITY"

# Update package lists
apt-get update

# Install Nginx and Keepalived
apt-get install -y nginx keepalived libnginx-mod-stream

# Configure Nginx as a load balancer
cat > /etc/nginx/sites-available/default <<EOF
upstream backend_servers {
    ip_hash;
    server ${BACKEND_1};
    server ${BACKEND_2};
}

server {
    listen 80 default_server;
    listen [::]:80 default_server;

    location / {
        proxy_pass http://backend_servers;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
    }
}
EOF

# Configure TCP port forwarding on the reserved IP using the stream module.
cat > /etc/nginx/stream.conf <<EOF
stream {
EOF

for port in $FORWARDED_PORTS; do
cat >> /etc/nginx/stream.conf <<EOF
    upstream backend_servers_${port} {
        server ${BACKEND_1}:${port};
        server ${BACKEND_2}:${port};
    }

    server {
        listen ${port};
        proxy_pass backend_servers_${port};
    }
EOF
done

cat >> /etc/nginx/stream.conf <<EOF
}
EOF

# Ensure stream config is loaded at top-level nginx context.
if ! grep -q '^include /etc/nginx/stream.conf;$' /etc/nginx/nginx.conf; then
    sed -i '/^http {/i include /etc/nginx/stream.conf;' /etc/nginx/nginx.conf
fi

# Allow forwarded service ports through UFW on the load balancer.
for port in $FORWARDED_PORTS; do
    ufw allow ${port}/tcp
done

# Allow keepalived peer traffic over the private interface.
ufw allow in on eth1 from ${PEER_IP} to any

# Test and reload Nginx
nginx -t
systemctl restart nginx
systemctl enable nginx

cat > /etc/init/keepalived.conf <<EOF
description "load-balancing and high-availability service"

start on runlevel [2345]
stop on runlevel [!2345]

respawn

exec /usr/local/sbin/keepalived --dont-fork
EOF

cd /usr/local/bin || exit 1
sudo curl -fLSo assign-ip https://do.co/assign-ip

cat > /etc/keepalived/master.sh <<'EOF'
#!/bin/bash
set -u

# Keepalived runs this non-interactively; load token from root profile if present.
if [ -f /root/.bash_profile ]; then
    . /root/.bash_profile
fi

LOG_FILE=/var/log/keepalived-master.log
echo "[$(date -Iseconds)] notify_master invoked" >> "$LOG_FILE"

IP='129.212.253.237'
ID=$(curl -s http://169.254.169.254/metadata/v1/id)
HAS_RESERVED_IP=$(curl -s http://169.254.169.254/metadata/v1/reserved_ip/ipv4/active)

if [ "$HAS_RESERVED_IP" = "false" ]; then
    n=0
    while [ "$n" -lt 10 ]
    do
        if [ -n "${DO_TOKEN:-}" ]; then
            python3 /usr/local/bin/assign-ip "$IP" "$ID" >> "$LOG_FILE" 2>&1 && break
        else
            echo "DO_TOKEN is not set; cannot assign reserved IP" >&2
            echo "[$(date -Iseconds)] DO_TOKEN missing" >> "$LOG_FILE"
            break
        fi
        n=$((n+1))
        sleep 3
    done
else
    echo "[$(date -Iseconds)] reserved IP already active on this droplet" >> "$LOG_FILE"
fi
EOF

sudo chmod +x /etc/keepalived/master.sh

cat > /etc/keepalived/check_nginx.sh <<'EOF'
#!/bin/bash
/usr/bin/pgrep -x nginx >/dev/null
EOF

sudo chmod +x /etc/keepalived/check_nginx.sh

# Configure Keepalived
cat > /etc/keepalived/keepalived.conf <<EOF
global_defs {
    enable_script_security
    script_user root
}

vrrp_script check_nginx {
    script "/etc/keepalived/check_nginx.sh"
    user root
    interval 2
}

vrrp_instance VI_1 {    
    interface eth1
    state $STATE
    priority $PRIORITY

    virtual_router_id 51
    unicast_src_ip $SRC_IP
    unicast_peer {
        $PEER_IP
    }

    authentication {
        auth_type PASS
        auth_pass secret123
    }

    track_script {
        check_nginx
    }

    notify_master /etc/keepalived/master.sh
}
EOF

# Enable IP forwarding
echo "net.ipv4.ip_nonlocal_bind=1" >> /etc/sysctl.conf
sysctl -p

# Start and enable Keepalived
systemctl start keepalived
systemctl enable keepalived

echo "Load balancer $HOSTNAME provisioned successfully"
echo "Virtual IP: $VIRTUAL_IP"
echo "Navigate your browser to: $VIRTUAL_IP"