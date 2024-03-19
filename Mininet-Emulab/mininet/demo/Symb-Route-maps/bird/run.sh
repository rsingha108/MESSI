#!/bin/sh

docker-compose down

docker-compose up -d

#docker compose exec peerb birdc show route

nohup docker-compose exec -T peere exabgp exabgp/conf.ini &

sleep 10 # min 10 needed

docker-compose exec peerb birdc show route all 

docker-compose exec peerb birdc show route all > out.txt

