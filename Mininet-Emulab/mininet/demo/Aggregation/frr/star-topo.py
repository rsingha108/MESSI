
"""
Topology :


       subnet1          subnet4
(s1) ----------- (s2) ----------- (s4)
nh1            ip1| ip4		     nh4
                  |ip3
                  |
                  | subnet3
                  |
                  |nh3
                (s3)		
                   
s1 : exabgp
s3 : exabgp
s2 : router (frr/quagga)
s4 : router (frr/quagga)

nh1 : 1.1.1.1
ip1 : 1.1.1.2
nh4 : 4.4.4.1
ip4 : 4.4.4.2
nh3 : 3.3.3.1
ip3 : 3.3.3.2

"""




import sys

sys.path.insert(0,'../../../..')
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
parser.add_argument("--software", type=str, default="frr")
parser.add_argument("--nh1", type=str, default="1.1.1.1")
parser.add_argument("--ip1", type=str, default="1.1.1.2")
parser.add_argument("--nh4", type=str, default="4.4.4.1")
parser.add_argument("--ip4", type=str, default="4.4.4.2")
parser.add_argument("--nh3", type=str, default="3.3.3.1")
parser.add_argument("--ip3", type=str, default="3.3.3.2")
parser.add_argument("--conset", type=str, default="0")
args = parser.parse_args()

nh1 = args.nh1 #"1.1.1.1"
ip1 = args.ip1 #"1.1.1.2"
nh4 = args.nh4 #"4.4.4.1"
ip4 = args.ip4 #"4.4.4.2"
nh3 = args.nh3 #"3.3.3.1"
ip3 = args.ip3 #"3.3.3.2"

cset = args.conset

img_dict = {'quagga' : "kathara/quagga:latest", 'frr' : "kathara/frr8:latest", 'bird' : "ibhde/bird4:latest"} # frr-ubuntu20:latest, kathara/frr:latest, frr-debian:latest


subprocess.run(["sudo", "docker", "stop", f"mn.s1{cset}"])
subprocess.run(["sudo", "docker", "stop", f"mn.s2{cset}"])
subprocess.run(["sudo", "docker", "stop", f"mn.s3{cset}"])
subprocess.run(["sudo", "docker", "stop", f"mn.s4{cset}"])

subprocess.run(["sudo", "docker", "rm", f"mn.s1{cset}"])
subprocess.run(["sudo", "docker", "rm", f"mn.s2{cset}"])
subprocess.run(["sudo", "docker", "rm", f"mn.s3{cset}"])
subprocess.run(["sudo", "docker", "rm", f"mn.s4{cset}"])


net = Containernet(controller=Controller)

info('*** Adding switches\n')

	             
s1 = net.addDocker(f's1{cset}', dimage="mikenowak/exabgp:latest") #, cls=DockerRouter)  

s3 = net.addDocker(f's3{cset}', dimage="mikenowak/exabgp:latest") #, cls=DockerRouter)  

s2 = net.addDocker(f's2{cset}', dimage=img_dict[args.software], cls=DockerRouter, software=args.software,
			      zebra='yes', 
	              bgpd='yes')

s4 = net.addDocker(f's4{cset}', dimage=img_dict[args.software], cls=DockerRouter, software=args.software,
			      zebra='yes', 
	              bgpd='yes')	                  
	                
                       
info('*** Adding subnets\n')
sp1 = ip1.split('.')[0]+".0.0.0"
sp4 = ip4.split('.')[0]+".0.0.0"
sp3 = ip3.split('.')[0]+".0.0.0" 
snet1 = Subnet(ipStr=sp1, prefixLen=8) 
snet3 = Subnet(ipStr=sp3, prefixLen=8)
snet4 = Subnet(ipStr=sp4, prefixLen=8)

print(f"subnet1 = {sp1}\n")
print(f"subnet3 = {sp3}\n")
print(f"subnet4 = {sp4}\n")

info('*** Creating links\n')

a1 = snet1.assignIpAddr(nh1) 
a2 = snet1.assignIpAddr(ip1)
net.addLink(s1, s2, ip1=a1, ip2=a2) 
snet1.addNode(s1, s2)
print(f"s1-s2 : {a1}-{a2}")

a1 = snet3.assignIpAddr(nh3)
a2 = snet3.assignIpAddr(ip3)
net.addLink(s3, s2, ip1=a1, ip2=a2)
snet3.addNode(s3, s2)
print(f"s3-s2 : {a1}-{a2}")

a1 = snet4.assignIpAddr(nh4)
a2 = snet4.assignIpAddr(ip4)
net.addLink(s4, s2, ip1=a1, ip2=a2)
snet4.addNode(s4, s2)
print(f"s4-s2 : {a1}-{a2}")


s1.start()

s2.start()

s3.start()

s4.start()


info('*** Exp Setup\n')


info('*** Starting network\n')

net.start()

info('*** Running CLI\n')

CLI(net)

info('*** Stopping network')

net.stop()
