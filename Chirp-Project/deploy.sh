source ~/.bash_profile

# Make sure DOCKER_USERNAME is set
export DOCKER_USERNAME=${DOCKER_USERNAME:-nickychengde}

cd ../vagrant

docker compose pull
docker compose up -d
