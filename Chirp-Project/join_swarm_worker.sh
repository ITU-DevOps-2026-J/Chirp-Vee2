#!/bin/bash
set -e

echo "========================================="
echo "Joining Docker Swarm as Worker..."
echo "========================================="

# Wait for the manager to be ready and the join script to be available
sleep 60

if [ -f /vagrant/swarm-tokens/join_worker.sh ]; then
    bash /vagrant/swarm-tokens/join_worker_$MANAGER_STACK_NAME.sh
    echo "Successfully joined the swarm!"
else
    echo "Error: Join script not found at /vagrant/swarm-tokens/join_worker_$MANAGER_STACK_NAME.sh"
    exit 1
fi