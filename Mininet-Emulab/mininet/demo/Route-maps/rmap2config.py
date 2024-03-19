import argparse
parser = argparse.ArgumentParser()
parser.add_argument("--software", type=str, default="frr")
args = parser.parse_args()

with open('rmap.txt','r') as f:
    lines = f.readlines()

rmap_name = lines[0][:-1]
print(rmap_name)

PD = {'True':'permit', 'False':'deny'}
comm = {'frr':'bgp', 'quagga':'ip'} # same for as-path
rm_counter = 1
com_counter = 100
asp_counter = 1

rm_entries = []
for line in lines[1:-1]:
    line = line[:-1]
    sp = line.split('\t')
    print(sp)
    print(rm_entries)
    print('\n')
    if sp[1] == "Local_Preference":
        rm_entries.append(f'-c "route-map {rmap_name} {PD[sp[0]]} {10*rm_counter}" -c "match local-preference {sp[2]}"\n')
        rm_counter += 1
        
    if sp[1] == "MED":
        rm_entries.append(f'-c "route-map {rmap_name} {PD[sp[0]]} {10*rm_counter}" -c "match metric {sp[2]}"\n')
        rm_counter += 1
        
    if sp[1] == "Community_List":    
        com_list = [s.split(',') for s in sp[2].split(';')]
        #-c "bgp community-list 1 permit 0:80 0:90"
        com_entries = []
        for com in com_list:
            com_entries.append(f'-c "{comm[args.software]} community-list {com_counter} {PD[com[1]]} {com[0]}"\n')
        rm_entries = com_entries + rm_entries

        rm_entries.append(f'-c "route-map {rmap_name} {PD[sp[0]]} {10*rm_counter}" -c "match community {com_counter}"\n')
        rm_counter += 1

        com_counter += 1
        
    if sp[1] == "Prefix_List": #ip prefix-list EXIST seq 5 permit 10.10.10.10/32
        pref_list = [s.split(',') for s in sp[2].split(';')]
        pref_entries = []
        for pref in pref_list:
            pref_entries.append(f'-c "ip prefix-list PFXL {PD[pref[1]]} {pref[0]}"\n')
        rm_entries = pref_entries + rm_entries

        rm_entries.append(f'-c "route-map {rmap_name} {PD[sp[0]]} {10*rm_counter}" -c "match ip address prefix-list PFXL"\n')
        rm_counter += 1

    if sp[1] == "ASP_List": #bgp as-path access-list 1 permit _51108+_1299 
        asp_list = [s.split(',') for s in sp[2].split(';')]
        asp_entries = []
        for asp in asp_list:
            asp_str = asp[0].replace(' ','_')
            asp_entries.append(f'-c "{comm[args.software]} as-path access-list {asp_counter} {PD[asp[1]]} {asp_str}"\n')
        rm_entries = asp_entries + rm_entries

        rm_entries.append(f'-c "route-map {rmap_name} {PD[sp[0]]} {10*rm_counter}" -c "match as-path {asp_counter}"\n')
        rm_counter += 1

        asp_counter += 1

    if sp[1] == "Default":
        rm_entries.append(f'-c "route-map {rmap_name} permit {10*rm_counter}"\n')
        rm_counter += 1

with open('rmconfig.txt','w') as g:
    for ent in rm_entries:
        g.write(ent)
