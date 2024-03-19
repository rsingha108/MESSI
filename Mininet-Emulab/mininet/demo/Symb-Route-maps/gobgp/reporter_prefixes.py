import json
import random
import subprocess
import parse

error_classes = {'P1-no-yes': 17550, 'P1H5-no-yes': 25705, 'P1A4-no-yes': 41402, 'P1C4-yes-no': 10071, 'P1L3M3-no-yes': 16578, 'P1C4A3-yes-no': 15685, 'P1C3H3-no-yes': 8231, 'P1C3A3-no-yes': 51469, 'P1C3A4-no-yes': 9010, 'P1C3-no-yes': 4801, 'P1C4-no-yes': 53746, 'P1C3A4-yes-no': 10181, 'P1H3-no-yes': 27117, 'P1A3-no-yes': 14403, 'P1C4A4-yes-no': 27485, 'P1C3-yes-no': 23130, 'P1C3H5-no-yes': 11702, 'P1L3M3C3-no-yes': 4705, 'P1A4H3-no-yes': 50401, 'P1L3M3A3H5-no-yes': 16907, 'P1C3H5-yes-no': 1664, 'P1C4A3-no-yes': 13851, 'P1L3M3C3A3-yes-no': 1669, 'P1L3M3A4-yes-no': 47560, 'P1C3H3-yes-no': 33812, 'P1C3A3H5-yes-no': 2184, 'P1C4H5-yes-no': 16920, 'P1L3M3C4-yes-no': 34660, 'P1A3H3-no-yes': 24478, 'P1A4H5-no-yes': 40982, 'P1C3A4H3-no-yes': 6241, 'P1L3M3H3-no-yes': 4545, 'P1C4H5-no-yes': 16726, 'P1L3M3H5-no-yes': 34325, 'P1C4A4-no-yes': 41871, 'P1L3M3C3A4-no-yes': 28237, 'P1C4H3-yes-no': 25591, 'P1L3M3A4-no-yes': 31938, 'P1L3M3C3H5-no-yes': 26289, 'P1L3M3C4-no-yes': 52009, 'P1L3M3A3-no-yes': 33446, 'P1L3M3C4A4-yes-no': 38392, 'P1L3M3A3H3-no-yes': 46126, 'P1C3A3H5-no-yes': 16822, 'P1L3M3A4H5-yes-no': 52498, 'P1L3M3-yes-no': 4714, 'P1C3A4H3-yes-no': 43982, 'P1C4A4H3-no-yes': 46375, 'P1L3M3A4H3-no-yes': 22410, 'P1C4H3-no-yes': 29026, 'P1C4A3H5-yes-no': 45963, 'P1A3H5-no-yes': 10629, 'P1C3A4H5-no-yes': 34498, 'P1L3M3C4H5-no-yes': 30712, 'P1L3M3C4A3-no-yes': 49515, 'P1L3M3C3A3-no-yes': 7700, 'P1L3M3C4A4-no-yes': 42672, 'P1L3M3A3H3-yes-no': 34549, 'P1C4A4H3-yes-no': 33985, 'P1L3M3C3A4H5-yes-no': 34048, 'P1C3A4H5-yes-no': 46461, 'P1L3M3C4A3-yes-no': 51686, 'P1L3M3C4H5-yes-no': 20084, 'P1C3A3H3-yes-no': 14023, 'P1L3M3C3A4-yes-no': 17900, 'P1L3M3C3H5-yes-no': 38383, 'P1L3M3C3-yes-no': 24990, 'P1C4A4H5-yes-no': 17695, 'P1C3A3H3-no-yes': 8885, 'P1L3M3C3A4H3-no-yes': 42877, 'P1L3M3C3H3-no-yes': 22056, 'P1L3M3C3A3H5-no-yes': 39314, 'P1C4A4H5-no-yes': 15123, 'P1L3M3C3A4H3-yes-no': 50149, 'P1L3M3C4H3-yes-no': 54027, 'P1C4A3H3-no-yes': 26451, 'P1L3M3C3A4H5-no-yes': 41552, 'P1C4A3H3-yes-no': 22984, 'P1L3M3C4H3-no-yes': 43409, 'P1L3M3A4H5-no-yes': 20357, 'P1L3M3C3H3-yes-no': 49192, 'P1L3M3A4H3-yes-no': 26322, 'P1L3M3C4A3H3-no-yes': 51746, 'P1C4A3H5-no-yes': 26778, 'P1L3M3C3A3H5-yes-no': 26964, 'P1L3M3C4A4H3-yes-no': 42478, 'P1L3M3H3-yes-no': 37192, 'P1L3M3H5-yes-no': 37987, 'P1L3M3C3A3H3-no-yes': 50795, 'P1C3A3-yes-no': 51448, 'P1L3M3C4A3H5-yes-no': 51268, 'P1L3M3C4A4H5-yes-no': 41916, 'P1L3M3C3A3H3-yes-no': 8603, 'P1L3M3C4A3H3-yes-no': 27176, 'P1L3M3A3H5-yes-no': 14648, 'P1L3M3C4A4H5-no-yes': 45838, 'P1L3M3A3-yes-no': 22587, 'P1L3M3C4A4H3-no-yes': 21695, 'P1L3M3C4A3H5-no-yes': 42997}

# error_classes = {'P1C3H3-no-yes': 8231}

# with open("results/buckets.txt", "r") as f:
#     lines = f.readlines()
#     errors = {}
#     for l in lines:
#         l = l[:-1]
#         l = l.split(": ")
#         errors = l[1].strip('][').split(', ')
#         errors = [int(e[1:-1]) for e in errors]
#         error_classes[l[0]] = errors

# error_classes['some-yes-no'] = [186, 200, 245]
error_output_file = open("results/report_gobgp.txt", "w")
error_output_file.close()

for err_cls in error_classes.keys():
    test_case = error_classes[err_cls]
    with open('../tests2/' + str(test_case) + '.json', 'r') as f:
        test_attrs = json.load(f)
    with open('test.json', 'w') as f:
        json.dump(test_attrs, f, indent=4)
    
    subprocess.run(["bash", "start.sh"])

    gobgp_attr = parse.set_attributes()

    print(f"gobgp_attr: {gobgp_attr}")

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
        if gobgp_attr["NH"] != decision["NextHop"]:
            match_attr = "no"
            unmatched_attr.append("NH="+str(gobgp_attr["NH"]))
        if gobgp_attr["ASPath"] != decision["ASPath"]:
            match_attr = "no"
            unmatched_attr.append("ASPath="+str(gobgp_attr["ASPath"]))

    if (match_decision == "no" or match_attr == "no"):
        error_output_file = open("results/report_gobgp.txt", "a")
        error_output_file.write("Test case: " + str(test_case) + ", " + err_cls.split('-')[1] + "-" + err_cls.split('-')[2] + '\n')
        json.dump(test_attrs, error_output_file, indent=4)
        error_output_file.write("\n\nUnmatched route attributes:\n")
        error_output_file.write(match_decision + " "  + str(unmatched_attr) + "\n")
        error_output_file.write("#######################################################################################")
        error_output_file.write("\n\n\n")
        error_output_file.close()
    else:
        error_output_file = open("results/report_gobgp.txt", "a")
        error_output_file.write(f"Test case {str(test_case)} : no error!")
        error_output_file.write("#######################################################################################")
        error_output_file.write("\n\n\n")
        error_output_file.close()
