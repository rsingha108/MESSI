import subprocess

with open('exabgp_router_commands.txt','rb') as f:
	data = f.read()
	
message = 'source exabgp_router_commands.txt'

args = ["sudo", "python", "exabgp-router.py"]

sp = subprocess.run(args,input=message.encode()) #stdin=myinp)



