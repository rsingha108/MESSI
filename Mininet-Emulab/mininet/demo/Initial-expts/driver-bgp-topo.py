import subprocess

with open('commands.txt','rb') as f:
	data = f.read()
	
myinp = open('commands.txt')
	
message = 'source commands.txt'

args = ["sudo", "python", "bgp-topo.py"]

#sp = subprocess.run("sudo python bgp-topo.py",shell=True,input=message.encode()) #stdin=myinp) (working)

sp = subprocess.run(args,input=message.encode()) #stdin=myinp)



#sp = subprocess.Popen(args, stdin=subprocess.PIPE)
#sp.wait()

#sp.stdin.write(message.encode())

#sp.stdin.write(b"s2 route")
