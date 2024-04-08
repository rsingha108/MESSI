
from tqdm import tqdm

import subprocess
import time
import numpy as np

### Read test cases from Zen output ###

with open('zen_out.txt','r') as f:
	lines = f.readlines()
		
	
lines = lines[5:-1]	
print(lines[-1])
tests = []
test = []
for line in lines:
	if line == '\n' : 
		tests.append(';'.join(test))
		test = []
	else:
		if line.startswith('(') : 
			line = line[1:-1]
			test.append(line)
		if line.startswith(',') : 
			line = line[2:-1]
			test.append(line)

#print(tests)

g = open('bird_result.txt','w')
g.close()

ct = 0	
for line in tqdm(tests):
	ct += 1
	print(f"\n****  TEST CASE - {ct} ****")
	
	sp = line.split(';')
	d2 = {}; d1 = {};  d3 = {}
	sp[0] = sp[0].replace("0.0.1","0.0.2") ## because 0.0.1 is not supported
	d1["IP1"],d1["IP2"],d1["AS"] = sp[0].split(',') ## ip and AS of BIRD router
	#AS Number, Local Preference, AS_Path Length, Origin, MED, IGP Metric, Send/Arrival Time, Router ID, Neighbor Address
	d2["AS"],d2["LP"],d2["ASP"],d2["ORG"],d2["MED"],d2["IGP"],d2["AT"],d2["RID"],d2["NH"] = sp[1].split(',')
	d3["AS"],d3["LP"],d3["ASP"],d3["ORG"],d3["MED"],d3["IGP"],d3["AT"],d3["RID"],d3["NH"] = sp[2].split(',')
	asn2,asn3 = int(d2["ASP"]), int(d3["ASP"])
	d2["ASP"] =  str(list(np.arange(100, asn2*100 + 1, 100)))#'[' + ' '.join(['100']*asn1) + ']'
	d3["ASP"] =  str(list(np.arange(1000, asn3*1000 + 1, 1000)))
	omap = {'i':'igp','e':'egp','?':'incomplete'}
	d2["ORG"] = omap[d2["ORG"]]
	d3["ORG"] = omap[d3["ORG"]]
	
	### Create a new docker-compose.yaml file ###
	print("Create a new docker-compose.yaml file...")
	
	with open('docker-compose-modified-working.yaml','r') as f:
		sents = f.readlines()
	with open('docker-compose.yaml','w') as g:
		for i,sent in enumerate(sents) :
			sn1 = d1["IP1"].split('.')[0]+".0.0.0"
			sn2 = d1["IP2"].split('.')[0]+".0.0.0"
			if i == 11 : g.write(f'        ipv4_address: {d1["IP1"]}\n')
			elif i == 13 : g.write(f'        ipv4_address: {d1["IP2"]}\n')
			elif i == 23 : g.write(f'        ipv4_address: {d2["NH"]}\n')
			elif i == 35 : g.write(f'        ipv4_address: {d3["NH"]}\n')
			elif i == 44 : g.write(f"       - subnet: '{sn1}/24'\n")
			elif i == 49 : g.write(f"       - subnet: '{sn2}/24'\n")
			else : g.write(sent)
	
	### Create new config files ###
	print("Create new config files...")
	
	## peer1 bird.conf
	print("peer1 bird.conf...")
	
	with open('./conf/peer1/etc/bird/bird.working.conf','r') as f:
		sents = f.readlines()
	with open('./conf/peer1/etc/bird/bird.conf','w') as g:
		for i,sent in enumerate(sents) :
			if i == 13 : g.write(f"  local as {d1['AS']};\n")
			elif i == 14 : g.write(f"  neighbor {d2['NH']} as {d2['AS']};\n")
			elif i == 19 : g.write(f"  local as {d1['AS']};\n")
			elif i == 20 : g.write(f"  neighbor {d3['NH']} as {d3['AS']};\n")
			else : g.write(sent)
			
	## peer2 exabgp conf.ini
	print("peer2 exabgp conf.ini...")
	
	with open('./conf/peer2/etc/exabgp/conf.working.ini','r') as f:
		sents = f.readlines()
	with open('./conf/peer2/etc/exabgp/conf.ini','w') as g:
		for i,sent in enumerate(sents) :
			if i == 5 : g.write(f"neighbor {d1['IP1']} {{\n")
			elif i == 6 : g.write(f"    router-id {d2['NH']};\n")
			elif i == 7 : g.write(f"    local-address {d2['NH']};\n")
			elif i == 8 : g.write(f"    local-as {d2['AS']};\n")
			elif i == 9 : g.write(f"    peer-as {d1['AS']};\n")
			else : g.write(sent)
			
	## peer2 exabgp example.py
	print("peer2 exabgp example.py...")
	
	with open('./conf/peer2/etc/exabgp/example.working.py','r') as f:
		sents = f.readlines()
	with open('./conf/peer2/etc/exabgp/example.py','w') as g:
		for i,sent in enumerate(sents) :
			if i == 7 : g.write(f"    'announce route 100.10.0.0/24 next-hop self local-preference {d2['LP']} as-path {d2['ASP']} med {d2['MED']} origin {d2['ORG']}',\n")
			else : g.write(sent)	
			
	## peer3 exabgp conf.ini
	print("peer3 exabgp conf.ini...")
	
	with open('./conf/peer3/etc/exabgp/conf.working.ini','r') as f:
		sents = f.readlines()
	with open('./conf/peer3/etc/exabgp/conf.ini','w') as g:
		for i,sent in enumerate(sents) :
			if i == 5 : g.write(f"neighbor {d1['IP2']} {{\n")
			elif i == 6 : g.write(f"    router-id {d3['NH']};\n")
			elif i == 7 : g.write(f"    local-address {d3['NH']};\n")
			elif i == 8 : g.write(f"    local-as {d3['AS']};\n")
			elif i == 9 : g.write(f"    peer-as {d1['AS']};\n")
			else : g.write(sent)	
			
	## peer3 exabgp example.py
	print("peer3 exabgp example.py...")
	
	with open('./conf/peer3/etc/exabgp/example.working.py','r') as f:
		sents = f.readlines()
	with open('./conf/peer3/etc/exabgp/example.py','w') as g:
		for i,sent in enumerate(sents) :
			if i == 7 : g.write(f"    'announce route 100.10.0.0/24 next-hop self local-preference {d3['LP']} as-path {d3['ASP']} med {d3['MED']} origin {d3['ORG']}',\n")
			else : g.write(sent)	
			
	print("running main.sh...\n")
	subprocess.run(["sudo", "bash", "main.sh"])
	
	print("running parser...\n")
	subprocess.run(["python", "parse_output.py"])
	
	print("Done!")
	
			
			
			
			
