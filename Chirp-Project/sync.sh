#!/usr/bin/env bash

LB2_IP=$(curl -fsS -H "Authorization: Bearer $DO_TOKEN" \
  "https://api.digitalocean.com/v2/droplets?per_page=200" | \
  python3 -c '
import json, sys
data = json.loads(sys.stdin.read())
for d in data["droplets"]:
    if d["name"] == "itu-minitwit-load-balancer2":
        for net in d["networks"]["v4"]:
            if net["type"] == "public":
                print(net["ip_address"])
' )
rsync -avz -e "ssh -o StrictHostKeyChecking=no" /etc/letsencrypt/ root@$LB2_IP:/etc/letsencrypt/
rsync -avz -e "ssh -o StrictHostKeyChecking=no"/etc/nginx/sites-available/veechirp.app root@$LB2_IP:/etc/nginx/sites-available/veechirp.app
ssh -o StrictHostKeyChecking=no root@$LB2_IP "nginx -t && systemctl reload nginx"