
"""
Topology :


       subnet1          subnet2
(s1) ----------- (s2) ----------- (d1)
1.1.1.1    1.1.1.2 | 2.2.2.1     2.2.2.2
                   
s1 : exabgp
s2 : router (frr/quagga)
ip1 : 1.1.1.2
nh1 : 1.1.1.1

"""




import sys

# sys.path.insert(0,'/home/rathin/Desktop/Mininet-Emulab')
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
# setLogLevel('info') ## comment out to supress all info messages
import subprocess
import argparse


parser = argparse.ArgumentParser()
parser.add_argument("--software", type=str, default="frr")
parser.add_argument("--ip1", type=str, default="1.1.1.2")
parser.add_argument("--nh1", type=str, default="1.1.1.1")
parser.add_argument("--conset", type=str, default="0")
args = parser.parse_args()

cset = args.conset

img_dict = {'quagga' : "kathara/quagga:latest", 'frr' : "kathara/frr8:latest", 'bird' : "ibhde/bird4:latest"} # frr-ubuntu20:latest, kathara/frr:latest, frr-debian:latest


subprocess.run(["sudo", "docker", "stop", f"mn.d1{cset}"], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
subprocess.run(["sudo", "docker", "stop", f"mn.s1{cset}"], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
subprocess.run(["sudo", "docker", "stop", f"mn.s2{cset}"], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)

subprocess.run(["sudo", "docker", "rm", f"mn.d1{cset}"], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
subprocess.run(["sudo", "docker", "rm", f"mn.s1{cset}"], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
subprocess.run(["sudo", "docker", "rm", f"mn.s2{cset}"], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)


net = Containernet(controller=Controller)

info('*** Adding docker containers\n')

d1 = net.addDocker(f'd1{cset}', dimage="ubuntu:trusty")

info('*** Adding switches\n')

	             
s1 = net.addDocker(f's1{cset}', dimage="mikenowak/exabgp:latest") #, cls=DockerRouter)                        

s2 = net.addDocker(f's2{cset}', dimage=img_dict[args.software], cls=DockerRouter, software=args.software,
			      zebra='yes', 
	              bgpd='yes')
	                     
	                  
	                
                       
info('*** Adding subnets\n')
sp1 = args.ip1.split('.')[0]+".0.0.0"
snet1 = Subnet(ipStr=sp1, prefixLen=8) 
snet2 = Subnet(ipStr="2.0.0.0", prefixLen=8)


print(f"subnet1 = {sp1}\n")

info('*** Creating links\n')

ip1 = snet1.assignIpAddr(args.nh1) 
ip2 = snet1.assignIpAddr(args.ip1)
net.addLink(s1, s2, ip1=ip1, ip2=ip2) 
snet1.addNode(s1, s2)
print(f"s1-s2 : {ip1}-{ip2}")

ip1 = snet2.assignIpAddr("2.2.2.1") 
ip2 = snet2.assignIpAddr("2.2.2.2")
net.addLink(s2, d1, ip1=ip1, ip2=ip2) 
snet1.addNode(s2, d1)
print(f"s2-d1 : {ip1}-{ip2}")


s1.start()

s2.start()



info('*** Exp Setup\n')


info('*** Starting network\n')

net.start()

info('*** Running CLI\n')

CLI(net)

info('*** Stopping network')

net.stop()
