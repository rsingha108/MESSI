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


########## ARGUMENTS ##########
parser = argparse.ArgumentParser()
parser.add_argument("--software", type=str, default="frr")
args = parser.parse_args()

sw = args.software
if sw == 'quagga' or sw == 'frr' : 
	cnsl = 'vtysh'
elif sw == 'bird' :
	cnsl = 'birdc'
	
######## HYPERPARAMETERS ########
cs = 4 # container set

########## RUN TOPO ##########

nh1 = "1.1.1.1"
ip1 = "1.1.1.2"
nh4 = "4.4.4.1"
ip4 = "4.4.4.2"
nh3 = "3.3.3.1"
ip3 = "3.3.3.2"

## manual config
# d1 = {"NH" : nh1, "AS" : "43617", "IP" : "10.100.1.0/24", "LP" : "200", "ASP" : "[55]", "MED" : "5", "ORG" : "egp", "COM" : "[2:3]"}
# d2 = {"IP1" : ip1, "IP4" : ip4, "IP3" : ip3, "AS" : "43618"}
# d3 = {"NH" : nh3, "AS" : "43619", "IP" : "10.100.2.0/24", "LP" : "200", "ASP" : "[55]", "MED" : "3", "ORG" : "egp", "COM" : "[2:3]"}
# d4 = {"NH" : nh4, "AS" : "43620"}


# Iterate over JSON files in the tests directory
tests_dir = "./tests"
g = open('results.txt','w')
g.close()
for filename in os.listdir(tests_dir):
       print(f"Running test {filename}")
       file_path = os.path.join(tests_dir, filename)
       with open(file_path, "r") as file:
              data = json.load(file)
       d1, d2, d3, d4, dec = translator.translate(data)
       
       g = open('out.txt','w')
       g.close()

       command_generator.cmd_gen(d1,d2,d3,d4,cs,sw,cnsl)

       args = ["sudo", "python3", "star-topo.py", f"--software={sw}", f"--nh1={nh1}", f"--ip1={ip1}", f"--nh4={nh4}", f"--ip4={ip4}", f"--nh3={nh3}", f"--ip3={ip3}", f'--conset={cs}']
       print(" ".join(args))
       message = 'source generated_commands.txt'
       sp = subprocess.run(args,input=message.encode())


       ### PARSING ###
       actual_decision = dec
       with open('out.txt','r') as f:
              out = f.read()
       out = out.strip()
       router_decision = False
       if "% Network not in table" in out:
              router_decision = False
       else:
              router_decision = True
       with open('results.txt','a') as f:
              f.write(f"{actual_decision},{router_decision}\n")
              

