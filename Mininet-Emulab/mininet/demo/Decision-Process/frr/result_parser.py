"""
Displayed  4 routes and 5 total paths
BGP table version is 4, local router ID is 192.168.19.26, vrf id 0
Default local pref 100, local AS 100
Status codes:  s suppressed, d damped, h history, * valid, > best, = multipath,
               i internal, r RIB-failure, S Stale, R Removed
Nexthop codes: @NNN nexthop's vrf id, < announce-nh-self
Origin codes:  i - IGP, e - EGP, ? - incomplete
RPKI validation codes: V valid, I invalid, N Not found

    Network          Next Hop            Metric LocPrf Weight Path
 *> 1.0.0.0/8        0.0.0.0                  0         32768 i
 *> 3.0.0.0/8        0.0.0.0                  0         32768 i
 * i4.0.0.0/8        4.4.4.1                  0    100      0 i
 *>                  0.0.0.0                  0         32768 i
 *  100.10.1.0/24    3.3.3.1                173             0 3000 3001 3002 i
 *>                  1.1.1.1                177             0 1000 1001 e
"""

def parse_rib(test):
	with open("out.txt","r") as f:
		lines = f.readlines()
	route_idx = 0
	for i,line in enumerate(lines):
		if "100.10.1.0/24" not in line:
			continue
		route_idx = i
		break

	filtered_lines = lines[route_idx:]

	for line in filtered_lines:
		if ">" in line:
			best = line
	
	if "1.1.1.1" in best:
		return (1 if test["Route1"]["NgbrAddr"] == '1.1.1.1' else 3)
	if "3.3.3.1" in best:
		return (1 if test["Route1"]["NgbrAddr"] == '3.3.3.1' else 3)
		
	