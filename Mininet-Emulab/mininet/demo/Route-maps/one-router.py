import subprocess
import time
import numpy as np
from tqdm import tqdm

import argparse
parser = argparse.ArgumentParser()
parser.add_argument("--software", type=str, default="frr")
args = parser.parse_args()

sw = args.software
if sw == 'quagga' or sw == 'frr' : 
	cnsl = 'vtysh'
elif sw == 'bird' :
	cnsl = 'birdc'
	
def commands_gen(d1,d2):
	nw1 = d1["NH"].split('.')[0]+'.0.0.0'
	
	with open('generated_commands.txt','w') as g:
	
		# g.write(f's2 {cnsl} -c "conf t" -c "debug bgp updates" -c "log file /var/log/{sw}/bgpd.log" -c "router bgp {d2["AS"]}" -c "no bgp ebgp-requires-policy" -c "neighbor {d1["NH"]} remote-as {d1["AS"]}" -c "network {nw1}" -c "network 2.0.0.0" -c "neighbor {d1["NH"]} soft-reconfiguration inbound"\n') # -c "debug bgp updates", -c "log file /var/log/frr/bgpd.log", -c "bgp bestpath compare-routerid"
		g.write(f's2 {cnsl} -c "conf t" -c "debug bgp updates" -c "router bgp {d2["AS"]}" -c "neighbor {d1["NH"]} remote-as {d1["AS"]}" -c "network {nw1}" -c "network 2.0.0.0" -c "neighbor {d1["NH"]} soft-reconfiguration inbound"\n') 

		g.write('s1 mkdir exabgp\n')

		g.write('s1 cd exabgp\n')
		g.write('s1 echo "process announce-routes{" >> conf.ini\n')
		g.write('s1 echo -e "\\trun python exabgp/example.py;" >> conf.ini\n')
		g.write('s1 echo -e "\\tencoder json;" >> conf.ini\n')
		g.write('s1 echo "}" >> conf.ini\n')


		g.write(f's1 echo "neighbor {d2["IP1"]}')
		g.write('{" >> conf.ini\n')
		g.write(f's1 echo -e "\\trouter-id {d1["NH"]};" >> conf.ini\n')
		g.write(f's1 echo -e "\\tlocal-address {d1["NH"]};" >> conf.ini\n')
		g.write(f's1 echo -e "\\tlocal-as {d1["AS"]};" >> conf.ini\n')
		g.write(f's1 echo -e "\\tpeer-as {d2["AS"]};" >> conf.ini\n')
		g.write('s1 echo -e "\\tapi{" >> conf.ini\n')
		g.write('s1 echo -e "\\t\\tprocesses [announce-routes];" >> conf.ini\n')
		g.write('s1 echo -e "\\t}" >> conf.ini\n')
		g.write('s1 echo -e "}" >> conf.ini\n')


		g.write('s1 echo "from __future__ import print_function" >> example.py\n')
		g.write('s1 echo "from sys import stdout" >> example.py\n')
		g.write('s1 echo "from time import sleep" >> example.py\n')

		g.write('s1 echo "messages = [" >> example.py\n')
		g.write(f's1 echo -e "\\t\'announce route {d1["IP"]} next-hop self local-preference {d1["LP"]} as-path {d1["ASP"]} med {d1["MED"]} origin {d1["ORG"]} community [{d1["COM"]}]\'," >> example.py\n')
	
		g.write('s1 echo "]" >> example.py\n')

		g.write('s1 echo "sleep(5)" >> example.py\n')

		g.write('s1 echo "for message in messages:" >> example.py\n')
		g.write('s1 printf "\\tstdout.write(message + \'\\\\\\n\')\\n" >> example.py\n')
		g.write('s1 echo -e "\\tstdout.flush()" >> example.py\n')
		g.write('s1 echo -e "\\tsleep(1)" >> example.py\n')

		g.write('s1 echo "while True:" >> example.py\n')
		g.write('s1 echo -e "\\tsleep(1)" >> example.py\n')

		g.write('s1 cd ..\n')
		g.write('py s1.cmd("exabgp exabgp/conf.ini &")\n')
		g.write('py time.sleep(10)\n')
		# g.write(f's2 {cnsl} -c "show ip bgp" >> /mnt/Route-maps/out_log_{sw}.txt\n')

		# g.write('s2 vtysh -c "conf t" -c "access-list 50 permit 100.10.0.0/24"\n')
		# g.write('s2 vtysh -c "conf t" -c "route-map RM1 deny 10" -c "match ip address 50"\n')
		# g.write('s2 vtysh -c "conf t" -c "route-map RM1 permit 20"\n')
		# g.write('s2 vtysh -c "conf t" -c "route-map RM3 deny 10" -c "match local-preference 135"\n')
		# g.write('s2 vtysh -c "conf t" -c "route-map RM3 permit 20"\n')

		with open('rmconfig.txt','r') as rmfile: 
			statements = rmfile.readlines()
		for statement in statements:
			g.write(f's2 vtysh -c "conf t" {statement}\n')

		with open('rmap.txt','r') as f:
			lines = f.readlines()	

		g.write(f's2 vtysh -c "conf t" -c "router bgp {d2["AS"]}" -c "neighbor {d1["NH"]} route-map {lines[0][:-1]} in" -c "end" -c "exit"\n')
		g.write('s2 vtysh -c "clear ip bgp * soft"\n')
		g.write('py time.sleep(5)\n')
		g.write(f's2 {cnsl} -c "show ip bgp" >> /mnt/Route-maps/out.txt\n')
		g.write(f's2 {cnsl} -c "show ip bgp" >> /mnt/Route-maps/out_log_{sw}.txt\n')
		# g.write(f's2 cat /var/log/{sw}/bgpd.log\n')

		
		
		
with open('routes.txt','r') as f:
	lines = f.readlines()		
lines = lines[1:]	
tests = []
for line in lines:
	tests.append(line[:-1])
print("tests : \n",tests)

g = open(f'out_log_{sw}.txt','w')
g.close()
g = open(f'results/results_{sw}.txt','w')
g.close()
g = open(f'results/exp_{sw}.txt','w')
g.close()
# g = open(f'out.txt','w')
# g.close()

args = ["python", "rmap2config.py", f"--software={sw}"]	
sp = subprocess.run(args)

ct = 0	
PD = {'True':'permit', 'False':'deny'}
for line in tqdm(tests):
	ct += 1
	print(f"\n**** ROUTE : {ct} ****")
	#print("\nline : ",line)
	
	d2 = {}
	d1 = {}
	d2["IP1"],d2["IP2"],d2["AS"] = '7.0.0.1','3.0.0.1','43617'
	#AS Number, Local Preference, AS_Path Length, Origin, MED, IGP Metric, Send/Arrival Time, Router ID, Neighbor Address
	d1["IP"],d1["LP"],d1["MED"],d1["ASP"],d1["COM"], decision = line.split(',')
	d1["NH"], d1["AS"], d1["ORG"] = '7.0.0.3','43617', 'egp'
	d1["ASP"] = "[" + d1["ASP"] + "]"

	with open(f'out_log_{sw}.txt','a') as g:
		g.write(f"{line}\n")
		g.write(f"Expected decision : {PD[decision]}\n\n")

	with open(f'out.txt','w') as g:
		g.write(f"{line}\n")
		g.write(f"Expected decision : {PD[decision]}\n\n")

	commands_gen(d1,d2)
	time.sleep(5)
	args = ["sudo", "python", "one-router-topo.py", f"--software={sw}", f'--ip1={d2["IP1"]}', f'--nh1={d1["NH"]}']
	print(" ".join(args))
	message = 'source generated_commands.txt'
	sp = subprocess.run(args,input=message.encode())

	## Parsing
	with open("out.txt","r") as f:
		lines = f.readlines()
	sp = lines[0][:-1].split(',')
	ip_addr, exp_decision = sp[0], PD[sp[-1]]
	ip_split = ip_addr.split('/')
	msk = int(ip_split[1])
	ip_addr = '.'.join(ip_split[0].split('.')[:(msk//8)]) + ".0"*(4-msk//8)
	router_decision = 'deny'
	with open(f'results/results_{sw}.txt','a') as g:
		for line in lines:
			if not line.startswith('*>') : continue
			if ip_addr in line :
				router_decision = 'permit'
		g.write(f"{router_decision}\n")
	with open(f"results/exp_{sw}.txt",'a') as g:
		g.write(exp_decision+'\n')

	
	
	

	
		
		
