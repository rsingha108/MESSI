### IMPORTS ###

import json
import time
from glob import glob
import subprocess
import parse
from prefix_list_conversion import convert_prefix_list_to_prefix_set
from time import sleep

### UTIL FUNCTIONS ###

def write_docker_commands_for_prefix_set(prefix_list):
    prefix_logs = open('prefix_logs.txt', 'w')
    subprocess.run(['docker', 'exec', '-it', 'gobgp_1', 'gobgp', 'policy', 'prefix', 'ps1'], stdout=prefix_logs, text=True)
    prefix_logs.close()
    
    docker_commands = open('new_docker_commands.sh', 'a')

    with open('prefix_logs.txt', 'r') as f:
        prefixes = f.readlines()

        prefixes = prefixes[1:]
        prefixes[0] = prefixes[0].replace('ps1', '    ')

        for p in prefixes:
            p = p[:-1]
            prefix, range = p.split()
            print(prefix, range)
            docker_commands.write('docker exec -it gobgp_1 gobgp policy prefix del ps1 ' + prefix + ' ' + range + '\n')
        
        for p in prefix_list:
            docker_commands.write('docker exec -it gobgp_1 gobgp policy prefix add ps1 ' + p['prefix'] + '/' + str(p['mask']) + ' ' + str(p['ge']) + '..' + str(p['le']) + '\n')           

    subprocess.run(['rm', '-rf', 'prefix_logs.txt'])
    docker_commands.close()

def write_docker_commands_for_as_path(as_path_regex1, as_path_pd1, as_path_regex2, as_path_pd2):
    docker_commands = open('new_docker_commands.sh', 'a')

    if as_path_regex1 != as_path_regex2:
        cmd1 = "docker exec -it gobgp_1 gobgp policy as-path del a1 " + as_path_regex1
        cmd2 = "docker exec -it gobgp_1 gobgp policy as-path add a1 " + as_path_regex2

        docker_commands.write(cmd1 + "\n" + cmd2 + "\n")
    
    if as_path_pd1 != as_path_pd2:
        cmd1 = "docker exec -it gobgp_1 gobgp policy statement statement1 del condition as-path a1\n"
        cmd2 = "docker exec -it gobgp_1 gobgp policy statement statement1 add condition as-path a1 " + ("any" if as_path_pd2=="True" else "invert") + "\n"
        docker_commands.write(cmd1)
        docker_commands.write(cmd2)

    docker_commands.close()

def write_docker_commands_for_community(com_regex1, com_permit1, com_regex2, com_permit2):
    docker_commands = open('new_docker_commands.sh', 'a')

    if as_path_regex1 != as_path_regex2:
        cmd1 = "docker exec -it gobgp_1 gobgp policy as-path del c1"
        cmd2 = "docker exec -it gobgp_1 gobgp policy as-path add c1 " + com_regex2

        docker_commands.write(cmd1 + "\n" + cmd2 + "\n")
    
    if com_permit1 != com_permit2:
        cmd1 = "docker exec -it gobgp_1 gobgp policy statement statement1 del condition community c1\n"
        cmd2 = "docker exec -it gobgp_1 gobgp policy statement statement1 add condition community c1 " + "any" if as_path_pd2=="True" else "invert" + "\n"
        docker_commands.write(cmd1)
        docker_commands.write(cmd2)

    docker_commands.close()

def write_docker_commands_for_route_map_permit(rpd1, rpd2):
    docker_commands = open('new_docker_commands.sh', 'a')

    docker_commands.write('docker exec -it gobgp_1 gobgp policy statement statement1 del action ' + ("reject" if rpd1=="False" else "accept") + "\n")
    docker_commands.write('docker exec -it gobgp_1 gobgp policy statement statement1 add action ' + ("reject" if rpd2=="False" else "accept") + "\n")

    docker_commands.close()

### MAIN ###

g = open("results/dynamic_report.txt", "w")
g.close()

g1 = open("results/dynamic_err.txt", "w")
g1.close()

tests = glob("../tests/*.json")

for test_file in tests:

    with open(test_file, 'r') as f:
        test = json.load(f)

    tid = int(test_file.split('/')[-1].split('.')[0])

    rmap1 = test["Rmap1"]
    rmap2 = test["Rmap2"]
    decision1 = test["Decision1"]
    decision2 = test["Decision2"]
    route = test["Route"]

    with open('test.json', 'w') as f:
        json.dump([rmap1, route, {"Allowed": ("True" if decision1 else "False")}], f)


    attrs = ["Prefix", "ASPath", "Community", "LP", "MED"]

    docker_commands = open('new_docker_commands.sh', 'w')
    docker_commands.close()


    subprocess.run(['bash', 'start_compare.sh'])

    for a in attrs:
        if rmap1[a] != "None":
            if a == "Prefix":
                prefix_list = []
                for i in range(3):
                    p0 = {
                        "prefix": rmap2["Prefix" + str(i)].split("/")[0],
                        "mask": int(rmap2["Prefix" + str(i)].split("/")[1]),
                        "le":  32 if rmap2["LE" + str(i)] == "None" else int(rmap2["LE" + str(i)]),
                        "ge":  int(rmap2["Prefix" + str(i)].split("/")[1]) if rmap2["GE" + str(i)] == "None" else int(rmap2["GE" + str(i)]),
                        "permit": True if rmap2["PrefixPD" + str(i)]=="True" else False 
                    }
                    if rmap2["LE" + str(i)] == "None" and rmap2["GE" + str(i)] == "None":
                        p0["le"] = p0["mask"]
                        p0["ge"] = p0["mask"]

                    prefix_list.append(p0)

                prefix_list = convert_prefix_list_to_prefix_set(prefix_list)
                write_docker_commands_for_prefix_set(prefix_list)
            elif  a == "ASPath":
                as_path_regex1 = rmap1["ASPath-regex"]
                as_path_pd1 = rmap1["ASPath-permit"]
                as_path_regex2 = rmap2["ASPath-regex"]
                as_path_pd2 = rmap2["ASPath-permit"]
                write_docker_commands_for_as_path(as_path_regex1, as_path_pd1, as_path_regex2, as_path_pd2)
            else:
                com_regex1 = rmap1["Community-regex"]
                com_pd1 = rmap1["Community-permit"]
                com_regex2 = rmap2["Community-regex"]
                com_pd2 = rmap2["Community-permit"]
                write_docker_commands_for_community(com_regex1, com_pd1, com_regex2, com_pd2)

    if rmap1["RmapPD"] != rmap2["RmapPD"]:
        rmap_pd1 = rmap1["RmapPD"]
        rmap_pd2 = rmap2["RmapPD"]

    write_docker_commands_for_route_map_permit(rmap_pd1, rmap_pd2)

    with open('new_docker_commands.sh', 'a') as f:
        f.write('docker exec -it gobgp_1 gobgp neighbor 3.0.0.3 softreset\n')
        f.write('sleep 10\n')
        f.write('docker exec -it gobgp_1 gobgp global rib > out.txt\n')
        f.write('docker-compose down\n')
        f.write('rm -rf gobgp1/gobgp.yml')

    gobgp_attr = parse.set_attributes()
    if gobgp_attr == -1:
        g = open("results/dynamic_err.txt", "a")
        g.write(str(tid) + "\n")
        g.close()
        continue
    router_decision1 = "False" if gobgp_attr=={} else "True"
    exp_decision1 = "True" if decision1 else "False"
    match_decision1 = "yes" if exp_decision1 == router_decision1 else "no"

    subprocess.run(['bash', 'new_docker_commands.sh'])

    gobgp_attr = parse.set_attributes()
    router_decision2 = "False" if gobgp_attr=={} else "True"
    exp_decision2 = "True" if decision2 else "False"
    match_decision2 = "yes" if exp_decision2 == router_decision2 else "no"

    with open("results/dynamic_report.txt", "a") as f:
        if match_decision1 == "no" or match_decision2 == "no":
            f.write(f"Test Case {tid} : {match_decision1},{match_decision2}\n")
            json.dump(test, f, indent=4)
            f.write('\n')
        else:
            f.write(f"Test Case {tid} : {match_decision1},{match_decision2}\n")
            f.write('\n')


