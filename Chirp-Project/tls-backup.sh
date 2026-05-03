#!/usr/bin/env bash

cat > /etc/nginx/sites-available/veechirp.app <<'EOF'
server {
    listen 80;
    listen [::]:80;

    server_name veechirp.app;

    location / {
        proxy_pass http://localhost:8080;
        proxy_set_header Host $http_host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;

        # proxy_http_version 1.1;
        proxy_set_header X-URIScheme https;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
    }
}
EOF

sudo ln -sf /etc/nginx/sites-available/veechirp.app /etc/nginx/sites-enabled/veechirp.app

sudo nginx -t
sudo systemctl restart nginx

sudo apt update
sudo apt install python3 python3-dev python3-venv libaugeas-dev gcc -y

sudo ufw status
sudo ufw allow 'Nginx Full'
sudo ufw delete allow 'Nginx HTTP'
sudo ufw allow ssh
sudo ufw status

