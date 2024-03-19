import subprocess

	
message = 'source exabgp_router2_commands.txt'

args = ["sudo", "python", "exabgp-router2.py"]

sp = subprocess.run(args,input=message.encode()) #stdin=myinp)



