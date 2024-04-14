# convert test case to config file
python3 test_to_config.py

# command to run by forking a new process
command_to_fork(){
    docker exec exabgp_1 bash -c "exabgp exabgp/conf.ini"
}

FILE=gobgp1/gobgp.yml

if [[ -f "$FILE" ]];
then
    # File found, building the network
    echo "Running docker-compose"

    # setup the network and start the docker containers
    docker-compose -f docker-compose.yml up -d
    
    # forking a new process to run exabgp command
    command_to_fork &

    # let the parent process sleep so that exabgp can send the route
    # and gobgp can update its routing tables
    sleep 10

    docker ps
    docker exec -it gobgp_1 gobgp --version
    # log the configs
    docker exec -it gobgp_1 gobgp policy > config_logs.txt
    docker exec -it gobgp_1 gobgp policy prefix >> config_logs.txt
    docker exec -it gobgp_1 gobgp policy as-path >> config_logs.txt
    docker exec -it gobgp_1 gobgp policy community >> config_logs.txt

    # check the routing information base
    docker exec -it gobgp_1 gobgp global rib > out.txt

    # stop the containers and shut down the network
    docker-compose down

    # remove the gobgp configuration
    rm -rf gobgp1/gobgp.yml

    # stop all child processes before exiting
    pkill -P $$

    echo "Successfully exited docker-compose"
else
    # Error while creating the config file
    echo "Error: gobgp.yml file not generated" > out.txt
fi
