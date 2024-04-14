import yaml
import json
from prefix_list_conversion import convert_prefix_list_to_prefix_set
from argparse import ArgumentParser

parser = ArgumentParser()
parser.add_argument('-p', '--num-prefixes', type=int, default=3)
args = parser.parse_args()

with open("test.json", "r") as f:
    data = json.load(f)

print(data)

route = data[1]
data = data[0]

pl_flag = False
com_flag = False
as_flag = False

route_map = [
    {
        "match": {},
        "set": {},
        "permit": True if data["RmapPD"] == "True" else False
    }
]

prefix_list = []
if data["Prefix"] == "Some":
    for i in range(args.num_prefixes):
        p0 = {
            "prefix": data["Prefix" + str(i)].split("/")[0],
            "mask": int(data["Prefix" + str(i)].split("/")[1]),
            "le":  32 if data["LE" + str(i)] == "None" else int(data["LE" + str(i)]),
            "ge":  int(data["Prefix" + str(i)].split("/")[1]) if data["GE" + str(i)] == "None" else int(data["GE" + str(i)]),
            "permit": True if data["PrefixPD" + str(i)]=="True" else False 
        }
        if data["LE" + str(i)] == "None" and data["GE" + str(i)] == "None":
            p0["le"] = p0["mask"]
            p0["ge"] = p0["mask"]

        prefix_list.append(p0)
    pl_flag = True

    print(prefix_list)
    prefix_list = convert_prefix_list_to_prefix_set(prefix_list)
    route_map[0]["match"]["Prefix List"] = "ps1"

print(prefix_list)

community_list = []
if data["Community"]=="Some":
    c = {
        "RegularExpression": data["Community-regex"],
        "permit": True if data["Community-permit"]=="True" else False
    }
    community_list.append(c)
    com_flag = True
    route_map[0]["match"]["Community List"] = "c1"

aspath_list = []
if data["ASPath"]=="Some":
    c = {
        "RegularExpression": data["ASPath-regex"],
        "permit": True if data["ASPath-permit"]=="True" else False
    }
    aspath_list.append(c)
    as_flag = True
    route_map[0]["match"]["As Path List"] = "a1"


if data["Set"]=="Some":
    if not data["SetCommunity"]=="None":
        route_map[0]["set"]["community"] = data["SetCommunity"]
    if not data["DeleteCommunity"]=="None":
        route_map[0]["set"]["delete-community"] = data["DeleteCommunity"]
    if not data["SetMED"]=="None":
        route_map[0]["set"]["med"] = data["SetMED"]
    if not data["SetLP"]=="None":
        route_map[0]["set"]["lp"] = data["SetLP"]
    if not data["ASPathPrepend"]=="None":
        route_map[0]["set"]["asp_prepend"] = data["ASPathPrepend"]
    if not data["NextHopIP"] == "None":
        route_map[0]["set"]["next-hop"] = data["NextHopIP"]
    if data["NextHopUnchanged"] == "True":
        route_map[0]["set"]["next-hop"] = "unchanged"



print("Route map: ", route_map)

############# GLOBAL CONFIGURATION ###########
d = dict()
d["global"] = dict()
d["global"]["config"] = {"as": 65001, "router-id": "192.2.3.4"}
d["global"]["apply-policy"] = {
    "config": {
        "import-policy-list": ["example-policy"],
        "default-import-policy": "reject-route"
    }
}



############ NEIGHBOR CONFIGURATION ###########
d["neighbors"] = [dict()]
d["neighbors"][0]["config"] = {
    "neighbor-address": "3.0.0.3",
    "peer-as": 65002,
}
d["neighbors"][0]["transport"] = {
    "config" : 
        {
            "local-address": "3.0.0.2"
        }
}
with open("gobgp1/gobgp.yml", "w") as f:
    yaml.dump(d, f)

######### PREFIX-LIST CONFIGURATION #######

prefix_list_toml = []
for p in prefix_list:
    d = dict()
    d["ip-prefix"] = p["prefix"] + "/" + str(p["mask"])
    le = str(p["le"]) if p["le"] is not None else "32"
    ge = str(p["ge"]) if p["ge"] is not None else str(p["mask"])
    d["masklength-range"] = ge + ".." + le
    prefix_list_toml.append(d)

d = dict()
d["defined-sets"] = dict()

if pl_flag:
    d["defined-sets"]["prefix-sets"] = {
                "prefix-set-name" : "ps1",
                "prefix-list": prefix_list_toml
            }


######## COMMUNITY LIST CONFIGURARTION ###########

community_list_toml = [c["RegularExpression"] for c in community_list]

if as_flag or com_flag:
    d["defined-sets"]["bgp-defined-sets"] = dict()

if com_flag:
    d["defined-sets"]["bgp-defined-sets"]["community-sets"]= {
                        "community-set-name": "c1",
                        "community-list": community_list_toml
                    }

####### AS PATH LIST CONFIGURATION ################
aspath_list_toml = [a["RegularExpression"] for a in aspath_list]

if as_flag:
    d["defined-sets"]["bgp-defined-sets"]["as-path-sets"] = {
                        "as-path-set-name": "a1",
                        "as-path-list": aspath_list_toml
                    }

####### ROUTE MAP CONFIGURATION ###################
d["policy-definitions"] = [dict()]
d["policy-definitions"][0]["name"] = "example-policy"

d["policy-definitions"][0]["statements"]= [dict() for i in range(len(route_map))]

for i in range(len(route_map)):
    d["policy-definitions"][0]["statements"][i]["name"] = "statement" + str(i+1)
    d["policy-definitions"][0]["statements"][i]["conditions"] = dict()
    for m in route_map[i]["match"].keys():
        if m == "As Path List":
            if "bgp-conditions" not in d["policy-definitions"][0]["statements"][i]["conditions"]:
                d["policy-definitions"][0]["statements"][i]["conditions"]["bgp-conditions"] = {"match-as-path-set" : {"as-path-set" : route_map[i]["match"][m], "match-set-options": ("any" if aspath_list[0]["permit"] is True else "invert")}}
            else:
                d["policy-definitions"][0]["statements"][i]["conditions"]["bgp-conditions"]["match-as-path-set"] = {"as-path-set" : route_map[i]["match"][m], "match-set-options": ("any" if aspath_list[0]["permit"] is True else "invert")}
        elif m == "Community List":
            if "bgp-conditions" in d["policy-definitions"][0]["statements"][i]["conditions"]:
                d["policy-definitions"][0]["statements"][i]["conditions"]["bgp-conditions"]["match-community-set"] = {"community-set" : route_map[i]["match"][m], "match-set-options": ("any" if community_list[0]["permit"] is True else "invert")}
            else:
                d["policy-definitions"][0]["statements"][i]["conditions"]["bgp-conditions"] = {"match-community-set" : {"community-set": route_map[i]["match"][m], "match-set-options": ("any" if community_list[0]["permit"] is True else "invert")}}
        elif m == "Prefix List":
            d["policy-definitions"][0]["statements"][i]["conditions"]["match-prefix-set"] = {"prefix-set": route_map[i]["match"][m]}

    print(route_map[i]["permit"])
    d["policy-definitions"][0]["statements"][i]["actions"] = {"route-disposition": "accept-route" if route_map[i]["permit"] else "reject-route"}

    for s in route_map[i]["set"].keys():
        if s == "med":
            if "bgp-actions" in d["policy-definitions"][0]["statements"][i]["actions"]:
                d["policy-definitions"][0]["statements"][i]["actions"]["bgp-actions"]["set-med"] = route_map[i]["set"][s]
            else:
                d["policy-definitions"][0]["statements"][i]["actions"]["bgp-actions"] = {"set-med": route_map[i]["set"][s]}
        if s == "community":
            if "bgp-actions" in d["policy-definitions"][0]["statements"][i]["actions"]:
                d["policy-definitions"][0]["statements"][i]["actions"]["bgp-actions"]["set-community"] = {"options": "add", 
                                                                                                                    "set-community-method": { 
                                                                                                                        "communities-list": [route_map[i]["set"][s]]
                                                                                                                    }
                                                                                                                }
            else:
                d["policy-definitions"][0]["statements"][i]["actions"]["bgp-actions"] = {"set-community": {"options": "add", "set-community-method": { 
                                                                                                                        "communities-list": [route_map[i]["set"][s]]
                                                                                                                    }}}
        elif s == "lp":
            if "bgp-actions" in d["policy-definitions"][0]["statements"][i]["actions"]:
                d["policy-definitions"][0]["statements"][i]["actions"]["bgp-actions"]["set-local-pref"] = route_map[i]["set"][s]
            else:
                d["policy-definitions"][0]["statements"][i]["actions"]["bgp-actions"] = {"set-local-pref": route_map[i]["set"][s]}

        elif s == "asp_prepend":
            if "bgp-actions" in d["policy-definitions"][0]["statements"][i]["actions"]:
                d["policy-definitions"][0]["statements"][i]["actions"]["bgp-actions"]["set-as-path-prepend"] = {
                    "as" : route_map[i]["set"][s],
                    "repeat-n": 1
                }
            else:
                d["policy-definitions"][0]["statements"][i]["actions"]["bgp-actions"] = {"set-as-path-prepend": {
                    "as" : route_map[i]["set"][s],
                    "repeat-n": 1
                }}

        elif s == "next-hop":
            if "bgp-actions" in d["policy-definitions"][0]["statements"][i]["actions"]:
                d["policy-definitions"][0]["statements"][i]["actions"]["bgp-actions"]["set-next-hop"] = route_map[i]["set"][s]
            else:
                d["policy-definitions"][0]["statements"][i]["actions"]["bgp-actions"] = {"set-next-hop": route_map[i]["set"][s]}

        elif s == "delete-community":
            if "bgp-actions" in d["policy-definitions"][0]["statements"][i]["actions"]:
                d["policy-definitions"][0]["statements"][i]["actions"]["bgp-actions"]["set-community"] = {"options": "remove", "set-community-method": { 
                                                                                                                        "communities-list": [route_map[i]["set"][s]]
                                                                                                                    }}
            else:
                d["policy-definitions"][0]["statements"][i]["actions"]["bgp-actions"] = {"set-community": {"options": "remove", "set-community-method": { 
                                                                                                                        "communities-list": [route_map[i]["set"][s]]
                                                                                                                    }}}
print(d)
print(d["policy-definitions"][0]["statements"][0]["actions"])

with open("gobgp1/gobgp.yml", "a") as f:
    yaml.dump(d, f)

with open('exabgp1/example.working.py','r') as f:
        lines = f.readlines()

with open('exabgp1/example.py','w') as e:
    for i,line in enumerate(lines):
        if i == 7:
            e.write(f"    'announce route {route['Prefix']} next-hop {route['NextHop']} local-preference {route['LP']} as-path [{route['ASPath']}] med {route['MED']} origin egp community [{route['Community']}]',\n")
        else:
            e.write(line)




