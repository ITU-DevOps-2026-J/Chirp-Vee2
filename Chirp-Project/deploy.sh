source ~/.bash_profile

cd ../vagrant
chmod +x deploy.sh

docker compose pull
docker compose up -d