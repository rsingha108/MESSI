- [About this repository](#about-this-repository)
- [Requirements to run this repository](#requirements-to-run-this-repository)
- [Instructions to use this repository](#instructions-to-use-this-repository)
- [Credit](#credit)

# About this repository

GoBGP demo using Docker Compose:
- This [docker-compose file](docker-compose.yml) uses this [Dockerfile](Dockerfile)
- The GoBGP container "gobgp_1" uses this [configuration file](gobgp1/gobgp.yml)
- The GoBGP container "gobgp_2" uses this [configuration file](gobgp2/gobgp.yml)


# Requirements to run this repository

- Install Docker
- Install Docker Compose

```
docker --version
Docker version 20.10.1, build 831ebea
```
```
docker-compose --version
docker-compose version 1.27.4, build 40524192
```

# Instructions to use this repository

- Clone this repository
```
git clone https://gitlab.aristanetworks.com/emea-se-team/gobgp_demo.git
```
- Move to the local directory
```
cd gobgp_demo
```
- Run this command to:
  - Create the network
  - Build the docker image
  - Create the containers
  - Start the containers

```
docker-compose -f docker-compose.yml up -d
Creating network "gobgp_demo_test_net" with driver "bridge"
Creating gobgp_2 ... done
Creating gobgp_1 ... done
```
- Run these commands to verify:
```
docker images
REPOSITORY         TAG           IMAGE ID       CREATED          SIZE
ksator/gobgp       1.0           f8cbe28b2372   56 minutes ago   1.05GB
golang             latest        b09f7387a719   12 days ago      862MB

docker ps
CONTAINER ID   IMAGE              COMMAND                  CREATED          STATUS          PORTS     NAMES
af448f6bd9fd   ksator/gobgp:1.0   "gobgpd -t yaml -f /…"   30 seconds ago   Up 27 seconds   179/tcp   gobgp_1
597af3ed77f6   ksator/gobgp:1.0   "gobgpd -t yaml -f /…"   30 seconds ago   Up 28 seconds   179/tcp   gobgp_2

docker network ls | grep gobgp
48e93609db19   gobgp_demo_test_net   bridge    local

docker-compose ps
 Name                Command               State    Ports
----------------------------------------------------------
gobgp_1   gobgpd -t yaml -f /etc/gob ...   Up      179/tcp
gobgp_2   gobgpd -t yaml -f /etc/gob ...   Up      179/tcp
```

- Run these commands to verify BGP sessions state
```
docker exec -it gobgp_1 gobgp neighbor
Peer          AS  Up/Down State       |#Received  Accepted
10.0.0.200 65002 00:01:11 Establ      |        0         0
```
```
docker exec -it gobgp_2 bash

root@597af3ed77f6:/go# gobgp neighbor
Peer          AS  Up/Down State       |#Received  Accepted
10.0.0.100 65001 00:01:34 Establ      |        0         0

root@597af3ed77f6:/go# gobgp neighbor 10.0.0.100
BGP neighbor is 10.0.0.100, remote AS 65001
  BGP version 4, remote router ID 192.168.255.1
  BGP state = ESTABLISHED, up for 00:02:49
  BGP OutQ = 0, Flops = 0
  Hold time is 90, keepalive interval is 30 seconds
  Configured hold time is 90, keepalive interval is 30 seconds

  Neighbor capabilities:
    multiprotocol:
        ipv4-unicast:   advertised and received
    route-refresh:      advertised and received
    extended-nexthop:   advertised and received
        Local:  nlri: ipv4-unicast, nexthop: ipv6
        Remote: nlri: ipv4-unicast, nexthop: ipv6
    4-octet-as: advertised and received
    fqdn:       advertised and received
      Local:
         name: 597af3ed77f6, domain:
      Remote:
         name: af448f6bd9fd, domain:
  Message statistics:
                         Sent       Rcvd
    Opens:                  1          1
    Notifications:          0          0
    Updates:                0          0
    Keepalives:             6          6
    Route Refresh:          0          0
    Discarded:              0          0
    Total:                  7          7
  Route statistics:
    Advertised:             0
    Received:               0
    Accepted:               0

root@597af3ed77f6:/go#  exit
exit
```
Run these commands if you want to add a route (IPv4 address family):
```
docker exec -it gobgp_2 bash

root@597af3ed77f6:/go# gobgp global
AS:        65002
Router-ID: 192.168.255.0
Listening Port: 179, Addresses: 0.0.0.0, ::

root@597af3ed77f6:/go# gobgp global rib summary
Table afi:AFI_IP safi:SAFI_UNICAST
Destination: 0, Path: 0

root@597af3ed77f6:/go# gobgp global rib add 10.33.0.0/16 -a ipv4

root@597af3ed77f6:/go# gobgp global rib summary
Table afi:AFI_IP safi:SAFI_UNICAST
Destination: 1, Path: 1

root@597af3ed77f6:/go# gobgp neighbor 10.0.0.100  adj-out -a ipv4
   ID  Network              Next Hop             AS_PATH              Attrs
   1   10.33.0.0/16         10.0.0.200           65002                [{Origin: ?}]

root@597af3ed77f6:/go# gobgp neighbor 10.0.0.100
BGP neighbor is 10.0.0.100, remote AS 65001
  BGP version 4, remote router ID 192.168.255.1
  BGP state = ESTABLISHED, up for 00:12:01
  BGP OutQ = 0, Flops = 0
  Hold time is 90, keepalive interval is 30 seconds
  Configured hold time is 90, keepalive interval is 30 seconds

  Neighbor capabilities:
    multiprotocol:
        ipv4-unicast:   advertised and received
    route-refresh:      advertised and received
    extended-nexthop:   advertised and received
        Local:  nlri: ipv4-unicast, nexthop: ipv6
        Remote: nlri: ipv4-unicast, nexthop: ipv6
    4-octet-as: advertised and received
    fqdn:       advertised and received
      Local:
         name: 597af3ed77f6, domain:
      Remote:
         name: af448f6bd9fd, domain:
  Message statistics:
                         Sent       Rcvd
    Opens:                  1          1
    Notifications:          0          0
    Updates:                1          0
    Keepalives:            25         25
    Route Refresh:          0          0
    Discarded:              0          0
    Total:                 27         26
  Route statistics:
    Advertised:             1
    Received:               0
    Accepted:               0

root@597af3ed77f6:/go# exit
exit
```
```
docker exec -it gobgp_1 gobgp neighbor
Peer          AS  Up/Down State       |#Received  Accepted
10.0.0.200 65002 00:04:36 Establ      |        1         1

docker exec -it gobgp_1  gobgp neighbor 10.0.0.200 adj-in -a ipv4
   ID  Network              Next Hop             AS_PATH              Age        Attrs
   0   10.33.0.0/16         10.0.0.200           65002                00:00:43   [{Origin: ?}]

docker exec -it gobgp_1  gobgp neighbor 10.0.0.200
BGP neighbor is 10.0.0.200, remote AS 65002
  BGP version 4, remote router ID 192.168.255.0
  BGP state = ESTABLISHED, up for 00:13:20
  BGP OutQ = 0, Flops = 0
  Hold time is 90, keepalive interval is 30 seconds
  Configured hold time is 90, keepalive interval is 30 seconds

  Neighbor capabilities:
    multiprotocol:
        ipv4-unicast:   advertised and received
    route-refresh:      advertised and received
    extended-nexthop:   advertised and received
        Local:  nlri: ipv4-unicast, nexthop: ipv6
        Remote: nlri: ipv4-unicast, nexthop: ipv6
    4-octet-as: advertised and received
    fqdn:       advertised and received
      Local:
         name: af448f6bd9fd, domain:
      Remote:
         name: 597af3ed77f6, domain:
  Message statistics:
                         Sent       Rcvd
    Opens:                  1          1
    Notifications:          0          0
    Updates:                0          1
    Keepalives:            27         27
    Route Refresh:          0          0
    Discarded:              0          0
    Total:                 28         29
  Route statistics:
    Advertised:             0
    Received:               1
    Accepted:               1
```
- Run this command to:
  - Stop the containers
  - Remove the containers
  - Remove the networks
```
docker-compose down
Stopping gobgp_1 ... done
Stopping gobgp_2 ... done
Removing gobgp_1 ... done
Removing gobgp_2 ... done
Removing network gobgp_demo_test_net
```
# Credit

Credit goes to Pierre Dezitter for his works on this topic.
