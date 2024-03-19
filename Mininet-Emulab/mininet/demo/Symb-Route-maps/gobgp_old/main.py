import json
import time
import subprocess
import parse
from tqdm import tqdm

g = open("results/results.txt","w")
g.close()

g1 = open("results/error.txt", "w")
g1.close()

# sp_idx =  271 #405 - set com, 506 - del com,  #1 - AS path
# for i in tqdm(range(sp_idx,sp_idx+1)):

# for i in tqdm(range(264,286)):

i = -1
while True:
	i += 1

	patience = 1000 
	while True:
		if patience == 0:
			print("Timeout")
			break
		try:
			with open(f'../tests/{i}.json') as f:
				test = json.load(f)
			break
		except:
			time.sleep(5)
			patience -= 1
			continue

	if patience == 0:
		print("Timeout")
		break

	print(f"\n*************** Test Case : {i} ***************\n")
	
    ## overwrite the test.json file with the current test case

	rmap, route, decision = test[0], test[1], test[2]
	
	# if ((rmap["Prefix"] == "None") and (rmap["Community"] == "None") and (rmap["ASPath"] == "None")):
	#	if ((rmap["LP"] != "None") or (rmap["MED"] != "None")):
	#		continue

	with open("test.json", "w") as f:
		json.dump(test, f)
	

	subprocess.run(["bash", "start.sh"])

	gobgp_attr = parse.set_attributes()
	
	if gobgp_attr == -1:
		g = open("results/error.txt", "a")
		g.write(str(i) + "\n")
		g.close()
		continue
		

	# print(gobgp_attr)

	exp_decision = decision['Allowed']
	router_decision = "False" if gobgp_attr == {} else "True"
	match_decision = "yes" if exp_decision == router_decision else "no"
	# print(gobgp_attr)
	
	match_attr = "yes" ## if all attributes matching with expected decision
	if (rmap["Set"] != "None") and (decision["Allowed"] == "True") and router_decision=="True":
		if gobgp_attr["MED"]!=decision["MED"]:
			match_attr = "no"
		elif sorted(decision["Community"].split(' '))!=sorted(gobgp_attr["Community"].split(' ')):
			match_attr = "no"
		elif gobgp_attr["LP"] != "" and gobgp_attr["LP"] !=decision["LP"]:
			match_attr = "no"
		elif gobgp_attr["NH"]!=decision["NextHop"]:
			match_attr = "no"
		elif gobgp_attr["ASPath"] != decision["ASPath"]:
			match_attr = "no"

	g = open("results/results.txt","a")
	g.write(f"{i},{match_decision},{match_attr}\n")
	g.close()

	


