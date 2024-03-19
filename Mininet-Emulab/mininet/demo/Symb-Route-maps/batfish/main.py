# Import packages
import sys
sys.path.insert(0,'..')
import warnings
import startup
import translate
import logging
import json
import argparse
import random  # noqa: F401
from typing import List, Optional  # noqa: F401

import pandas as pd
import time
from IPython.display import display
from pandas.io.formats.style import Styler

from pybatfish.client.session import Session  # noqa: F401

# noinspection PyUnresolvedReferences
from pybatfish.datamodel import Edge, Interface  # noqa: F401
from pybatfish.datamodel.answer import TableAnswer
from pybatfish.datamodel.flow import HeaderConstraints, PathConstraints  # noqa: F401
from pybatfish.datamodel.route import BgpRoute  # noqa: F401
from pybatfish.util import get_html
from pybatfish.datamodel.route import BgpRouteConstraints
logger = logging.getLogger(__name__)

########## ARGUMENTS ##########
parser = argparse.ArgumentParser()
parser.add_argument("--test_id", type=int, default=-1)
args = parser.parse_args()

print(f"Test ID : {args.test_id}\n")
################################


g = open("results/results.txt","w")
g.close()
g = open("results/err.txt","w")
g.close()

# sp_idx =  0 #405 - set com, 506 - del com,  #1 - AS path
# for i in (range(sp_idx,sp_idx+1)):

# for i in tqdm(range(264,286)):

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
		
	rmap, route, decision = test[0], test[1], test[2]
	
	# if ((rmap["Prefix"] == "None") and (rmap["Community"] == "None") and (rmap["ASPath"] == "None")):
	# 	if ((rmap["LP"] != "None") or (rmap["MED"] != "none")):
	# 		continue
	
	# print(rmap)
	translate.write_rmap(rmap)
	
	logger.warning(f"{i}\n")
	bf = Session(host="localhost")
	# Initialize a network and snapshot
	NETWORK_NAME = "example_network"
	SNAPSHOT_NAME = "example_snapshot"
	SNAPSHOT_PATH = "../networks/route-analysis"
	bf.set_network(NETWORK_NAME)
	bf.init_snapshot(SNAPSHOT_PATH, name=SNAPSHOT_NAME, overwrite=True)

	

	asp = list(map(int,route["ASPath"].split()))
	com = route["Community"].split()

	#print(asp,com)

	# Create an example route to use for testing
	inRoute1 = BgpRoute(network=route["Prefix"], 
		                originatorIp="4.4.4.4", 
		                originType="egp", 
		                protocol="bgp",
		                asPath=asp, #[32,34],
		                communities=com, #["1:0", "22221:1"],
		                localPreference=int(route["LP"]),
		                metric=int(route["MED"]),
						nextHopIp=route["NextHop"])

	# Test how our policy treats this route
	result = bf.q.testRoutePolicies(policies="Rmap", 
		                         direction="in", 
		                         inputRoutes=[inRoute1]).answer().frame()
	# Pretty print the result
	if result["Action"][0] == 'DENY':
		bf_attr = {"LP" : str(result["Input_Route"][0].localPreference), "MED" : str(result["Input_Route"][0].metric), "Community" : list(result["Input_Route"][0].communities), "ASPath" : [str(asd['asns'][0]) for asd in list(result["Input_Route"][0].asPath)], "NextHop" : result["Input_Route"][0].nextHopIp}
		print(f'{i} : DENY')
	else:
		bf_attr = {"LP" : str(result["Output_Route"][0].localPreference), "MED" : str(result["Output_Route"][0].metric), "Community" : list(result["Output_Route"][0].communities), "ASPath" : [str(asd['asns'][0]) for asd in list(result["Output_Route"][0].asPath)], "NextHop" : result["Output_Route"][0].nextHopIp}
		print(f'{i} :',result["Action"][0], bf_attr)
		
	exp_decision = decision['Allowed']
	if ("L2" in decision['Tag']) or ("M2" in decision['Tag']) :
		exp_decision = "False"
	router_decision = "False" if result["Action"][0] == 'DENY' else "True"
	match_decision = "yes" if exp_decision == router_decision else "no"

	unmatched_attr = []
	match_attr = "yes" ## if all attributes matching with expected decision
	if (rmap["Set"] != "None") and (decision["Allowed"] == "True") and router_decision == "True":
		if (bf_attr["LP"]!=decision["LP"]):
			match_attr = "no"
			unmatched_attr.append(f"LP={bf_attr['LP']}")
      	
		if (bf_attr["MED"]!=decision["MED"]):
			match_attr = "no"
			unmatched_attr.append(f"MED={bf_attr['MED']}")

		if (sorted(decision["Community"].split(' '))!=sorted(bf_attr["Community"])):
			match_attr = "no"
			unmatched_attr.append(f"Community={bf_attr['Community']}")

		if (sorted(decision["ASPath"].split(' '))!=sorted(bf_attr["ASPath"])):
			match_attr = "no"
			unmatched_attr.append(f"ASPath={bf_attr['ASPath']}")

		if (bf_attr["NextHop"]!=decision["NextHop"]):
			match_attr = "no"
			unmatched_attr.append(f"NextHop={bf_attr['NextHop']}")


	g = open("results/results.txt","a")
	g.write(f"{i},{match_decision},{match_attr}\n")
	g.close()

	if match_decision == "no" or match_attr == "no":
		with open('results/report.txt','a') as g:
			g.write(f"Test Case {i} : {match_decision},{match_attr}\n")
			json.dump(test,g,indent=4)
			g.write('\n')
			g.write("\nUnmatched Route Attributes : \n")
			g.write('\n'.join(unmatched_attr))
			g.write("\n"+"="*70+"\n")

	if args.test_id != -1:
		break
