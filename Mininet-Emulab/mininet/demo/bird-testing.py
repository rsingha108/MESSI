import sys

sys.path.insert(0,'/home/rathin/Desktop/Mininet-Emulab')
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

	             
s1 = net.addDocker('s1', dimage="ibhde/bird4:latest", cls=DockerRouter, software='bird',
	                     bgpd='yes')                         

s2 = net.addDocker('s2', dimage="ibhde/bird4:latest", cls=DockerRouter, software='bird',
	                     bgpd='yes')
	                  
s3 = net.addDocker('s3', dimage="ibhde/bird4:latest", cls=DockerRouter, software='bird',
	                     bgpd='yes') 
	                     
s4 = net.addDocker('s4', dimage="ibhde/bird4:latest", cls=DockerRouter, software='bird',
	                     bgpd='yes')
	                
                       
info('*** Adding subnets\n')
snet1 = Subnet(ipStr="1.0.0.0", prefixLen=8) 
snet2 = Subnet(ipStr="2.0.0.0", prefixLen=8)
snet3 = Subnet(ipStr="3.0.0.0", prefixLen=8)
snet4 = Subnet(ipStr="4.0.0.0", prefixLen=8)
snet5 = Subnet(ipStr="10.0.0.0", prefixLen=8)
snet6 = Subnet(ipStr="20.0.0.0", prefixLen=8)
snet7 = Subnet(ipStr="30.0.0.0", prefixLen=8)
snet8 = Subnet(ipStr="40.0.0.0", prefixLen=8)


info('*** Creating links\n')

ip1 = snet1.assignIpAddr("1.1.1.1") 
ip2 = snet1.assignIpAddr("1.1.1.2")
net.addLink(s1, s2, ip1=ip1, ip2=ip2) 
snet1.addNode(s1, s2)

ip1 = snet2.assignIpAddr("2.2.2.1") 
ip2 = snet2.assignIpAddr("2.2.2.2") 
net.addLink(s2, s3, ip1=ip1, ip2=ip2) 
snet2.addNode(s2,s3)

ip1 = snet3.assignIpAddr("3.3.3.1")
ip2 = snet3.assignIpAddr("3.3.3.2")
net.addLink(s3, s4, ip1=ip1, ip2=ip2) 
snet3.addNode(s3,s4)

ip1 = snet4.assignIpAddr("4.4.4.1")
ip2 = snet4.assignIpAddr("4.4.4.2")
net.addLink(s4, s1, ip1=ip1, ip2=ip2) 
snet4.addNode(s4, s1)

ip1 = snet5.assignIpAddr("10.0.0.1")
ip2 = snet5.assignIpAddr("10.0.0.2")
net.addLink(d1, s1, ip1=ip1, ip2=ip2) 
snet5.addNode(d1, s1)

ip1 = snet6.assignIpAddr("20.0.0.1")
ip2 = snet6.assignIpAddr("20.0.0.2")
net.addLink(d2, s2, ip1=ip1, ip2=ip2) 
snet6.addNode(d2, s2)

ip1 = snet7.assignIpAddr("30.0.0.1")
ip2 = snet7.assignIpAddr("30.0.0.2")
net.addLink(d3, s3, ip1=ip1, ip2=ip2) 
snet7.addNode(d3, s3)

ip1 = snet8.assignIpAddr("40.0.0.1")
ip2 = snet8.assignIpAddr("40.0.0.2")
net.addLink(d4, s4, ip1=ip1, ip2=ip2) 
snet8.addNode(d4, s4)

s1.start()

s2.start()

s3.start()

s4.start()
	
d1.setDefaultRoute("gw 10.0.0.2") ## set default route as s1 for host d1 (see the link s1-d1)
d2.setDefaultRoute("gw 20.0.0.2")
d3.setDefaultRoute("gw 30.0.0.2")
d4.setDefaultRoute("gw 40.0.0.2")


info('*** Exp Setup\n')


info('*** Starting network\n')

net.start()

info('*** Running CLI\n')

CLI(net)

info('*** Stopping network')

net.stop()
