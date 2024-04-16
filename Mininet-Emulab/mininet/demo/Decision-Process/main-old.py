import subprocess
import time
import numpy as np
from tqdm import tqdm

sw = 'frr'
if sw == 'quagga' or sw == 'frr' : 
	cnsl = 'vtysh'
elif sw == 'bird' :
	cnsl = 'birdc'

def decision_maker(d1,d2,d3):
	if d1["AS"] != d2["AS"] : lp1 = 100
	else: lp1 = int(d1["LP"])
	if d3["AS"] != d2["AS"] : lp3 = 100
	else: lp3 = int(d3["LP"])
	
	oval = {'i':3,'e':2,'?':1}
	o1 = oval[d1["ORG"]]
	o3 = oval[d3["ORG"]]	
	
	n1 = int(''.join(d1["NH"].split('.')))
	n3 = int(''.join(d3["NH"].split('.')))
	
	l1 = [lp1, -1*int(d1["ASP"]), o1, -1*int(d1["MED"]), -1*n1]
	l3 = [lp3, -1*int(d3["ASP"]), o3, -1*int(d3["MED"]), -1*n3] 
	#print(l1,l3)

	for i in range(len(l1)):
		if l1[i] != l3[i] :
			if l1[i] > l3[i] : return 1
			else : return 3
	
def commands_gen(d1,d2,d3):
	nw1 = d1["NH"].split('.')[0]+'.0.0.0'
	nw2 = d3["NH"].split('.')[0]+'.0.0.0'
	
	with open('generated_commands.txt','w') as g:
	
		## The Router Configuration Commands
		g.write(f's2 {cnsl} -c "conf t" -c "debug bgp updates" -c "log file /var/log/frr/bgpd.log" -c "router bgp {d2["AS"]}" -c "no bgp ebgp-requires-policy" -c "neighbor {d1["NH"]} remote-as {d1["AS"]}" -c "neighbor {d3["NH"]} remote-as {d3["AS"]}" -c "network {nw1}" -c "network 2.0.0.0" -c "network {nw2}"\n') # -c "debug bgp updates", -c "log file /var/log/frr/bgpd.log", -c "bgp bestpath compare-routerid"

		## ExaBGP 1 Configuration Commands
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
		g.write(f's1 echo -e "\\t\'announce route 100.10.0.0/24 next-hop self local-preference {d1["LP"]} as-path {d1["ASP"]} med {d1["MED"]} origin {d1["ORG"]}\'," >> example.py\n')
	
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
		g.write(f's2 {cnsl} -c "show ip bgp" >> /mnt/Preference-order/out_log_{sw}.txt\n')
		
		## ExaBGP 3 Configuration Commands
		g.write('s3 mkdir exabgp\n')

		g.write('s3 cd exabgp\n')
		g.write('s3 echo "process announce-routes{" >> conf.ini\n')
		g.write('s3 echo -e "\\trun python exabgp/example.py;" >> conf.ini\n')
		g.write('s3 echo -e "\\tencoder json;" >> conf.ini\n')
		g.write('s3 echo "}" >> conf.ini\n')

		g.write(f's3 echo "neighbor {d2["IP2"]}')
		g.write('{" >> conf.ini\n')
		g.write(f's3 echo -e "\\trouter-id {d3["NH"]};" >> conf.ini\n')
		g.write(f's3 echo -e "\\tlocal-address {d3["NH"]};" >> conf.ini\n')
		g.write(f's3 echo -e "\\tlocal-as {d3["AS"]};" >> conf.ini\n')
		g.write(f's3 echo -e "\\tpeer-as {d2["AS"]};" >> conf.ini\n')
		g.write('s3 echo -e "\\tapi{" >> conf.ini\n')
		g.write('s3 echo -e "\\t\\tprocesses [announce-routes];" >> conf.ini\n')
		g.write('s3 echo -e "\\t}" >> conf.ini\n')
		g.write('s3 echo -e "}" >> conf.ini\n')

		g.write('s3 echo "from __future__ import print_function" >> example.py\n')
		g.write('s3 echo "from sys import stdout" >> example.py\n')
		g.write('s3 echo "from time import sleep" >> example.py\n')

		g.write('s3 echo "messages = [" >> example.py\n')
		g.write(f's3 echo -e "\\t\'announce route 100.10.0.0/24 next-hop self local-preference {d3["LP"]} as-path {d3["ASP"]} med {d3["MED"]} origin {d3["ORG"]}\'," >> example.py\n')
	
		g.write('s3 echo "]" >> example.py\n')

		g.write('s3 echo "sleep(5)" >> example.py\n')

		g.write('s3 echo "for message in messages:" >> example.py\n')
		g.write('s3 printf "\\tstdout.write(message + \'\\\\\\n\')\\n" >> example.py\n')
		g.write('s3 echo -e "\\tstdout.flush()" >> example.py\n')
		g.write('s3 echo -e "\\tsleep(1)" >> example.py\n')

		g.write('s3 echo "while True:" >> example.py\n')
		g.write('s3 echo -e "\\tsleep(1)" >> example.py\n')

		g.write('s3 cd ..\n')
		g.write('py s3.cmd("exabgp exabgp/conf.ini &")\n')
		g.write('py time.sleep(10)\n')
		g.write(f's2 {cnsl} -c "show ip bgp" > /mnt/Preference-order/out.txt\n')
		g.write(f's2 {cnsl} -c "show ip bgp" >> /mnt/Preference-order/out_log_{sw}.txt\n')		

		
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

#print("tests : \n",tests)

g = open(f'out_log_{sw}.txt','w')
g.close()
g = open(f'results_{sw}.txt','w')
g.close()
g = open('correct.txt','w')
g.close()

			
ct = 0	
for line in tqdm(tests[11:12]):
	ct += 1
	print(f"\n**** NEW SESSION {ct} ****")
	#print("\nline : ",line)
	with open(f'out_log_{sw}.txt','a') as g:
		g.write(f"\n\n@@Test case {ct} : {line}\n\n")
	sp = line.split(';')
	d2 = {}
	d1 = {}
	d3 = {}
	d2["IP1"],d2["IP2"],d2["AS"] = sp[0].split(',')
	#AS Number, Local Preference, AS_Path Length, Origin, MED, IGP Metric, Send/Arrival Time, Router ID, Neighbor Address
	d1["AS"],d1["LP"],d1["ASP"],d1["ORG"],d1["MED"],d1["IGP"],d1["AT"],d1["RID"],d1["NH"] = sp[1].split(',')
	d3["AS"],d3["LP"],d3["ASP"],d3["ORG"],d3["MED"],d3["IGP"],d3["AT"],d3["RID"],d3["NH"] = sp[2].split(',')
	x = decision_maker(d1,d2,d3)
	with open('correct.txt','a') as g:
		if x == 1 : g.write(d1["NH"]+'\n')
		elif x == 3 : g.write(d3["NH"]+'\n')
	asn1,asn3 = int(d1["ASP"]), int(d3["ASP"])
	d1["ASP"] =  str(list(np.arange(100, asn1*100 + 1, 100)))#'[' + ' '.join(['100']*asn1) + ']'
	d3["ASP"] =  str(list(np.arange(1000, asn3*1000 + 1, 1000)))
	omap = {'i':'igp','e':'egp','?':'incomplete'}
	d1["ORG"] = omap[d1["ORG"]]
	d3["ORG"] = omap[d3["ORG"]]

	commands_gen(d1,d2,d3)
	time.sleep(5)
	args = ["sudo", "python", "three-routers.py", f"--software={sw}", f'--ip1={d2["IP1"]}', f'--ip2={d2["IP2"]}', f'--nh1={d1["NH"]}', f'--nh2={d3["NH"]}']
	print(" ".join(args))
	message = 'source generated_commands.txt'
	
	sp = subprocess.run(args,input=message.encode())
	
	sp1 = subprocess.run(["python", "parser.py", f"--software={sw}"])
	
	

	
		
		

