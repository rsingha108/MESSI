
"""
Topology :


       subnet1          subnet2
(s1) ----------- (s2) ----------- (d1)
1.1.1.1    1.1.1.2  2.2.2.1     2.2.2.2

s1 : exabgp
s2 : quagga


"""




import sys

sys.path.append('/home/rathin/Desktop/Mininet-Emulab')
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

subprocess.run(["sudo", "docker", "stop", "mn.d1"])
subprocess.run(["sudo", "docker", "stop", "mn.s1"])
subprocess.run(["sudo", "docker", "stop", "mn.s2"])

subprocess.run(["sudo", "docker", "rm", "mn.d1"])
subprocess.run(["sudo", "docker", "rm", "mn.s1"])
subprocess.run(["sudo", "docker", "rm", "mn.s2"])


net = Containernet(controller=Controller)

info('*** Adding docker containers\n')

d1 = net.addDocker('d1', dimage="ubuntu:trusty")

info('*** Adding switches\n')

	             
s1 = net.addDocker('s1', dimage="mikenowak/exabgp:latest", cls=DockerRouter)                        

s2 = net.addDocker('s2', dimage="kathara/quagga:latest", cls=DockerRouter, 
						 zebra='yes',
	                     bgpd='yes')
	                  
	                
                       
info('*** Adding subnets\n')
snet1 = Subnet(ipStr="1.0.0.0", prefixLen=8) 
snet2 = Subnet(ipStr="2.0.0.0", prefixLen=8)

info('*** Creating links\n')

ip1 = snet1.assignIpAddr("1.1.1.1") 
ip2 = snet1.assignIpAddr("1.1.1.2")
net.addLink(s1, s2, ip1=ip1, ip2=ip2) 
snet1.addNode(s1, s2)

ip1 = snet2.assignIpAddr("2.2.2.1") 
ip2 = snet2.assignIpAddr("2.2.2.2")
net.addLink(s2, d1, ip1=ip1, ip2=ip2) 
snet1.addNode(s2, d1)

s1.start()

s2.start()


info('*** Exp Setup\n')


info('*** Starting network\n')

net.start()

info('*** Running CLI\n')

CLI(net)

info('*** Stopping network')

net.stop()
