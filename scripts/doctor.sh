#!/bin/bash

clear

echo "==========================================="
echo "         PROJECT GARUDA DOCTOR"
echo "==========================================="
echo

echo "===== HOSTNAME ====="
hostnamectl
echo

echo "===== CPU ====="
lscpu | egrep 'Model name|CPU\(s\)|Thread|Core|Socket'
echo

echo "===== MEMORY ====="
free -h
echo

echo "===== DISK ====="
df -h
echo

echo "===== PROXMOX VERSION ====="
pveversion
echo

echo "===== UPTIME ====="
uptime
echo

echo "===== LOAD ====="
cat /proc/loadavg
echo

echo "===== NETWORK ====="
ip -br addr
echo

echo "===== DOCKER ====="
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
echo

echo "===== DOCKER IMAGES ====="
docker images
echo

echo "===== DISK USAGE DOCKER ====="
docker system df
echo

echo "===== MEMORY TOP PROCESS ====="
ps aux --sort=-%mem | head
echo

echo "===== CPU TOP PROCESS ====="
ps aux --sort=-%cpu | head
echo

echo "===== LDAP DATA ====="
du -sh /opt/bdip/data/ldap 2>/dev/null
echo

echo "===== POSTGRES DATA ====="
du -sh /opt/bdip/data/postgres 2>/dev/null
echo

echo "===== BACKUP ====="
du -sh /opt/bdip/backup 2>/dev/null
echo

echo "==========================================="
echo " GARUDA DOCTOR REPORT FINISHED"
echo "==========================================="
