import subprocess
import time
import numpy as np
	
	
"""
Any one of them must be server otherwise base case. (C,C,C), (C,C,NC), (C,NC,C) ... all 8 cases merge to 1
All of them can't be server, at max 2 can be server. If there are 2 servers, third node should be client to both
So first divide into two category : 1 server, 2 servers
if 2 servers : other one is client to both --> 3 possible cases
if one server (3C1) : other two can be both client, one client, one non-client --> 3 * 3 = 9 cases
base case --> 1
13 possible cases


"""
def commands_gen(s1,s2,s3):
		
	with open('commands-rr-scc.txt','w') as g:
	
		g.write(f's2 vtysh -c "conf t" -c "router bgp 500" -c "neighbor 1.1.1.1 remote-as 500" -c "neighbor 2.2.2.1 remote-as 500" -c "network 10.0.0.0" -c "network 2.0.0.0" -c "network 1.0.0.0"\n')
		g.write('s1 vtysh -c "conf t" -c "router bgp 500" -c "neighbor 1.1.1.2 remote-as 500" -c "network 20.0.0.0" -c "network 1.0.0.0"\n')
		g.write('s3 vtysh -c "conf t" -c "router bgp 500" -c "neighbor 2.2.2.2 remote-as 500" -c "network 30.0.0.0" -c "network 2.0.0.0"\n')
		
		n_s = [s1,s2,s3].count('S')
		if n_s == 2:
			## establish neighborship between s1 & s3
			g.write('s1 vtysh -c "conf t" -c "router bgp 500" -c "neighbor 2.2.2.1 remote-as 500"\n')
			g.write('s3 vtysh -c "conf t" -c "router bgp 500" -c "neighbor 1.1.1.1 remote-as 500"\n')
			if s1!='S': ## s1 is client, s2,s3 are servers
				g.write('s2 vtysh -c "conf t" -c "router bgp 500" -c "neighbor 1.1.1.1 route-reflector-client"\n')	
				g.write('s3 vtysh -c "conf t" -c "router bgp 500" -c "neighbor 1.1.1.1 route-reflector-client"\n')
			if s3!='S': ## s3 is client, s2,s1 are servers
				g.write('s2 vtysh -c "conf t" -c "router bgp 500" -c "neighbor 2.2.2.1 route-reflector-client"\n')	
				g.write('s1 vtysh -c "conf t" -c "router bgp 500" -c "neighbor 2.2.2.1 route-reflector-client"\n')
			if s2!='S': ## s2 is client, s1,s3 are servers
				g.write('s1 vtysh -c "conf t" -c "router bgp 500" -c "neighbor 1.1.1.2 route-reflector-client"\n')
				g.write('s3 vtysh -c "conf t" -c "router bgp 500" -c "neighbor 2.2.2.2 route-reflector-client"\n')
				
		if n_s == 1 :
			if s2 != 'S': ## s1 or s3 is server
				g.write('s1 vtysh -c "conf t" -c "router bgp 500" -c "neighbor 2.2.2.1 remote-as 500"\n')
				g.write('s3 vtysh -c "conf t" -c "router bgp 500" -c "neighbor 1.1.1.1 remote-as 500"\n')
				if s1 == 'S':
					if s2=='C' : g.write('s1 vtysh -c "conf t" -c "router bgp 500" -c "neighbor 1.1.1.2 route-reflector-client"\n')
					if s3=='C' : g.write('s1 vtysh -c "conf t" -c "router bgp 500" -c "neighbor 2.2.2.1 route-reflector-client"\n')
				if s3=='S':
					if s1=='C' : g.write('s3 vtysh -c "conf t" -c "router bgp 500" -c "neighbor 1.1.1.1 route-reflector-client"\n')
					if s2=='C' : g.write('s3 vtysh -c "conf t" -c "router bgp 500" -c "neighbor 2.2.2.2 route-reflector-client"\n')
			else: ## s2 is server
				if s1=='C' : g.write('s2 vtysh -c "conf t" -c "router bgp 500" -c "neighbor 1.1.1.1 route-reflector-client"\n')
				if s3=='C' : g.write('s2 vtysh -c "conf t" -c "router bgp 500" -c "neighbor 2.2.2.1 route-reflector-client"\n')
					
		
		
		
		g.write('s1 vtysh -c "show ip bgp"\n')
		g.write('s3 vtysh -c "show ip bgp"\n')	
		g.write('s2 vtysh -c "show ip bgp"\n')	

		
commands_gen ('C','C','C')

			
# typs = ['S','C','NC']
# ct = 0
# for s2 in typs:
# 	for s1 in typs:
# 		for s3 in typs:
# 			ct += 1
# 			print(f"***** CASE #{ct} : s1 = {s1}, s2 = {s2}, s3 = {s3} *****")
	
# 			commands_gen(s1,s2,s3)
# 			time.sleep(5)
# 			args = ["sudo", "python", "route-reflector-scc.py"]
# 			message = 'source commands-rr-scc.txt'
			
# 			sp = subprocess.run(args,input=message.encode())
			
			
			
	

	
		
		

