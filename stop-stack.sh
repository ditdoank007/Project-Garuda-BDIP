#!/bin/bash

STACKS=(
radius
keycloak
postgres
ldap
portainer
)

echo "========================================="
echo " Stopping BDIP Services"
echo "========================================="

for stack in "${STACKS[@]}"
do
    if [ -f "/opt/bdip/compose/${stack}/compose.yml" ]; then
        echo ""
        echo ">>> Stopping ${stack}"
        docker compose -f /opt/bdip/compose/${stack}/compose.yml down
    fi
done
