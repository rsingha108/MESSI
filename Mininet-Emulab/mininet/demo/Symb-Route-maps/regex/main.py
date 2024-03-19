# Import packages
import sys
sys.path.insert(0,'..')
import warnings
import startup
import translate
import logging
import json
import random  # noqa: F401
from typing import List, Optional  # noqa: F401

import pandas as pd
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


g = open("results/results.txt","w")
g.close()
g = open("results/err.txt","w")
g.close()

with open('regex-ex.txt','r') as f:
	rgx_lines = f.readlines()

for i in range(len(rgx_lines)):
	
	rgx_line = rgx_lines[i].strip()
	sp = rgx_line.split(',')
	rgx, _com, res = sp[0], sp[1], sp[2]
		
	with open(f'test.json') as f:
		test = json.load(f)
	rmap, route, decision = test[0], test[1], test[2]
	
	rmap["Community-regex"] = rgx
	route["Community"] = _com
	decision["Allowed"] = "True" if res=="P" else "False"
	
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
		                metric=int(route["MED"]))

	# Test how our policy treats this route
	result = bf.q.testRoutePolicies(policies="Rmap", 
		                         direction="in", 
		                         inputRoutes=[inRoute1]).answer().frame()
	# Pretty print the result
	if result["Action"][0] == 'DENY':
		bf_attr = {"LP" : str(result["Input_Route"][0].localPreference), "MED" : str(result["Input_Route"][0].metric), "Community" : list(result["Input_Route"][0].communities)}
		print(f'{i} : DENY')
	else:
		bf_attr = {"LP" : str(result["Output_Route"][0].localPreference), "MED" : str(result["Output_Route"][0].metric), "Community" : list(result["Output_Route"][0].communities)}
		print(f'{i} :',result["Action"][0], bf_attr)
		
	exp_decision = decision['Allowed']
	if ("L2" in decision['Tag']) or ("M2" in decision['Tag']) :
		exp_decision = "False"
	router_decision = "False" if result["Action"][0] == 'DENY' else "True"
	match_decision = "yes" if exp_decision == router_decision else "no"

	match_attr = "yes" ## if all attributes matching with expected decision
	if (rmap["Set"] != "None") and (decision["Allowed"] == "True") and router_decision == "True":
		if (bf_attr["LP"]!=decision["LP"] or bf_attr["MED"]!=decision["MED"] or sorted(decision["Community"].split(' '))!=sorted(bf_attr["Community"])):
			match_attr = "no"


	g = open("results/results.txt","a")
	g.write(f"{i},{match_decision},{match_attr}\n")
	g.close()
