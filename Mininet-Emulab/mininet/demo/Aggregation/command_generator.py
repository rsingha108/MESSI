def cmd_gen(d1,d2,d3,d4,cs,sw,cnsl):

    nw1 = d1["NH"].split('.')[0]+'.0.0.0'
    nw3 = d3["NH"].split('.')[0]+'.0.0.0'
    nw4 = d4["NH"].split('.')[0]+'.0.0.0'

    with open('generated_commands.txt','w') as g:
	
        ## CONFIG OF S2 (ROUTER)
        g.write(f's2{cs} {cnsl} -c "conf t" -c "debug bgp updates" -c "log file /var/log/{sw}/bgpd.log" -c "router bgp {d2["AS"]}" -c "neighbor {d1["NH"]} remote-as {d1["AS"]}" -c "neighbor {d3["NH"]} remote-as {d3["AS"]}" -c "neighbor {d4["NH"]} remote-as {d4["AS"]}" -c "network {nw1}" -c "network {nw3}" -c "network {nw4}"\n') 
        g.write(f's2{cs} {cnsl} -c "conf t" -c "router bgp {d2["AS"]}" -c "neighbor {d1["NH"]} soft-reconfiguration inbound" -c "neighbor {d3["NH"]} soft-reconfiguration inbound" -c "neighbor {d4["NH"]} soft-reconfiguration inbound"\n')
        g.write(f's2{cs} {cnsl} -c "conf t" -c "router bgp {d2["AS"]}" -c "no bgp ebgp-requires-policy"\n')

        config = f's2{cs} {cnsl} -c "conf t" -c "router bgp {d2["AS"]}" -c "aggregate-address {d2["Agg"]}'
        if (d2["SummaryOnly"]=="True"):
            config += ' summary-only'
        if (d2["MatchingMEDOnly"]=="True"):
            config += ' matching-MED-only'
        config += '"\n'
        # g.write(f's2{cs} {cnsl} -c "conf t" -c "router bgp {d2["AS"]}" -c "aggregate-address {d2["Agg"]} {summary-only} {matching-MED-only}"\n')
        g.write(config)
       
        ## CONFIG OF S4 (ROUTER)
        g.write(f's4{cs} {cnsl} -c "conf t" -c "debug bgp updates" -c "log file /var/log/{sw}/bgpd.log" -c "router bgp {d4["AS"]}" -c "neighbor {d2["IP4"]} remote-as {d2["AS"]}" -c "network {nw4}"\n') 
        g.write(f's4{cs} {cnsl} -c "conf t" -c "router bgp {d4["AS"]}" -c "neighbor {d2["IP4"]} soft-reconfiguration inbound"\n')
        g.write(f's4{cs} {cnsl} -c "conf t" -c "router bgp {d4["AS"]}" -c "no bgp ebgp-requires-policy"\n')

        ## CONFIG OF S1 (EXABGP)
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
        g.write(f's1{cs} echo -e "\\t\'announce route {d1["IP"]} next-hop self local-preference {d1["LP"]} as-path {d1["ASP"]} med {d1["MED"]} origin {d1["ORG"]} community {d1["COM"]}\'," >> example.py\n')
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
        g.write('py time.sleep(5)\n')
        g.write(f's2{cs} {cnsl} -c "show ip bgp"\n')

        ## CONFIG OF S3 (EXABGP)
        g.write(f's3{cs} mkdir exabgp\n')
        g.write(f's3{cs} cd exabgp\n')
        g.write(f's3{cs} echo "process announce-routes{{" >> conf.ini\n')
        g.write(f's3{cs} echo -e "\\trun python exabgp/example.py;" >> conf.ini\n')
        g.write(f's3{cs} echo -e "\\tencoder json;" >> conf.ini\n')
        g.write(f's3{cs} echo "}}" >> conf.ini\n')
        g.write(f's3{cs} echo "neighbor {d2["IP3"]}')
        g.write('{" >> conf.ini\n')
        g.write(f's3{cs} echo -e "\\trouter-id {d3["NH"]};" >> conf.ini\n')
        g.write(f's3{cs} echo -e "\\tlocal-address {d3["NH"]};" >> conf.ini\n')
        g.write(f's3{cs} echo -e "\\tlocal-as {d3["AS"]};" >> conf.ini\n')
        g.write(f's3{cs} echo -e "\\tpeer-as {d2["AS"]};" >> conf.ini\n')
        g.write(f's3{cs} echo -e "\\tapi{{" >> conf.ini\n')
        g.write(f's3{cs} echo -e "\\t\\tprocesses [announce-routes];" >> conf.ini\n')
        g.write(f's3{cs} echo -e "\\t}}" >> conf.ini\n')
        g.write(f's3{cs} echo -e "}}" >> conf.ini\n')
        g.write(f's3{cs} echo "from __future__ import print_function" >> example.py\n')
        g.write(f's3{cs} echo "from sys import stdout" >> example.py\n')
        g.write(f's3{cs} echo "from time import sleep" >> example.py\n')
        g.write(f's3{cs} echo "messages = [" >> example.py\n')
        g.write(f's3{cs} echo -e "\\t\'announce route {d3["IP"]} next-hop self local-preference {d3["LP"]} as-path {d3["ASP"]} med {d3["MED"]} origin {d3["ORG"]} community {d3["COM"]}\'," >> example.py\n')
        g.write(f's3{cs} echo "]" >> example.py\n')
        g.write(f's3{cs} echo "sleep(5)" >> example.py\n')
        g.write(f's3{cs} echo "for message in messages:" >> example.py\n')
        g.write(f's3{cs} printf "\\tstdout.write(message + \'\\\\\\n\')\\n" >> example.py\n')
        g.write(f's3{cs} echo -e "\\tstdout.flush()" >> example.py\n')
        g.write(f's3{cs} echo -e "\\tsleep(1)" >> example.py\n')
        g.write(f's3{cs} echo "while True:" >> example.py\n')
        g.write(f's3{cs} echo -e "\\tsleep(1)" >> example.py\n')
        g.write(f's3{cs} cd ..\n')
        g.write(f'py s3{cs}.cmd("exabgp exabgp/conf.ini &")\n')
        g.write('py time.sleep(5)\n')

        g.write(f's2{cs} vtysh -c "clear ip bgp * soft"\n')
        g.write('py time.sleep(5)\n')
        g.write(f's4{cs} vtysh -c "clear ip bgp * soft"\n')
        g.write('py time.sleep(5)\n')
        # g.write(f's2{cs} {cnsl} -c "show ip bgp" >> /mnt/Aggregation/out.txt\n')
        g.write(f's2{cs} {cnsl} -c "show running-config"\n')
        g.write(f's2{cs} {cnsl} -c "show ip bgp"\n')
        g.write(f's4{cs} {cnsl} -c "show running-config"\n')
        g.write(f's4{cs} {cnsl} -c "show ip bgp"\n')
        g.write(f's4{cs} {cnsl} -c "show ip bgp {d2["Agg"]}" >> /mnt/Aggregation/out.txt \n')
        

        # g.write(f's2{cs} cat /var/log/{sw}/bgpd.log >> /mnt/Symb-Route-maps/log.txt \n')


## WRITE HERE A FUNCTION : change_exabgp()
def change_exabgp():
    # Stop the running exabgp: py s14.cmd("pkill exabgp")
    # to check if it is running/stopped: py s14.cmd("pgrep exabgp")
    # Change the exabgp configuration file
    # Start the exabgp: py s14.cmd("exabgp exabgp/conf.ini &")
    # currently we do it manually for each test case
    pass
