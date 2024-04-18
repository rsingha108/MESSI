"""
{
  "RouterAS": 100,
  "Route1": {
    "LP": 257,
    "ASPathLength": 2,
    "Origin": "e",
    "MED": 173,
    "ASN": 192,
    "IGP": 521,
    "RID": "99.160.1.3",
    "NgbrAddr": "3.3.3.1",
    "ArrivalTime": 0
  },
  "Route2": {
    "LP": 173,
    "ASPathLength": 1,
    "Origin": "i",
    "MED": 109,
    "ASN": 189,
    "IGP": 521,
    "RID": "99.190.1.3",
    "NgbrAddr": "1.1.1.1",
    "ArrivalTime": 0
  },
  "Decision": {
    "LP": 257,
    "ASPathLength": 2,
    "Origin": "e",
    "MED": 173,
    "ASN": 192,
    "IGP": 521,
    "RID": "99.160.1.3",
    "NgbrAddr": "3.3.3.1",
    "ArrivalTime": 0
  }
}
"""

def generate_subnet_ips(test):
	nh1 = test["Route1"]["NgbrAddr"] ## 3.3.3.1
	ip1 = ".".join(nh1.split(".")[:3] + ["2"])
	nh3 = test["Route2"]["NgbrAddr"] 
	ip3 = ".".join(nh3.split(".")[:3] + ["2"])
	nh4 = "4.4.4.1"
	ip4 = "4.4.4.2"
	return (nh1, ip1, nh3, ip3, nh4, ip4)
