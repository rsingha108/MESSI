"""
Topology :


       subnet1          subnet4
(s1) ----------- (s2) ----------- (s4)
nh1            ip1| ip4		     nh4
                  |ip3
                  |
                  | subnet3
                  |
                  |nh3
                (s3)		
                   
s1 : exabgp
s3 : exabgp
s2 : router (frr/quagga)
s4 : router (frr/quagga)
"""

import os
import sys
sys.path.insert(0,'..')
import subprocess
import time
import numpy as np
from tqdm import tqdm
import command_generator
import argparse
import json
import translator
import helper
import result_parser

########## ARGUMENTS ##########
parser = argparse.ArgumentParser()
parser.add_argument("--software", type=str, default="frr")
args = parser.parse_args()

sw = args.software
cnsl = 'vtysh'

######## HYPERPARAMETERS ########
cs = 5 # container set

nh1 = "1.1.1.1"
ip1 = "1.1.1.2"
nh4 = "4.4.4.1"
ip4 = "4.4.4.2"
nh3 = "3.3.3.1"
ip3 = "3.3.3.2"

## ******* manual config ********
# d1 = {"NH" : nh1, "AS" : "43617", "IP" : "10.100.1.0/24", "LP" : "220", "ASP" : "3", "MED" : "5", "ORG" : "e", "COM" : "[2:3]"}
# d2 = {"IP1" : ip1, "IP4" : ip4, "IP3" : ip3, "AS" : "43618"}
# d3 = {"NH" : nh3, "AS" : "43619", "IP" : "10.100.2.0/24", "LP" : "200", "ASP" : "2", "MED" : "3", "ORG" : "e", "COM" : "[2:3]"}
# d4 = {"NH" : nh4, "AS" : "43620"}

# d1 =  {'AS': "193", 'LP': "177", 'ASP': "1", 'ORG': 'e', 'MED': "142", 'IGP': "4", 'AT': "0", 'RID': '100.0.1.64', 'NH': '1.1.1.1', 'NextHop': '99.190.1.32'}
# d2 =  {'IP1': '1.1.1.2', 'IP3': '3.3.3.2', 'IP4': '4.4.4.2', 'AS': "102"}
# d3 =  {'AS': "189", 'LP': "173", 'ASP': "2", 'ORG': 'e', 'MED': "173", 'IGP': "7", 'AT': "0", 'RID': '99.190.1.3', 'NH': '3.3.3.1', 'NextHop': '99.190.1.3'}
# d4 =  {'NH': '4.4.4.1', 'AS': "101"}
	
## ******* Read from test directory ********
tests_folder = "./tests"
g = open(f'results_{sw}.txt','w')
g.close()
# Iterate over the files in the tests folder
n_tests = len(os.listdir(tests_folder))
for i in range(n_tests):
	filename = f"{i}.json"
	# Read the contents of the JSON file
	with open(os.path.join(tests_folder, filename), "r") as file:
		test = json.load(file)

	subnet_ips = helper.generate_subnet_ips(test)
	nh1, ip1, nh3, ip3, nh4, ip4  = subnet_ips
	print("nh1: ", nh1)
	print("ip1: ", ip1)
	print("nh3: ", nh3)
	print("ip3: ", ip3)
	print("nh4: ", nh4)
	print("ip4: ", ip4)
		
	print(f"\n**** Test case {i} ****")
	
	d1, d2, d3, d4, dec = translator.translate(test, subnet_ips)

	print ("d1: ", d1)
	print ("d2: ", d2)
	print ("d3: ", d3)
	print ("d4: ", d4)
	print ("dec: ", dec)	
# ============================================ #
	aspl1,aspl3 = int(d1["ASP"]), int(d3["ASP"])
	d1["ASP"] =  str(list(np.arange(1000, 1000+aspl1, 1))) ## the s2 router is always AS100 set by Zen. Be careful to avoid that AS number otherwise horizon rule (route won't be accepted)
	d3["ASP"] =  str(list(np.arange(3000, 3000+aspl3, 1)))
	omap = {'i':'igp','e':'egp','?':'incomplete'}
	d1["ORG"] = omap[d1["ORG"]]
	d3["ORG"] = omap[d3["ORG"]]

	command_generator.cmd_gen(d1,d2,d3,d4,cs,sw,cnsl)

	args = ["sudo", "python3", "star-topo.py", f"--software={sw}", f"--nh1={nh1}", f"--ip1={ip1}", f"--nh4={nh4}", f"--ip4={ip4}", f"--nh3={nh3}", f"--ip3={ip3}", f'--conset={cs}']
	print(" ".join(args))
	message = 'source generated_commands.txt'

	sp = subprocess.run(args,input=message.encode())

# ============================================ #

	# Result Parsing
	actual_decision = dec ## 1 or 3
	router_decision = result_parser.parse_rib()
	with open(f'results_{sw}.txt','a') as f:
		f.write(f"{actual_decision},{router_decision}\n")

			


	
	
	
	

	
		
		

