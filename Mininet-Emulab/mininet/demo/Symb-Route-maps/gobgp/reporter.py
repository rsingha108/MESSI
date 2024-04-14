import json
import random
import subprocess
import parse

error_classes = dict()

with open("results/buckets.txt", "r") as f:
    lines = f.readlines()
    errors = {}
    for l in lines:
        l = l[:-1]
        l = l.split(": ")
        errors = l[1].strip('][').split(', ')
        errors = [int(e[1:-1]) for e in errors]
        error_classes[l[0]] = errors

error_output_file = open("results/report_gobgp.txt", "w")

random.seed(20)

for err_cls in error_classes.keys():
    length = len(error_classes[err_cls])
    test_case = error_classes[err_cls][random.randint(0, length-1)]
    with open('../tests2/' + str(test_case) + '.json', 'r') as f:
        test_attrs = json.load(f)
    with open('test.json', 'w') as f:
        json.dump(test_attrs, f)
    
    subprocess.run(["bash", "start.sh"])

    gobgp_attr = parse.set_attributes()
    error_output_file.write("Test case: " + str(test_case) + ", " + err_cls.split('-')[1] + "-" + err_cls.split('-')[2] + '\n')
    json.dump(test_attrs, error_output_file, indent=4)
    error_output_file.write("\n\nUnmatched route attributes:\n")

    rmap, route, decision = test_attrs[0], test_attrs[1], test_attrs[2]

    exp_decision = decision['Allowed']
    router_decision = "False" if gobgp_attr == {} else "True"
    match_decision = "yes" if exp_decision == router_decision else "no"

    unmatched_attr = []
    match_attr = "yes" ## if all attributes matching with expected decision
    if (rmap["Set"] != "None") and (decision["Allowed"] == "True") and router_decision=="True":
        if gobgp_attr["MED"] != decision["MED"]:
            match_attr = "no"
            unmatched_attr.append("MED=" + str(gobgp_attr["MED"]))
        if sorted(decision["Community"].split(' '))!=sorted(gobgp_attr["Community"].split(' ')):
            match_attr = "no"
            unmatched_attr.append("Community="+str(sorted(gobgp_attr["Community"].split(' '))))
        if gobgp_attr["LP"] != "" and gobgp_attr["LP"] != decision["LP"]:
            match_attr = "no"
            unmatched_attr.append("LP="+str(gobgp_attr["LP"]))
        if gobgp_attr["NH"] != decision["NH"]:
            match_attr = "no"
            unmatched_attr.append("NH="+str(gobgp_attr["NH"]))
        if gobgp_attr["ASPath"] != decision["ASPath"]:
            match_attr = "no"
            unmatched_attr.append("ASPath="+str(gobgp_attr["ASPath"]))

    error_output_file.write(match_decision + " "  + str(unmatched_attr) + "\n")
    error_output_file.write("#######################################################################################")
    error_output_file.write("\n\n\n")
