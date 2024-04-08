Basic Instructions can be found at Notion : Scale-BGP/Bird-setup

conf : folder containing config files for bird, exabgp

bird-topo.drawio.png  : topology

scripts : contains start.sh which is responsible to start BIRD in peer1

main-driver.py : main script that takes test cases from 'zen_out.txt, creates new 'docker-compose.yaml' file and config files inside 'conf/peer*/etc/bird|exabgp'.
				
				then runs 'main.sh' followed by 'parse_output.py' for every test case. 
				
main.sh : sequence of terminal commands needed to produce output from BIRD i.e. 'bird_output.txt'

parse_output.py : takes 'bird_output.txt' as input and appends result of each test case to 'bird_result.txt'


Run : "python main-driver.py" to get final results.

Special notes :

in docker-compose.yaml "tail -f dev/null" given to run a infinite process (otherwise peer2 & peer3 bash couldn't be open)


