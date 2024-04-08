#!/bin/sh

sudo docker-compose down

sudo docker-compose up -d

sudo docker-compose exec peer1 birdc show route

nohup sudo docker-compose exec -T peer2 exabgp exabgp/conf.ini &

nohup sudo docker-compose exec -T peer3 exabgp exabgp/conf.ini &

sleep 10

sudo docker-compose exec peer1 birdc show route > bird_output.txt
