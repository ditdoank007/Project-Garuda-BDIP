#!/bin/bash

set -e

STACKS=(
portainer
ldap
postgres
keycloak
radius
)

echo "========================================="
echo " Starting BDIP Services"
echo "========================================="

for stack in "${STACKS[@]}"
do
    if [ -f "/opt/bdip/compose/${stack}/compose.yml" ]; then
        echo ""
        echo ">>> Starting ${stack}"
        docker compose -f /opt/bdip/compose/${stack}/compose.yml up -d
    else
        echo ""
        echo ">>> Skip ${stack} (compose.yml belum ada)"
    fi
done

echo ""
echo "Semua stack selesai diproses."
