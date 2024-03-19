docker compose down

# convert test case to config file
python3 test_to_config.py

# setup the network and start the docker containers
docker compose -f docker-compose.yml up -d

# command to run by forking a new process
command_to_fork(){
    docker exec exabgp_1 bash -c "exabgp exabgp/conf.ini"
}

# forking a new process to run exabgp command
command_to_fork &

# let the parent process sleep so that exabgp can send the route
# and gobgp can update its routing tables
sleep 10

# get the output
docker exec -it gobgp_1 gobgp global rib > out.txt

# stop the containers and shut down the network
docker compose down

# stop all child processes before exiting
pkill -P $$

