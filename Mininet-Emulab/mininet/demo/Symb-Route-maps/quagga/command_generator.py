def cmd_gen(d1,d2,cs,sw,cnsl,rmap,route):

    PD = {"False":"deny","True":"permit"}
    nw1 = d1["NH"].split('.')[0]+'.0.0.0'

    with open('generated_commands.txt','w') as g:
	
        # g.write(f's2{cs} {cnsl} -c "conf t" -c "debug bgp updates" -c "log file /var/log/{sw}/bgpd.log" -c "router bgp {d2["AS"]}" -c "no bgp ebgp-requires-policy" -c "neighbor {d1["NH"]} remote-as {d1["AS"]}" -c "network {nw1}" -c "network 2.0.0.0" -c "neighbor {d1["NH"]} soft-reconfiguration inbound"\n') # -c "debug bgp updates", -c "log file /var/log/frr/bgpd.log", -c "bgp bestpath compare-routerid"
        g.write(f's2{cs} {cnsl} -c "conf t" -c "debug bgp updates" -c "log file /var/log/{sw}/bgpd.log" -c "router bgp {d2["AS"]}" -c "neighbor {d1["NH"]} remote-as {d1["AS"]}" -c "network {nw1}" -c "network 2.0.0.0" -c "neighbor {d1["NH"]} soft-reconfiguration inbound"\n') 

        g.write(f's1{cs} mkdir exabgp\n')

        g.write(f's1{cs} cd exabgp\n')
        g.write(f's1{cs} echo "process announce-routes{{" >> conf.ini\n')
        g.write(f's1{cs} echo -e "\\trun python exabgp/example.py;" >> conf.ini\n')
        g.write(f's1{cs} echo -e "\\tencoder json;" >> conf.ini\n')
        g.write(f's1{cs} echo "}}" >> conf.ini\n')


        g.write(f's1{cs} echo "neighbor {d2["IP1"]}')
        g.write('{" >> conf.ini\n')
        g.write(f's1{cs} echo -e "\\trouter-id {d1["NH"]};" >> conf.ini\n')
        g.write(f's1{cs} echo -e "\\tlocal-address {d1["NH"]};" >> conf.ini\n')
        g.write(f's1{cs} echo -e "\\tlocal-as {d1["AS"]};" >> conf.ini\n')
        g.write(f's1{cs} echo -e "\\tpeer-as {d2["AS"]};" >> conf.ini\n')
        g.write(f's1{cs} echo -e "\\tapi{{" >> conf.ini\n')
        g.write(f's1{cs} echo -e "\\t\\tprocesses [announce-routes];" >> conf.ini\n')
        g.write(f's1{cs} echo -e "\\t}}" >> conf.ini\n')
        g.write(f's1{cs} echo -e "}}" >> conf.ini\n')


        g.write(f's1{cs} echo "from __future__ import print_function" >> example.py\n')
        g.write(f's1{cs} echo "from sys import stdout" >> example.py\n')
        g.write(f's1{cs} echo "from time import sleep" >> example.py\n')

        g.write(f's1{cs} echo "messages = [" >> example.py\n')
        g.write(f's1{cs} echo -e "\\t\'announce route {d1["IP"]} next-hop {route["NextHop"]} local-preference {d1["LP"]} as-path [{d1["ASP"]}] med {d1["MED"]} origin {d1["ORG"]} community [{d1["COM"]}]\'," >> example.py\n')

        g.write(f's1{cs} echo "]" >> example.py\n')

        g.write(f's1{cs} echo "sleep(5)" >> example.py\n')

        g.write(f's1{cs} echo "for message in messages:" >> example.py\n')
        g.write(f's1{cs} printf "\\tstdout.write(message + \'\\\\\\n\')\\n" >> example.py\n')
        g.write(f's1{cs} echo -e "\\tstdout.flush()" >> example.py\n')
        g.write(f's1{cs} echo -e "\\tsleep(1)" >> example.py\n')

        g.write(f's1{cs} echo "while True:" >> example.py\n')
        g.write(f's1{cs} echo -e "\\tsleep(1)" >> example.py\n')

        g.write(f's1{cs} cd ..\n')
        g.write(f'py s1{cs}.cmd("exabgp exabgp/conf.ini &")\n')
        g.write('py time.sleep(10)\n')

        # g.write(f's2{cs} {cnsl} -c "show ip bgp" >> /mnt/Route-maps/out_log_{sw}.txt\n')

        # g.write('s2{cs} vtysh -c "conf t" -c "access-list 50 permit 100.10.0.0/24"\n')
        # g.write('s2{cs} vtysh -c "conf t" -c "route-map RM1 deny 10" -c "match ip address 50"\n')
        # g.write('s2{cs} vtysh -c "conf t" -c "route-map RM1 permit 20"\n')
        # g.write('s2{cs} vtysh -c "conf t" -c "route-map RM3 deny 10" -c "match local-preference 135"\n')
        # g.write('s2{cs} vtysh -c "conf t" -c "route-map RM3 permit 20"\n')

        # with open('rmconfig.txt','r') as rmfile: 
        # 	statements = rmfile.readlines()
        # for statement in statements:
        # 	g.write(f's2{cs} vtysh -c "conf t" {statement}\n')

        # with open('rmap.txt','r') as f:
        # 	lines = f.readlines()	

        if rmap["Community"] != "None":
            g.write(f's2{cs} vtysh -c "conf t" -c "ip community-list 100 {PD[rmap["Community-permit"]]} {rmap["Community-regex"]}"\n')

        if rmap["DeleteCommunity"] != "None":
            g.write(f's2{cs} vtysh -c "conf t" -c "ip community-list expanded DEL permit {rmap["DeleteCommunity"]}"\n')

        if rmap["Prefix"] != "None":
            g.write(f's2{cs} vtysh -c "conf t" -c "ip prefix-list PFXL {PD[rmap["PrefixPD0"]]} {rmap["Prefix0"]}')
            if rmap["LE0"] != "None":
                g.write(f' le {rmap["LE0"]}')
            if rmap["GE0"] != "None":
                g.write(f' ge {rmap["GE0"]}')
            g.write('"\n')

            g.write(f's2{cs} vtysh -c "conf t" -c "ip prefix-list PFXL {PD[rmap["PrefixPD1"]]} {rmap["Prefix1"]}')
            if rmap["LE1"] != "None":
                g.write(f' le {rmap["LE1"]}')
            if rmap["GE1"] != "None":
                g.write(f' ge {rmap["GE1"]}')
            g.write('"\n')

            g.write(f's2{cs} vtysh -c "conf t" -c "ip prefix-list PFXL {PD[rmap["PrefixPD2"]]} {rmap["Prefix2"]}')
            if rmap["LE2"] != "None":
                g.write(f' le {rmap["LE2"]}')
            if rmap["GE2"] != "None":
                g.write(f' ge {rmap["GE2"]}')
            g.write('"\n')
            
        if rmap["ASPath"] != "None":
            g.write(f's2{cs} vtysh -c "conf t" -c "ip as-path access-list 99 {PD[rmap["ASPath-permit"]]} {rmap["ASPath-regex"]}"\n')

        
        
        
        ## all match statements first (AND of them) then set statements 
        g.write(f's2{cs} vtysh -c "conf t" -c "route-map RMap {PD[rmap["RmapPD"]]} 10"')
        if rmap["Prefix"] != "None":
            g.write(f' -c "match ip address prefix-list PFXL"')
        if rmap["LP"] != "None":
            g.write(f' -c "match local-preference {rmap["LP"]}"')
        if rmap["MED"] != "None":
            g.write(f' -c "match metric {rmap["MED"]}"')
        if rmap["Community"] != "None":
            g.write(f' -c "match community 100"')
        if rmap["ASPath"] != "None":
            g.write(f' -c "match as-path 99"')
        if rmap["SetLP"] != "None":
            g.write(f' -c "set local-preference {rmap["SetLP"]}"')
        if rmap["SetMED"] != "None":
            g.write(f' -c "set metric {rmap["SetMED"]}"')
        if rmap["SetCommunity"] != "None":
            g.write(f' -c "set community additive {rmap["SetCommunity"]}"')
        if rmap["DeleteCommunity"] != "None":
            g.write(f' -c "set comm-list DEL delete"')
        if rmap["ASPathPrepend"] != "None":
            g.write(f' -c "set as-path prepend {rmap["ASPathPrepend"]}"')
        if rmap["ASPathExclude"] != "None":
            g.write(f' -c "set as-path exclude {rmap["ASPathExclude"]}"')
        if rmap["NextHopIP"] != "None":
            g.write(f' -c "set ip next-hop {rmap["NextHopIP"]}"')
        if rmap["NextHopUnchanged"] != "False":
            g.write(f' -c "set ip next-hop unchanged"')
        if rmap["NextHopPeer"] != "False":
            g.write(f' -c "set ip next-hop peer-address"')
        g.write('\n')

        g.write(f's2{cs} vtysh -c "conf t" -c "router bgp {d2["AS"]}" -c "neighbor {d1["NH"]} route-map RMap in" -c "end" -c "exit"\n')
        g.write(f's2{cs} vtysh -c "clear ip bgp * soft"\n')
        g.write('py time.sleep(5)\n')
        g.write(f's2{cs} {cnsl} -c "show ip bgp {d1["IP"]}" >> /mnt/Symb-Route-maps/{sw}/out.txt\n')
        # g.write(f's2{cs} {cnsl} -c "show ip bgp {d1["IP"]}"\n')
        g.write(f's2{cs} {cnsl} -c "show running-config" >> /mnt/Symb-Route-maps/{sw}/running-config.txt\n')
        # g.write(f's2{cs} cat /var/log/{sw}/bgpd.log >> /mnt/Symb-Route-maps/log.txt \n')

def diff_rmaps(rmap1, rmap2):
    diff = []
    for k in rmap1.keys():
        if rmap1[k] != rmap2[k]:
            diff.append(k)
    refined_diff = []
    ## if any of ("Prefix", "Prefix0", "LE0", "GE0", "PrefixPD0", "Prefix1", "LE1", "GE1", "PrefixPD1", "Prefix2", "LE2", "GE2", "PrefixPD2") in diff then add "Prefix"
    # if any(x in diff for x in ("Prefix", "Prefix0", "LE0", "GE0", "PrefixPD0", "Prefix1", "LE1", "GE1", "PrefixPD1", "Prefix2", "LE2", "GE2", "PrefixPD2")):
    #     refined_diff.append("Prefix-list")
    if any(x in diff for x in ("Prefix0", "LE0", "GE0", "PrefixPD0")):
        refined_diff.append("Prefix0")
    if any(x in diff for x in ("Prefix1", "LE1", "GE1", "PrefixPD1")):
        refined_diff.append("Prefix1")
    if any(x in diff for x in ("Prefix2", "LE2", "GE2", "PrefixPD2")):
        refined_diff.append("Prefix2")
    if any(x in diff for x in ("Community", "Community-regex", "Community-permit")):
        refined_diff.append("Community-list")
    if any(x in diff for x in ("ASPath", "ASPath-regex", "ASPath-permit")):
        refined_diff.append("ASPath-list")
    if "DeleteCommunity" in diff:
        refined_diff.append("Delcom-list")
    if any(x in diff for x in ("RmapPD", "LP", "MED", "SetLP", "SetMED", "SetCommunity", "ASPathPrepend", "ASPathExclude", "NextHopIP", "NextHopUnchanged", "NextHopPeer")):
        refined_diff.append("Route-map")
    return refined_diff

def append_rmap_changes(d1,d2,cs,sw,cnsl,rmap,rmap_old):
    PD = {"False":"deny","True":"permit"}
    with open('generated_commands.txt','a') as g:
        ## only change the diff ones
        ## first do s2 vtysh -c "conf t" -c "no bgp community-list 101 permit [1-2]*:[1]"
        ## then do s2 vtysh -c "conf t" -c "bgp community-list 101 permit [1-2]*:[1]"
        diffs = diff_rmaps(rmap,rmap_old)
        for diff in diffs:
            if diff == "Prefix0":
                if rmap["Prefix0"] == "None":
                    g.write(f's2{cs} vtysh -c "conf t" -c "no ip prefix-list PFXL seq 5"\n')
                else:
                    g.write(f's2{cs} vtysh -c "conf t" -c "ip prefix-list PFXL seq 5 {PD[rmap["PrefixPD0"]]} {rmap["Prefix0"]}')
                if rmap["LE0"] != "None":
                    g.write(f' le {rmap["LE0"]}')
                if rmap["GE0"] != "None":
                    g.write(f' ge {rmap["GE0"]}')
                g.write('"\n')
            elif diff == "Prefix1":
                if rmap["Prefix1"] == "None":
                    g.write(f's2{cs} vtysh -c "conf t" -c "no ip prefix-list PFXL seq 10"\n')
                else:
                    g.write(f's2{cs} vtysh -c "conf t" -c "ip prefix-list PFXL seq 10 {PD[rmap["PrefixPD1"]]} {rmap["Prefix1"]}')
                if rmap["LE1"] != "None":
                    g.write(f' le {rmap["LE1"]}')
                if rmap["GE1"] != "None":
                    g.write(f' ge {rmap["GE1"]}')
                g.write('"\n')
            elif diff == "Prefix2":
                if rmap["Prefix2"] == "None":
                    g.write(f's2{cs} vtysh -c "conf t" -c "no ip prefix-list PFXL seq 15"\n')
                else:
                    g.write(f's2{cs} vtysh -c "conf t" -c "ip prefix-list PFXL seq 15 {PD[rmap["PrefixPD2"]]} {rmap["Prefix2"]}')
                if rmap["LE2"] != "None":
                    g.write(f' le {rmap["LE2"]}')
                if rmap["GE2"] != "None":
                    g.write(f' ge {rmap["GE2"]}')
                g.write('"\n')
            elif diff == "Community-list":
                g.write(f's2{cs} vtysh -c "conf t" -c "no ip community-list 100"\n')
                if rmap["Community"] != "None":
                    g.write(f's2{cs} vtysh -c "conf t" -c "ip community-list 100 {PD[rmap["Community-permit"]]} {rmap["Community-regex"]}"\n')
            elif diff == "Delcom-list":
                g.write(f's2{cs} vtysh -c "conf t" -c "no ip community-list expanded DEL"\n')
                if rmap["DeleteCommunity"] != "None":
                    g.write(f's2{cs} vtysh -c "conf t" -c "ip community-list expanded DEL permit {rmap["DeleteCommunity"]}"\n')
            elif diff == "ASPath-list":
                g.write(f's2{cs} vtysh -c "conf t" -c "no ip as-path access-list 99"\n')
                if rmap["ASPath"] != "None":
                    g.write(f's2{cs} vtysh -c "conf t" -c "ip as-path access-list 99 {PD[rmap["ASPath-permit"]]} {rmap["ASPath-regex"]}"\n')
            elif diff == "Route-map":
                g.write(f's2{cs} vtysh -c "conf t" -c "route-map RMap {PD[rmap["RmapPD"]]} 10"')
                if rmap["Prefix"] != "None":
                    g.write(f' -c "match ip address prefix-list PFXL"')
                if rmap["LP"] != "None":
                    g.write(f' -c "match local-preference {rmap["LP"]}"')
                if rmap["MED"] != "None":
                    g.write(f' -c "match metric {rmap["MED"]}"')
                if rmap["Community"] != "None":
                    g.write(f' -c "match community 100"')
                if rmap["ASPath"] != "None":
                    g.write(f' -c "match as-path 99"')
                if rmap["SetLP"] != "None":
                    g.write(f' -c "set local-preference {rmap["SetLP"]}"')
                if rmap["SetMED"] != "None":
                    g.write(f' -c "set metric {rmap["SetMED"]}"')
                if rmap["SetCommunity"] != "None":
                    g.write(f' -c "set community additive {rmap["SetCommunity"]}"')
                if rmap["DeleteCommunity"] != "None":
                    g.write(f' -c "set comm-list DEL delete"')
                if rmap["ASPathPrepend"] != "None":
                    g.write(f' -c "set as-path prepend {rmap["ASPathPrepend"]}"')
                if rmap["ASPathExclude"] != "None":
                    g.write(f' -c "set as-path exclude {rmap["ASPathExclude"]}"')
                if rmap["NextHopIP"] != "None":
                    g.write(f' -c "set ip next-hop {rmap["NextHopIP"]}"')
                if rmap["NextHopUnchanged"] != "False":
                    g.write(f' -c "set ip next-hop unchanged"')
                if rmap["NextHopPeer"] != "False":
                    g.write(f' -c "set ip next-hop peer-address"')
            
            g.write('\n')

            ## APPLY ROUTE MAP
            g.write(f's2{cs} {cnsl} -c "clear ip bgp *"\n')
            g.write('py time.sleep(20)\n')
            g.write(f's2{cs} {cnsl} -c "show ip bgp {d1["IP"]}" >> /mnt/Symb-Route-maps/{sw}/out2.txt\n')
            # g.write(f's2{cs} {cnsl} -c "show ip bgp {d1["IP"]}"\n')
            g.write(f's2{cs} {cnsl} -c "show running-config" >> /mnt/Symb-Route-maps/{sw}/running-config2.txt\n')
            # g.write(f's2{cs} cat /var/log/{sw}/bgpd.log >> /mnt/Symb-Route-maps/log.txt \n')


