
"""
Topology :

 10.0.0.2     subnet3    10.0.0.1    1.1.1.1  subnet1    1.1.1.2    30.0.0.1  subnet5  30.0.0.2
 d1------------------------------ s1 --------------------------- s3 --------------------------- d3
									|2.2.2.1                   / 3.3.3.2
									|                        /
									|                      /
									|                    /
									|subnet2           / subnet6
									|               /
									|            /
									|         / 3.3.3.1
									|2.2.2.2
									s2
									|20.0.0.1
									|
									|
									|
									|subnet4
									|
									|
									|
									|20.0.0.2
									d2

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

subprocess.run(["sudo", "docker", "stop", "mn.d1"])
subprocess.run(["sudo", "docker", "stop", "mn.s1"])
subprocess.run(["sudo", "docker", "stop", "mn.s2"])
subprocess.run(["sudo", "docker", "stop", "mn.s3"])
subprocess.run(["sudo", "docker", "stop", "mn.d2"])
subprocess.run(["sudo", "docker", "stop", "mn.d3"])

subprocess.run(["sudo", "docker", "rm", "mn.d1"])
subprocess.run(["sudo", "docker", "rm", "mn.s1"])
subprocess.run(["sudo", "docker", "rm", "mn.s2"])
subprocess.run(["sudo", "docker", "rm", "mn.s3"])
subprocess.run(["sudo", "docker", "rm", "mn.d2"])
subprocess.run(["sudo", "docker", "rm", "mn.d3"])


net = Containernet(controller=Controller)

info('*** Adding docker containers\n')

d1 = net.addDocker('d1', dimage="ubuntu:trusty")
d2 = net.addDocker('d2', dimage="ubuntu:trusty")
d3 = net.addDocker('d3', dimage="ubuntu:trusty")

info('*** Adding switches\n')

	             
s1 = net.addDocker('s1', dimage="kathara/quagga:latest", cls=DockerRouter,
			     bgpd='yes')                        

s2 = net.addDocker('s2', dimage="kathara/quagga:latest", cls=DockerRouter, 
	                     bgpd='yes')
s3 = net.addDocker('s3', dimage="kathara/quagga:latest", cls=DockerRouter, 
			     bgpd='yes')

	                
                       
info('*** Adding subnets\n')
snet1 = Subnet(ipStr="1.0.0.0", prefixLen=8) 
snet2 = Subnet(ipStr="2.0.0.0", prefixLen=8)
snet3 = Subnet(ipStr="10.0.0.0", prefixLen=8)
snet4 = Subnet(ipStr="20.0.0.0", prefixLen=8)
snet5 = Subnet(ipStr="30.0.0.0", prefixLen=8)
snet6 = Subnet(ipStr="3.0.0.0",prefixLen=8)


info('*** Creating links\n')

ip1 = snet1.assignIpAddr("1.1.1.1") 
ip2 = snet1.assignIpAddr("1.1.1.2")
net.addLink(s1, s3, ip1=ip1, ip2=ip2) 
snet1.addNode(s1, s3)

ip1 = snet2.assignIpAddr("2.2.2.1") 
ip2 = snet2.assignIpAddr("2.2.2.2")
net.addLink(s1, s2, ip1=ip1, ip2=ip2) 
snet2.addNode(s1, s2)

ip1 = snet3.assignIpAddr("10.0.0.1") 
ip2 = snet3.assignIpAddr("10.0.0.2")
net.addLink(s1, d1, ip1=ip1, ip2=ip2) 
snet3.addNode(s1, d1)


ip1 = snet4.assignIpAddr("20.0.0.1") 
ip2 = snet4.assignIpAddr("20.0.0.2")
net.addLink(s2, d2, ip1=ip1, ip2=ip2) 
snet4.addNode(s2, d2)

ip1 = snet5.assignIpAddr("30.0.0.1") 
ip2 = snet5.assignIpAddr("30.0.0.2")
net.addLink(s3, d3, ip1=ip1, ip2=ip2) 
snet5.addNode(s3, d3)

ip1 = snet6.assignIpAddr("3.3.3.1") 
ip2 = snet6.assignIpAddr("3.3.3.2")
net.addLink(s2, s3, ip1=ip1, ip2=ip2) 
snet6.addNode(s2, s3)



s1.start()

s2.start()

s3.start()

d1.setDefaultRoute("gw 10.0.0.1")
d2.setDefaultRoute("gw 20.0.0.1")
d3.setDefaultRoute("gw 30.0.0.1")

info('*** Exp Setup\n')


info('*** Starting network\n')

net.start()

info('*** Running CLI\n')

CLI(net)

info('*** Stopping network')

net.stop()
