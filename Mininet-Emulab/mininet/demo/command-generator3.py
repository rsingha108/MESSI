import subprocess
import time
import numpy as np
	
def commands_gen(as1,as2,lp1,lp2,asp1,asp2,org1,org2,med1,med2):
	with open('generated_commands.txt','w') as g:
	
		g.write(f's1 vtysh -c "conf t" -c "router bgp 65000" -c "neighbor 1.1.1.2 remote-as 65000" -c "neighbor 3.3.3.2 remote-as 50000" -c "network 1.0.0.0" -c "network 3.0.0.0" -c "network 10.0.0.0"\n')
		
		g.write(f's2 vtysh -c "conf t" -c "router bgp 65000" -c "neighbor 1.1.1.1 remote-as 65000" -c "neighbor 2.2.2.2 remote-as 40000" -c "network 1.0.0.0" -c "network 2.0.0.0" -c "network 20.0.0.0"\n')

		g.write('s4 mkdir exabgp\n')

		g.write('s4 cd exabgp\n')
		g.write('s4 echo "process announce-routes{" >> conf.ini\n')
		g.write('s4 echo -e "\\trun python exabgp/example.py;" >> conf.ini\n')
		g.write('s4 echo -e "\\tencoder json;" >> conf.ini\n')
		g.write('s4 echo "}" >> conf.ini\n')


		g.write('s4 echo "neighbor 2.2.2.1{" >> conf.ini\n')
		g.write('s4 echo -e "\\trouter-id 2.2.2.2;" >> conf.ini\n')
		g.write('s4 echo -e "\\tlocal-address 2.2.2.2;" >> conf.ini\n')
		g.write(f's4 echo -e "\\tlocal-as 40000;" >> conf.ini\n')
		g.write('s4 echo -e "\\tpeer-as 65000;" >> conf.ini\n')
		g.write('s4 echo -e "\\tapi{" >> conf.ini\n')
		g.write('s4 echo -e "\\t\\tprocesses [announce-routes];" >> conf.ini\n')
		g.write('s4 echo -e "\\t}" >> conf.ini\n')
		g.write('s4 echo -e "}" >> conf.ini\n')


		g.write('s4 echo "from __future__ import print_function" >> example.py\n')
		g.write('s4 echo "from sys import stdout" >> example.py\n')
		g.write('s4 echo "from time import sleep" >> example.py\n')

		g.write('s4 echo "messages = [" >> example.py\n')
		g.write(f's4 echo -e "\\t\'announce route 100.10.0.0/24 next-hop self local-preference {lp1} as-path {asp1} med {med1} origin {org1}\'," >> example.py\n')
	
		g.write('s4 echo "]" >> example.py\n')

		g.write('s4 echo "sleep(5)" >> example.py\n')

		g.write('s4 echo "for message in messages:" >> example.py\n')
		g.write('s4 printf "\\tstdout.write(message + \'\\\\\\n\')\\n" >> example.py\n')
		g.write('s4 echo -e "\\tstdout.flush()" >> example.py\n')
		g.write('s4 echo -e "\\tsleep(1)" >> example.py\n')

		g.write('s4 echo "while True:" >> example.py\n')
		g.write('s4 echo -e "\\tsleep(1)" >> example.py\n')

		g.write('s4 cd ..\n')
		g.write('py s4.cmd("exabgp exabgp/conf.ini &")\n')
		g.write('py time.sleep(10)\n')
		
		##-------------------------------------------------------------------------------------------------------------------------
		g.write('s3 mkdir exabgp\n')

		g.write('s3 cd exabgp\n')
		g.write('s3 echo "process announce-routes{" >> conf.ini\n')
		g.write('s3 echo -e "\\trun python exabgp/example.py;" >> conf.ini\n')
		g.write('s3 echo -e "\\tencoder json;" >> conf.ini\n')
		g.write('s3 echo "}" >> conf.ini\n')


		g.write('s3 echo "neighbor 3.3.3.1{" >> conf.ini\n')
		g.write('s3 echo -e "\\trouter-id 3.3.3.2;" >> conf.ini\n')
		g.write('s3 echo -e "\\tlocal-address 3.3.3.2;" >> conf.ini\n')
		g.write(f's3 echo -e "\\tlocal-as 50000;" >> conf.ini\n')
		g.write('s3 echo -e "\\tpeer-as 65000;" >> conf.ini\n')
		g.write('s3 echo -e "\\tapi{" >> conf.ini\n')
		g.write('s3 echo -e "\\t\\tprocesses [announce-routes];" >> conf.ini\n')
		g.write('s3 echo -e "\\t}" >> conf.ini\n')
		g.write('s3 echo -e "}" >> conf.ini\n')


		g.write('s3 echo "from __future__ import print_function" >> example.py\n')
		g.write('s3 echo "from sys import stdout" >> example.py\n')
		g.write('s3 echo "from time import sleep" >> example.py\n')

		g.write('s3 echo "messages = [" >> example.py\n')
		g.write(f's3 echo -e "\\t\'announce route 100.10.0.0/24 next-hop self local-preference {lp2} as-path {asp2} med {med2} origin {org2}\'," >> example.py\n')
	
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
		
		g.write('s1 vtysh -c "show ip bgp" > /mnt/out1.txt\n')
		g.write('s1 vtysh -c "show ip bgp" >> /mnt/out_log1.txt\n')
		g.write('s2 vtysh -c "show ip bgp" > /mnt/out2.txt\n')
		g.write('s2 vtysh -c "show ip bgp" >> /mnt/out_log2.txt\n')
		
with open('zen_out.txt','r') as f:
	lines = f.readlines()
		
	
lines = lines[3:-1]	
tests = []
test = ""
for line in lines:
	if line == '\n' : 
		tests.append(test)
		test = ""
	else:
		if line.startswith('(') : 
			line = line[1:-1]
			test += line + ';'
		if line.startswith(',') : 
			line = line[2:-1]
			test += line

			
ct = 0	
g = open('results.json','w')	
g.close()
g = open('out_log.txt','w')	
g.close()

for line in tests:
	ct += 1
	print(f"**** NEW SESSION {ct} ****")
	with open('out_log.txt','a') as g:
		g.write(f"\n\n@@Test case {ct} : {line}\n\n")
	sp = line.split(';')
	as1,lp1,asn1,org1,med1 = sp[0].split(',')
	as2,lp2,asn2,org2,med2 = sp[1].split(',')
	asn1,asn2 = int(asn1), int(asn2)
	asp1 = 	str([485] + list(np.arange(100, asn1*100 + 1, 100)))#'[' + ' '.join(['100']*asn1) + ']'
	asp2 =  str([65000] + list(np.arange(1000, asn2*1000 + 1, 1000)))
	omap = {'i':'igp','e':'egp','?':'incomplete'}
	org1 = omap[org1]
	org2 = omap[org2]
	print(asp1,asp2)
	commands_gen(as1,as2,lp1,lp2,asp1,asp2,org1,org2,med1,med2)
	time.sleep(5)
	args = ["sudo", "python", "exabgp-router3.py"]
	message = 'source generated_commands.txt'
	
	sp = subprocess.run(args,input=message.encode())
	
	#sp1 = subprocess.run(["python", "parser.py"])
	
	

	
		
		

