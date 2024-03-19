#!/usr/bin/python
"""
This is a simple example to emulate a common network fault, random packet drops on some switch.
"""
import sys
import random
sys.path.append('/m/local2/wcr/Mininet-Emulab')

from mininet.net import Containernet
from mininet.node import * #Controller, Docker, DockerRouter, DockerP4Router
from mininet.nodelib import LinuxBridge
from mininet.cli import CLI
from mininet.link import TCLink
from mininet.log import info, setLogLevel
from mininet.config import Subnet, NodeList
import os
setLogLevel('info')

net = Containernet(controller=Controller)
numOfAS = 3
sizeOfAS = 3
nodes = NodeList() # used for generating topology file

info('*** Adding docker containers\n')

host_list = list()
for i in range(0, numOfAS * (sizeOfAS - 1)):
    new_host = net.addDocker('d{}'.format(i), dimage="ubuntu:trusty_v2")
    host_list.append(new_host)

admin_host = net.addDocker('admin', dimage="p4switch-frr:v2")
host_list.append(admin_host)

info('*** Adding switches\n')

switch_list = list()
for i in range(0, numOfAS * sizeOfAS):
    new_switch = net.addDocker('s{}'.format(i), cls=DockerP4Router, 
                         dimage="p4switch-frr:v2",
                         software="frr",
                         json_path="/m/local2/wcr/P4-Switches/ecmp_switch.json", 
                         pcap_dump="/tmp",
                         log_console=True,
                         log_level="info",
                         rt_mediator= "/m/local2/wcr/P4-Switches/rt_mediator.py",
                         bgpd='yes',
                         ospfd='yes')
    switch_list.append(new_switch)
    new_switch.addRoutingConfig("bgpd", "router bgp {asn}".format(asn=int(i / sizeOfAS + 1)))
    new_switch.addRoutingConfig("bgpd", "no bgp ebgp-requires-policy")
    new_switch.addRoutingConfig("ospfd", "router ospf")

info('*** Adding subnets\n')
snet_list = list()
for i in range(0, 100):
    new_snet = Subnet(ipStr="10.{}.0.0".format(i), prefixLen=24)
    snet_list.append(new_snet)

info('*** Creating links & Configure routes\n')

snet_counter = 0

# configure inter-AS switch-switch links
for i in range(0, numOfAS):
    for j in range(i + 1, numOfAS):
        index1 = i * sizeOfAS
        index2 = j * sizeOfAS

        if i != j:
            ip1 = snet_list[snet_counter].allocateIPAddr()
            ip2 = snet_list[snet_counter].allocateIPAddr()

            # configure links
            link = net.addLink(switch_list[index1], switch_list[index2], ip1=ip1, ip2=ip2, addr1=Subnet.ipToMac(ip1), addr2=Subnet.ipToMac(ip2))
            snet_list[snet_counter].addNode(switch_list[index1], switch_list[index2])

            nodes.addNode(switch_list[index1].name, ip=ip1, nodeType="switch")
            nodes.addNode(switch_list[index2].name, ip=ip2, nodeType="switch")
            nodes.addLink(switch_list[index1].name, switch_list[index2].name)

            # configure eBGP peers
            switch_list[index1].addRoutingConfig("bgpd", "neighbor {} remote-as {}".format(ip2.split("/")[0], j + 1))
            switch_list[index2].addRoutingConfig("bgpd", "neighbor {} remote-as {}".format(ip1.split("/")[0], i + 1))

            # add new advertised network prefix
            switch_list[index1].addRoutingConfig("bgpd", "network " + snet_list[snet_counter].getNetworkPrefix())
            switch_list[index2].addRoutingConfig("bgpd", "network " + snet_list[snet_counter].getNetworkPrefix())

            snet_counter += 1

# configure intra-AS switch-switch links
for i in range(0, numOfAS):
    edgeRouter = i * sizeOfAS
    edgeRouterIp = ""

    # configure a single AS
    bgp_network_list = []
    for j in range(0, sizeOfAS):
        index1 = i * sizeOfAS + j
        index2 = i * sizeOfAS + (j + 1) % sizeOfAS

        # configure links
        ip1 = snet_list[snet_counter].allocateIPAddr()
        ip2 = snet_list[snet_counter].allocateIPAddr()
        link = net.addLink(switch_list[index1], switch_list[index2], ip1=ip1, ip2=ip2, addr1=Subnet.ipToMac(ip1), addr2=Subnet.ipToMac(ip2))
        snet_list[snet_counter].addNode(switch_list[index1], switch_list[index2])

        # config IGP routing, using OSPF
        switch_list[index1].addRoutingConfig("ospfd", "network " + snet_list[snet_counter].getNetworkPrefix() + " area {}".format(0))
        switch_list[index2].addRoutingConfig("ospfd", "network " + snet_list[snet_counter].getNetworkPrefix() + " area {}".format(0))

        # select edge router ip
        if index1 == edgeRouter:
            edgeRouterIp = ip1

        # config iBGP peers
        if index1 != edgeRouter:
            switch_list[edgeRouter].addRoutingConfig("bgpd", "neighbor {} remote-as {}".format(ip1.split("/")[0], i + 1))
            switch_list[index1].addRoutingConfig("bgpd", "neighbor {} remote-as {}".format(edgeRouterIp.split("/")[0], i + 1))

        # add new bgp advertised network prefix
        bgp_network_list.append(snet_list[snet_counter].getNetworkPrefix())

        nodes.addNode(switch_list[index1].name, ip=ip1, nodeType="switch")
        nodes.addNode(switch_list[index2].name, ip=ip2, nodeType="switch")
        nodes.addLink(switch_list[index1].name, switch_list[index2].name)

        snet_counter += 1

    # configure the advertised network prefixes for the AS
    for bgpNetwork in bgp_network_list:
        switch_list[edgeRouter].addRoutingConfig("bgpd", "network " + bgpNetwork)

# configure host-switch links
for i in range(0, numOfAS):
    edgeRouter = i * sizeOfAS

    # configure a single AS
    for j in range(0, sizeOfAS - 1):
        sid = i * sizeOfAS + 1 + j
        hid = i * (sizeOfAS - 1) + j

        ip1 = snet_list[snet_counter].allocateIPAddr()
        ip2 = snet_list[snet_counter].allocateIPAddr()
        net.addLink(switch_list[sid], host_list[hid], ip1=ip1, ip2=ip2, addr1=Subnet.ipToMac(ip1), addr2=Subnet.ipToMac(ip2))
        snet_list[snet_counter].addNode(switch_list[sid])
        switch_list[sid].addRoutingConfig("ospfd", "network " + snet_list[snet_counter].getNetworkPrefix() + " area {}".format(0))

        host_list[hid].setDefaultRoute("gw {}".format(ip1.split("/")[0]))

        nodes.addNode(host_list[hid].name, ip=ip2, nodeType="host")
        nodes.addLink(switch_list[sid].name, host_list[hid].name)

        # add a new advertised network prefix for the AS
        switch_list[edgeRouter].addRoutingConfig("bgpd", "network " + snet_list[snet_counter].getNetworkPrefix())

        snet_counter += 1

# configure the link between admin host
ip1 = snet_list[snet_counter].allocateIPAddr()
ip2 = snet_list[snet_counter].allocateIPAddr()
net.addLink(switch_list[0], admin_host, ip1=ip1, ip2=ip2, addr1=Subnet.ipToMac(ip1), addr2=Subnet.ipToMac(ip2))
snet_list[snet_counter].addNode(switch_list[0])
switch_list[0].addRoutingConfig("ospfd", "network " + snet_list[snet_counter].getNetworkPrefix() + " area {}".format(0))
admin_host.setDefaultRoute("gw {}".format(ip1.split("/")[0]))
nodes.addNode(admin_host.name, ip=ip2, nodeType="host")
nodes.addLink(switch_list[0].name, admin_host.name)
switch_list[0].addRoutingConfig("bgpd", "network " + snet_list[snet_counter].getNetworkPrefix())
snet_counter += 1

for snet in snet_list:
    snet.installSubnetTable()

info('*** Exp Setup\n')

nodes.writeFile("topo.txt")
os.system("docker cp /m/local2/wcr/Diagnosis-driver/driver.tar.bz mn.admin:/")
os.system("docker cp /m/local2/wcr/Mininet-Emulab/topo.txt mn.admin:/")
os.system("docker cp /m/local2/wcr/Diagnosis-driver/example_mesh.config mn.admin:/")

info('*** Starting network\n')

for host in host_list:
    host.start()

for switch in switch_list:
    switch.addRoutingConfig("zebra","log file /tmp/zebra.log")
    switch.addRoutingConfig("ospfd", "log file /tmp/ospfd.log")
    switch.addRoutingConfig("bgpd", "log file /tmp/bgpd.log")
    switch.start()

net.start()

info('*** Running CLI\n')

CLI(net)

info('*** Stopping network')

net.stop()
