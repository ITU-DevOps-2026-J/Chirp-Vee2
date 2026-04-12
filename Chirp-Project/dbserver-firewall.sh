sudo ufw default deny incoming
sudo ufw default allow outgoing
sudo ufw allow ssh
sudo ufw allow from 104.248.28.105 to any port 5432
sudo ufw allow from 167.172.97.87 to any port 5432
sudo ufw enable
