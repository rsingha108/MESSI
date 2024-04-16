
"""
Topology :


       subnet1          subnet2
(s1) ----------- (s2) ----------- (d1)
1.1.1.1    1.1.1.2 | 2.2.2.1     2.2.2.2
                   |
                   |3.3.3.2
                   |
                   |
                   |subnet3
                   |
                   |
                   |3.3.3.1
                  (s3)
s1 : exabgp
s2 : quagga
ip1 : 1.1.1.2
ip2 : 3.3.3.2
nh1 : 1.1.1.1
nh2 : 3.3.3.1

"""




import sys

sys.path.insert(0,'../../..')
print(sys.path)
from mininet.net import Containernet
import mininet.node
from mininet.node import * #Controller, Docker, DockerRouter, DockerP4Router
from mininet.nodelib import LinuxBridge
from mininet.cli import CLI
from mininet.link import TCLink
from mininet.log import info, setLogLevel
from mininet.config import Subnet
import os
setLogLevel('info')
import subprocess
import argparse


parser = argparse.ArgumentParser()
parser.add_argument("--software", type=str, default="quagga")
parser.add_argument("--ip1", type=str, default="1.1.1.2")
parser.add_argument("--ip3", type=str, default="3.3.3.2")
parser.add_argument("--nh1", type=str, default="1.1.1.1")
parser.add_argument("--nh3", type=str, default="3.3.3.1")
parser.add_argument("--conset", type=str, default="0")
args = parser.parse_args()

nh1 = args.nh1 #d1[NH]
ip1 = args.ip1 #"1.1.1.2"
nh3 = args.nh3 #d3[NH]
ip3 = args.ip3 #"3.3.3.2"

cset = args.conset

img_dict = {'quagga' : "kathara/quagga:latest", 'frr' : "kathara/frr8:latest", 'bird' : "ibhde/bird4:latest"} # frr-ubuntu20:latest, kathara/frr:latest, frr-debian:latest


subprocess.run(["sudo", "docker", "stop", "mn.d1"])
subprocess.run(["sudo", "docker", "stop", "mn.s1"])
subprocess.run(["sudo", "docker", "stop", "mn.s2"])
subprocess.run(["sudo", "docker", "stop", "mn.s3"])

subprocess.run(["sudo", "docker", "rm", "mn.d1"])
subprocess.run(["sudo", "docker", "rm", "mn.s1"])
subprocess.run(["sudo", "docker", "rm", "mn.s2"])
subprocess.run(["sudo", "docker", "rm", "mn.s3"])


net = Containernet(controller=Controller)

info('*** Adding docker containers\n')

d1 = net.addDocker('d1', dimage="ubuntu:trusty")

info('*** Adding switches\n')

	             
s1 = net.addDocker('s1', dimage="mikenowak/exabgp:latest") #, cls=DockerRouter)                        

s2 = net.addDocker('s2', dimage=img_dict[args.software], cls=DockerRouter, software=args.software,
			      zebra='yes', 
	              bgpd='yes')
	                     
s3 = net.addDocker('s3', dimage="mikenowak/exabgp:latest") #, cls=DockerRouter)
	                  
	                
                       
info('*** Adding subnets\n')
sp1 = ip1.split('.')[0]+".0.0.0"
sp3 = ip3.split('.')[0]+".0.0.0"
snet1 = Subnet(ipStr=sp1, prefixLen=8) 
snetd1 = Subnet(ipStr="2.0.0.0", prefixLen=8)
snet3 = Subnet(ipStr=sp3, prefixLen=8)

# print ("subnet1 = ", sp1)
# print ("subnet3 = ", sp3)
print(f"subnet1 = {sp1}\n")
print(f"subnet3 = {sp3}\n")

info('*** Creating links\n')

a1 = snet1.assignIpAddr(nh1) 
a2 = snet1.assignIpAddr(ip1)
net.addLink(s1, s2, ip1=a1, ip2=a2) 
snet1.addNode(s1, s2)
print(f"s1-s2 : {a1}-{a2}")
# print("s1-s2 : ", a1, "-", a2)

a1 = snetd1.assignIpAddr("2.2.2.1") 
a2 = snetd1.assignIpAddr("2.2.2.2")
net.addLink(s2, d1, ip1=a1, ip2=a2) 
snetd1.addNode(s2, d1)
print(f"s2-d1 : {a1}-{a2}")
# print("s2-d1 : ", a1, "-", a2)

a1 = snet3.assignIpAddr(nh3)
a2 = snet3.assignIpAddr(ip3)
net.addLink(s3, s2, ip1=a1, ip2=a2) 
snet3.addNode(s3, s2)
print(f"s3-s2 : {a1}-{a2}")
# print("s3-s2 : ", a1, "-", a2)

s1.start()

s2.start()

s3.start()


info('*** Exp Setup\n')


info('*** Starting network\n')

net.start()

info('*** Running CLI\n')

CLI(net)

info('*** Stopping network')

net.stop()
