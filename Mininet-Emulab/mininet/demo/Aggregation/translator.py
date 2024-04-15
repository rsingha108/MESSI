"""
{
  "Router": {
    "AggregateRoute": "Prefix: 99.190.4.128/0, Local Preference: 0, MED: 0, Community: [], AS Path: [], Next Hop: 0.0.0.0",
    "SummaryOnly": "False",
    "MatchingMEDOnly": "False",
    "AS": "100"
  },
  "Route1": {
    "Prefix": "100.0.0.0/0",
    "LP": "160",
    "MED": "36",
    "Community": "69:4336 9802:656",
    "ASPath": "1",
    "NextHop": "99.160.0.3"
  },
  "Route2": {
    "Prefix": "100.0.16.3/0",
    "LP": "133",
    "MED": "48",
    "Community": "22:2 21212:1",
    "ASPath": "2 2",
    "NextHop": "100.0.1.3"
  },
  "output": [
    {
      "Prefix": "99.190.4.128/0",
      "LP": "0",
      "MED": "0",
      "Community": "",
      "ASPath": "",
      "NextHop": "0.0.0.0"
    },
    {
      "Prefix": "100.0.0.0/0",
      "LP": "160",
      "MED": "36",
      "Community": "69:4336 9802:656",
      "ASPath": "1",
      "NextHop": "99.160.0.3"
    },
    {
      "Prefix": "100.0.16.3/0",
      "LP": "133",
      "MED": "48",
      "Community": "22:2 21212:1",
      "ASPath": "2 2",
      "NextHop": "100.0.1.3"
    }
  ]
}
"""
"""
d1 = {"NH" : nh1, "AS" : "43617", "IP" : "10.100.1.0/24", "LP" : "200", "ASP" : "[55]", "MED" : "5", "ORG" : "egp", "COM" : "[2:3]"}
d2 = {"IP1" : ip1, "IP4" : ip4, "IP3" : ip3, "AS" : "43618", "IP" : "99.190.4.128/0"}
d3 = {"NH" : nh3, "AS" : "43619", "IP" : "10.100.2.0/24", "LP" : "200", "ASP" : "[55]", "MED" : "3", "ORG" : "egp", "COM" : "[2:3]"}
d4 = {"NH" : nh4, "AS" : "43620"}
"""

nh1 = "1.1.1.1"
ip1 = "1.1.1.2"
nh4 = "4.4.4.1"
ip4 = "4.4.4.2"
nh3 = "3.3.3.1"
ip3 = "3.3.3.2"

def extract_agg_prefix(aggregate_route):
    # Split the value based on the comma separator
    parts = aggregate_route.split(",")
    # Iterate over the parts to find the one containing the prefix
    for part in parts:
        if "Prefix" in part:
            # Extract the prefix
            prefix = part.split(":")[1].strip()
            break
    return prefix

def translate(data):
    d1 = {"NH" : nh1, "AS" : "43617", "IP" : data["Route1"]["Prefix"], "LP" : data["Route1"]["LP"], "ASP" : f"[{data['Route1']['ASPath']}]", "MED" : data["Route1"]["MED"], "ORG" : "egp", "COM" : f"[{data['Route1']['Community']}]"}
    agg_prefix = extract_agg_prefix(data["Router"]["AggregateRoute"])
    d2 = {"IP1" : ip1, "IP4" : ip4, "IP3" : ip3, "AS" : "43618", "Agg" : agg_prefix, "SummaryOnly": data["Router"]["SummaryOnly"], "MatchingMEDOnly": data["Router"]["MatchingMEDOnly"]}
    d3 = {"NH" : nh3, "AS" : "43619", "IP" : data["Route2"]["Prefix"], "LP" : data["Route2"]["LP"], "ASP" : f"[{data['Route2']['ASPath']}]", "MED" : data["Route2"]["MED"], "ORG" : "egp", "COM" : f"[{data['Route2']['Community']}]"}
    d4 = {"NH" : nh4, "AS" : "43620"}
    ## determine if the aggregate route is in output
    dec = False
    for d in data["output"]:
        if d["Prefix"] == agg_prefix:
            dec = True

    return d1, d2, d3, d4, dec
