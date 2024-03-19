
failed_tests = []
unique_tags = []

#### FRR ####

with open('../../frr/results/report_tests2.txt', 'r') as f:
    lines = f.readlines()
for line in lines:
    if "Tag" in line:
        tag = line.split(":")[1].strip()[1:-2]
        if tag not in unique_tags:
            unique_tags.append(tag)
    if "Test" in line:
        test_id = line.split(":")[0].strip().split()[2]
        if test_id not in failed_tests:
            failed_tests.append(test_id)

#### BATFISH ####

with open('../../batfish/results/report_tests2.txt', 'r') as f:
    lines = f.readlines()
for line in lines:
    if "Tag" in line:
        tag = line.split(":")[1].strip()[1:-2]
        if tag not in unique_tags:
            unique_tags.append(tag)
    if "Test" in line:
        test_id = line.split(":")[0].strip().split()[2]
        if test_id not in failed_tests:
            failed_tests.append(test_id)

#### GOBGP ####

with open('../../gobgp/results/report_gobgp.txt', 'r') as f:
    lines = f.readlines()
for line in lines:
    if "Tag" in line:
        tag = line.split(":")[1].strip()[1:-2]
        if tag not in unique_tags:
            unique_tags.append(tag)
    if ("Test" in line) and ("no error" not in line):
        print(line)
        test_id = line.split(":")[1].strip().split(',')[0]
        if test_id not in failed_tests:
            failed_tests.append(test_id)

print("Number of failed tests: ", len(failed_tests))
print("Number of unique tags: ", len(unique_tags))
