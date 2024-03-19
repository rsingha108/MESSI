"""
#router id 10.0.0.10;

protocol kernel {
  metric 0;
  import none;
  learn;
  export all;
}
protocol device {
}
protocol direct {
}

filter rmap 
{
  if (bgp_path ~ [= 2 3 * =]) then {
  	bgp_local_pref = 215;
  	bgp_community.add((5,6));
    accept;
  }
  reject;
}

protocol bgp peere {
  local as 43617;
  neighbor 7.0.0.3 as 43617;
  import filter rmap;
  export none;
}
"""


import json
import regex_pos_neg
import utils

def generate(rmap, route):

    # pos = [["(0,1)", "(0,2)"], ["(0,1)", "(11,1)"], ["(0,1)", "(12,1)"]]
    com_regex_dict, as_regex_dict, delcom_dict = regex_pos_neg.to_dict()

    b = open('conf/peerb/etc/bird/bird.conf','w')

    ########################## TRANSLATE TRIVIAL ##########################

    b.write(f"#router id 10.0.010;\n\n")
    b.write(f"protocol kernel {{\n  metric 0;\n  import none;\n  learn;\n  export all;\n}}\n")
    b.write(f"protocol device {{\n}}\n")
    b.write(f"protocol direct {{\n}}\n")

    ########################## TRANSLATE MATCHES ##########################
    matches = []

    if rmap['Prefix'] != "None" :
        pref_match = []
        pref_match.append(f"(net ~ {utils.pref_maker(rmap['Prefix0'],rmap['GE0'],rmap['LE0'])})")
        pref_match.append(f"(net ~ {utils.pref_maker(rmap['Prefix1'],rmap['GE1'],rmap['LE1'])})")
        pref_match.append(f"(net ~ {utils.pref_maker(rmap['Prefix2'],rmap['GE2'],rmap['LE2'])})")
        matches.append("("+ " || ".join(pref_match) +")")

    if rmap['LP'] != "None" : 
        matches.append(f"(bgp_local_pref = {rmap['LP']})")

    if rmap['MED'] != "None" : 
        matches.append(f"(bgp_med = {rmap['MED']})")

    if rmap['Community'] != "None" : 
        pos = com_regex_dict[rmap["Community-regex"]]['pos']
        com_match = []
        for p in pos:
            p_match = []
            for i in p:
                p_match.append(f"({i} ~ bgp_community)")
            com_match.append("("+" && ".join(p_match)+")")
        matches.append("("+ " || ".join(com_match) +")")

    if rmap['ASPath'] != "None" :
        pos = as_regex_dict[rmap["ASPath-regex"]]['pos']
        asp_match = []
        for p in pos:
            asp_match.append(f"(bgp_path ~ [= {p} =])")
        matches.append("("+ " || ".join(asp_match) +")")

    # print(" && ".join(matches))

    b.write(f"filter rmap \n{{\n  if ( {' && '.join(matches)} ) then {{\n")

    ########################## TRANSLATE SETS ##########################

    if rmap['Set'] == 'Some':
        if rmap['SetLP'] != "None":
            b.write(f"\t\tbgp_local_pref = {rmap['SetLP']};\n")
        if rmap['SetMED'] != "None":
            b.write(f"\t\tbgp_med = {rmap['SetMED']};\n")
        if rmap['SetCommunity'] != "None":
            b.write(f"\t\tbgp_community.add({rmap['SetCommunity']});\n")
        if rmap['DeleteCommunity'] != "None":
            coms = delcom_dict[rmap['DeleteCommunity']]
            for c in coms:
                b.write(f"\t\tbgp_community.delete({c});\n")
        if rmap["ASPathPrepend"] != "None":
            b.write(f"\t\tbgp_path.prepend({rmap['ASPathPrepend']});\n")
        if rmap["ASPathExclude"] != "None":
            b.write(f"\t\tbgp_path.delete({rmap['ASPathExclude']});\n")
        if rmap["NextHopIP"] != "None":
            b.write(f"\t\tbgp_next_hop = {rmap['NextHopIP']};\n")
        if rmap["NextHopPeer"] == "True":
            pass
        if rmap["NextHopUnchanged"] == "True":
            pass


    rmap_pd = 'accept' if rmap['RmapPD'] == 'True' else 'reject'
    b.write(f"\t\t{rmap_pd};\n  }}\n  reject;\n}}\n")

    ########################## TRANSLATE TRIVIAL ##########################

    b.write(f"protocol bgp peere {{\n  local as 43617;\n  neighbor 7.0.0.3 as 43617;\n  import filter rmap;\n  export none;\n}}\n")
    b.close()

    ########################## TRANSLATE ROUTE in EXABGP ##########################

    with open('conf/peere/etc/exabgp/example.working.py','r') as f:
        lines = f.readlines()

    with open('conf/peere/etc/exabgp/example.py','w') as e:
        for i,line in enumerate(lines):
            if i == 7:
                e.write(f"    'announce route {route['Prefix']} next-hop {route['NextHop']} local-preference {route['LP']} as-path [{route['ASPath']}] med {route['MED']} origin egp community [{route['Community']}]',\n")
            else:
                e.write(line)

