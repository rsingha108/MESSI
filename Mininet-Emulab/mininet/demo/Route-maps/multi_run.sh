#!/bin/bash
counter=1
> results/multi_run_result.txt
while [ $counter -le 50 ]
do
    echo "Run #$counter"
    # cd ~/Desktop/BGP_Zen/BGP
    cd /home/rathin/Desktop/BGP_Zen/BGP
    dotnet run
    # cd ~/Desktop/Mininet-Emulab/mininet/demo/Route-maps
    cd /home/rathin/Desktop/Mininet-Emulab/mininet/demo/Route-maps
    sudo python one-router.py --software=frr
    sudo python one-router.py --software=quagga
    cd results
    python find_error.py
    ((counter++))
done



