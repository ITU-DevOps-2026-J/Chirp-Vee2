#!/bin/bash
set -e

echo "========================================="
echo "Joining Docker Swarm as Worker..."
echo "========================================="

# Wait for the manager to be ready and the join script to be available
sleep 60

MANAGER_IP=$(cat minitwit1_ip-test.txt)

if [[ -f /vagrant/swarm-tokens/join_worker_$MANAGER_IP.sh ]]; then
    chmod +x /vagrant/swarm-tokens/join_worker_$MANAGER_IP.sh
    bash /vagrant/swarm-tokens/join_worker_$MANAGER_IP.sh
    echo "Successfully joined the swarm!"
else
    echo "Error: Join script not found at /vagrant/swarm-tokens/join_worker_$MANAGER_IP.sh"
    exit 1
fi