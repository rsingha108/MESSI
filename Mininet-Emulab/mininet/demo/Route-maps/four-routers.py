"""
cisco bgp tutorial 4 router topology
"""


import sys

sys.path.insert(0,'/home/rathin/Desktop/Mininet-Emulab')
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
args = parser.parse_args()

img_dict = {'quagga' : "kathara/quagga:latest", 'frr' : "kathara/frr:latest", 'bird' : "ibhde/bird4:latest"} # frr-ubuntu20:latest, kathara/frr:latest, frr-debian:latest


subprocess.run(["sudo", "docker", "stop", "mn.d1"])
subprocess.run(["sudo", "docker", "stop", "mn.d2"])
subprocess.run(["sudo", "docker", "stop", "mn.d3"])
subprocess.run(["sudo", "docker", "stop", "mn.d4"])
subprocess.run(["sudo", "docker", "stop", "mn.s1"])
subprocess.run(["sudo", "docker", "stop", "mn.s2"])
subprocess.run(["sudo", "docker", "stop", "mn.s3"])
subprocess.run(["sudo", "docker", "stop", "mn.s4"])

subprocess.run(["sudo", "docker", "rm", "mn.d1"])
subprocess.run(["sudo", "docker", "rm", "mn.d2"])
subprocess.run(["sudo", "docker", "rm", "mn.d3"])
subprocess.run(["sudo", "docker", "rm", "mn.d4"])
subprocess.run(["sudo", "docker", "rm", "mn.s1"])
subprocess.run(["sudo", "docker", "rm", "mn.s2"])
subprocess.run(["sudo", "docker", "rm", "mn.s3"])
subprocess.run(["sudo", "docker", "rm", "mn.s4"])

net = Containernet(controller=Controller)

info('*** Adding docker containers\n')

d1 = net.addDocker('d1', dimage="ubuntu:trusty")
d2 = net.addDocker('d2', dimage="ubuntu:trusty")
d3 = net.addDocker('d3', dimage="ubuntu:trusty")
d4 = net.addDocker('d4', dimage="ubuntu:trusty")

info('*** Adding switches\n')

	             
s1 = net.addDocker('s1', dimage=img_dict[args.software], cls=DockerRouter, software=args.software,
			      zebra='yes', 
	              bgpd='yes')                       

s2 = net.addDocker('s2', dimage=img_dict[args.software], cls=DockerRouter, software=args.software,
			      zebra='yes', 
	              bgpd='yes')
	                     
s3 = net.addDocker('s3', dimage=img_dict[args.software], cls=DockerRouter, software=args.software,
			      zebra='yes', 
	              bgpd='yes')
	              
s4 = net.addDocker('s4', dimage=img_dict[args.software], cls=DockerRouter, software=args.software,
			      zebra='yes', 
	              bgpd='yes')
	                  
	                
                       
info('*** Adding subnets\n')

snet1 = Subnet(ipStr="1.0.0.0", prefixLen=8) 
snet2 = Subnet(ipStr="2.0.0.0", prefixLen=8)
snet3 = Subnet(ipStr="3.0.0.0", prefixLen=8)
snet4 = Subnet(ipStr="4.0.0.0", prefixLen=8)
snet10 = Subnet(ipStr="10.0.0.0", prefixLen=8) 
snet20 = Subnet(ipStr="20.0.0.0", prefixLen=8)
snet30 = Subnet(ipStr="30.0.0.0", prefixLen=8)
snet40 = Subnet(ipStr="40.0.0.0", prefixLen=8)

info('*** Creating links\n')

ip1 = snet1.assignIpAddr("1.1.1.1") 
ip2 = snet1.assignIpAddr("1.1.1.2")
net.addLink(s1, s2, ip1=ip1, ip2=ip2) 
snet1.addNode(s1, s2)
print(f"s1-s2 : {ip1}-{ip2}")

ip1 = snet2.assignIpAddr("2.2.2.1") 
ip2 = snet2.assignIpAddr("2.2.2.2")
net.addLink(s2, s3, ip1=ip1, ip2=ip2) 
snet2.addNode(s2, s3)
print(f"s2-s3 : {ip1}-{ip2}")

ip1 = snet3.assignIpAddr("3.3.3.1") 
ip2 = snet3.assignIpAddr("3.3.3.2")
net.addLink(s3, s4, ip1=ip1, ip2=ip2) 
snet3.addNode(s3, s4)
print(f"s3-s4 : {ip1}-{ip2}")

ip1 = snet4.assignIpAddr("4.4.4.1") 
ip2 = snet4.assignIpAddr("4.4.4.2")
net.addLink(s4, s1, ip1=ip1, ip2=ip2) 
snet4.addNode(s4, s1)
print(f"s4-s1 : {ip1}-{ip2}")

ip1 = snet10.assignIpAddr("10.0.0.1") 
ip2 = snet10.assignIpAddr("10.0.0.2")
net.addLink(s1, d1, ip1=ip1, ip2=ip2) 
snet10.addNode(s1, d1)
print(f"s1-d1 : {ip1}-{ip2}")

ip1 = snet20.assignIpAddr("20.0.0.1") 
ip2 = snet20.assignIpAddr("20.0.0.2")
net.addLink(s2, d2, ip1=ip1, ip2=ip2) 
snet20.addNode(s2, d2)
print(f"s2-d2 : {ip1}-{ip2}")

ip1 = snet30.assignIpAddr("30.0.0.1") 
ip2 = snet30.assignIpAddr("30.0.0.2")
net.addLink(s3, d3, ip1=ip1, ip2=ip2) 
snet30.addNode(s3, d3)
print(f"s3-d3 : {ip1}-{ip2}")

ip1 = snet40.assignIpAddr("40.0.0.1") 
ip2 = snet40.assignIpAddr("40.0.0.2")
net.addLink(s4, d4, ip1=ip1, ip2=ip2) 
snet40.addNode(s4, d4)
print(f"s4-d4 : {ip1}-{ip2}")


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
