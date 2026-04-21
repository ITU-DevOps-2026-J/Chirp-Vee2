LB2_IP=$(cat ../vagrant/loadbalancer_ip.txt)
rsync -avz -e "ssh -o StrictHostKeyChecking=no" /etc/letsencrypt/ root@$LB2_IP:/etc/letsencrypt/
rsync -avz -e "ssh -o StrictHostKeyChecking=no"/etc/nginx/sites-available/veechirp.app root@$LB2_IP:/etc/nginx/sites-available/veechirp.app
ssh -o StrictHostKeyChecking=no root@$LB2_IP "nginx -t && systemctl reload nginx"