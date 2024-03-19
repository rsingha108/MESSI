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

g = open(f'results/results.txt','w')
g.close()
g = open(f'results/err.txt','w')
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
            with open(f'../tests2/{i}.json') as f:
                test = json.load(f)
            break
        except:
            time.sleep(5)
            patience -= 1
            continue

    if patience == 0:
        print("Timeout")
        break

    rmap, route, decision = test[0], test[1], test[2]
    
    d1["IP"],d1["LP"],d1["MED"],d1["ASP"],d1["COM"] = route["Prefix"], route["LP"], route["MED"], route["ASPath"], route["Community"]
    
    g = open('out.txt','w')
    g.close()
    gerr = open('err_msg.txt','w')

    command_generator.cmd_gen(d1,d2,cs,sw,cnsl,rmap,route)
    time.sleep(5)
    cmnd = ["sudo", "python3", "one-router-topo.py", f"--software={sw}", f'--ip1={d2["IP1"]}', f'--nh1={d1["NH"]}', f'--conset={cs}']
    message = 'source generated_commands.txt'
    sp = subprocess.run(cmnd,input=message.encode(),stderr=gerr)
    gerr.close()

    #### PARSING ####

    with open("out.txt","r") as f:
        lines = f.readlines()
    
    ip_addr, exp_decision = d1["IP"], decision["Allowed"]
    ip_split = ip_addr.split('/')
    msk = int(ip_split[1])
    ip_addr = utils.RewriteIP(ip_addr)
   
    router_decision = "True"
    if len(lines) == 1 and lines[0].startswith("% Network not in table"):
        router_decision = "False"

    LogLP = LogMED = LogCom = LogAS = LogNH = "None"
    if len(lines) > 1:
        for line in lines:
            if "localpref" in line:
                LogLP = line.split('localpref ')[1].split(',')[0] ## LogX = updated value of attribute X from FRR
                # print(decision["LP"]==LogLP)
            if "metric" in line:
                LogMED = line.split('metric ')[1].split(',')[0]
                # print(decision["MED"]==LogMED)
            if "Community" in line:
                LogCom = line.split('Community: ')[1][:-1]
                # print(decision["Community"]==LogCom)
        LogAS = ' '.join(lines[3].split())
        # print(LogAS)
        LogNH = lines[4].split()[0]
        # print(LogNH) 

    if decision["NextHop"] == "0":
        decision["NextHop"] = d1["NH"]

    unmatched_attr = []
    match_attr = "yes" ## if all attributes matching with expected decision
    if (rmap["Set"] != "None") and (decision["Allowed"] == "True") and router_decision == "True":
        ## sort community
        if LogLP!=decision["LP"]:
            match_attr = "no"
            unmatched_attr.append(f"LP={LogLP}")
        if LogMED!=decision["MED"]: 
            match_attr = "no"
            unmatched_attr.append(f"MED={LogMED}")
        if LogCom!=utils.SortNTransformCom(decision["Community"]):
            match_attr = "no"
            unmatched_attr.append(f"Community={LogCom}")
        if LogAS!=decision["ASPath"]:
            match_attr = "no"
            unmatched_attr.append(f"ASPath={LogAS}")
        if LogNH!=decision["NextHop"]:
            match_attr = "no"
            unmatched_attr.append(f"NextHop={LogNH}")

    match_decision = "yes" ## if decision matching with expected decision
    if router_decision != exp_decision:
         match_decision = "no"

    with open(f'results/results.txt','a') as g:
        g.write(f"{i},{match_decision},{match_attr}\n")

    if match_decision == "no" or match_attr == "no":
        with open('results/report.txt','a') as g:
            g.write(f"Test Case {i} : {match_decision},{match_attr}\n")
            json.dump(test,g,indent=4)
            g.write('\n')
            g.write("\nUnmatched Route Attributes : \n")
            g.write('\n'.join(unmatched_attr))
            g.write("\n"+"="*70+"\n")

    ## error message
    with open('err_msg.txt','r') as gerr:
        err_msg = gerr.readlines()
    for line in err_msg:
        if "%" in line:
            with open(f'results/err.txt','a') as ge:
                ge.write(f"Test Case {i} : {line}")
            break
            # raise Exception("Error in Configuration!!!")

    if args.test_id != -1:
        break
            
    

    
	
	
	

	
		
		
