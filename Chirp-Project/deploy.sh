source ~/.bash_profile

docker compose -f ../vagrant/docker-compose.yml pull
docker compose -f ../vagrant/docker-compose.yml up -d