import subprocess
import time
import numpy as np
from tqdm import tqdm
import command_generator
import argparse
import json
import utils


########## ARGUMENTS ##########
parser = argparse.ArgumentParser()
parser.add_argument("--test_id", type=int, default=-1)
args = parser.parse_args()

print(f"Test ID : {args.test_id}\n")

#sw = args.software
#if sw == 'quagga' or sw == 'frr' : 
#	cnsl = 'vtysh'
#elif sw == 'bird' :
#	cnsl = 'birdc'

sw = 'frr'
cnsl = 'vtysh'

######## HYPERPARAMETERS ########
cs = 1 # container set

########## FIXED CONFIGS ##########
## d2 : router, d1 : exabgp

d2 = {}; d1 = {} 
d2["IP1"],d2["IP2"],d2["AS"] = '7.0.0.1','3.0.0.1','43617'
d1["NH"], d1["AS"], d1["ORG"] = '7.0.0.3','43617', 'egp'
	

######## RUN TESTS ########

g = open(f'results/dynamic_report.txt','w')
g.close()
g = open(f'results/dynamic_err.txt','w')
g.close()

# sp_idx =  191 #405 - set com, 506 - del com,  #1 - As path
# for i in tqdm(range(sp_idx,sp_idx+1)):

# for i in tqdm(range(403,408)):

i = args.test_id
while True:

    if args.test_id == -1:
        i += 1

    patience = 1000 
    while True:
        if patience == 0:
            print("Timeout")
            break
        try:
            with open(f'../tests/{i}.json') as f:
                test = json.load(f)
            break
        except:
            time.sleep(5)
            patience -= 1
            continue

    if patience == 0:
        print("Timeout")
        break

    rmap1, rmap2, route, dec1, dec2 = test["Rmap1"], test["Rmap2"], test["Route"], test["Decision1"], test["Decision2"]
    
    d1["IP"],d1["LP"],d1["MED"],d1["ASP"],d1["COM"] = route["Prefix"], route["LP"], route["MED"], route["ASPath"], route["Community"]

    # print(rmap1,rmap2,route,dec1,dec2)
    
    g = open('out.txt','w')
    g.close()
    g = open('out2.txt','w')
    g.close()
    g = open("running-config.txt","w")
    g.close()
    g = open("running-config2.txt","w")
    g.close()
    gerr = open('err_msg.txt','w')

    #### GENERATE COMMANDS & RUN ####

    command_generator.cmd_gen(d1,d2,cs,sw,cnsl,rmap1,route)
    command_generator.append_rmap_changes(d1,d2,cs,sw,cnsl,rmap2,rmap1)
    cmnd = ["sudo", "python3", "one-router-topo.py", f"--software={sw}", f'--ip1={d2["IP1"]}', f'--nh1={d1["NH"]}', f'--conset={cs}']
    message = 'source generated_commands.txt'
    sp = subprocess.run(cmnd,input=message.encode(),stderr=gerr)
    gerr.close()

    #### PARSING ####

    with open("out.txt","r") as f:
        lines = f.readlines()
    router_decision1 = "True"
    if len(lines) == 1 and lines[0].startswith("% Network not in table"):
        router_decision1 = "False"
    match_decision1 = "yes" ## if decision matching with expected decision
    if router_decision1 != str(dec1):
        match_decision1 = "no"

    with open("out2.txt","r") as f:
        lines = f.readlines()
    router_decision2 = "True"
    if len(lines) == 1 and lines[0].startswith("% Network not in table"):
        router_decision2 = "False"
    match_decision2 = "yes" ## if decision matching with expected decision
    if router_decision2 != str(dec2):
        match_decision2 = "no"

    #### REPORT ####

    with open('results/dynamic_report.txt','a') as g:
        if match_decision1 == "no" or match_decision2 == "no":
            g.write(f"Test Case {i} : {match_decision1},{match_decision2}\n")
            json.dump(test,g,indent=4)
            g.write('\n')
        else:
            g.write(f"Test Case {i} : {match_decision1},{match_decision2}\n")
            g.write('\n')

    ## error message
    with open('err_msg.txt','r') as gerr:
        err_msg = gerr.readlines()
    for line in err_msg:
        if "%" in line:
            with open(f'results/dynamic_err.txt','a') as ge:
                ge.write(f"Test Case {i} : {line}")
            break
            # raise Exception("Error in Configuration!!!")

    if args.test_id != -1:
        break
            
    

    
	
	
	

	
		
		
