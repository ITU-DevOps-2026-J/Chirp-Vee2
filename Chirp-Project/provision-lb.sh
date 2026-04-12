#!/bin/bash

# Load balancer provisioning script
# Args: $1 = hostname, $2 = state (MASTER/BACKUP), $3 = priority

HOSTNAME=$1
STATE=$2
PRIORITY=$3
SRC_IP=$4
PEER_IP=$5
VIRTUAL_IP="144.126.246.132"

echo "Provisioning load balancer: $HOSTNAME as $STATE with priority $PRIORITY"

# Update package lists
apt-get update

# Install Nginx and Keepalived
apt-get install -y nginx keepalived

# Configure Nginx as a load balancer
cat > /etc/nginx/sites-available/default <<'EOF'
upstream backend_servers {
    ip_hash;
    server 104.248.28.105:8080;
    server 167.172.97.87:8080;
}

server {
    listen 80 default_server;
    listen [::]:80 default_server;

    location / {
        proxy_pass http://backend_servers;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
EOF

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

cat > /etc/keepalived/master.sh <<EOF
#!/bin/bash
set -e

# Keepalived runs scripts in a minimal environment. Load token if it is not already present.
if [ -z "\${DO_TOKEN:-}" ] && [ -f /root/.bash_profile ]; then
    # shellcheck disable=SC1091
    source ~/.bash_profile
fi

LOG_FILE="/var/log/keepalived-master.log"
IP='144.126.246.132'
ID=$(curl -s http://169.254.169.254/metadata/v1/id)
HAS_RESERVED_IP=$(curl -s http://169.254.169.254/metadata/v1/reserved_ip/ipv4/active)

if [ -z "\${DO_TOKEN:-}" ]; then
    echo "$(date -Iseconds) DO_TOKEN is not set; cannot assign reserved IP" >> "$LOG_FILE"
    exit 1
fi

if [ "$HAS_RESERVED_IP" = "false" ]; then
    n=0
    while [ $n -lt 10 ]
    do
        RESPONSE=$(curl -sS -X POST \
          -H "Content-Type: application/json" \
          -H "Authorization: Bearer \${DO_TOKEN}" \
          -d "{\"type\":\"assign\",\"droplet_id\":\"$ID\"}" \
          "https://api.digitalocean.com/v2/floating_ips/$IP/actions" || true)

        echo "$(date -Iseconds) assign attempt $((n+1)): $RESPONSE" >> "$LOG_FILE"

        echo "$RESPONSE" | grep -q '"action"' && break
        n=$((n+1))
        sleep 3
    done
fi
EOF

chmod +x /etc/keepalived/master.sh

# Configure Keepalived
cat > /etc/keepalived/keepalived.conf <<EOF
vrrp_script check_nginx {
    script "pidof nginx"
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
echo "Navigate your browser to: http://$VIRTUAL_IP"