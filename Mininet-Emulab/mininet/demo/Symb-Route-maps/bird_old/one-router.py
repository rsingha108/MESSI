import json
import time
import configs
import subprocess
import parse
from tqdm import tqdm

g = open("results/results.txt","w")
g.close()
g1 = open("results/err.txt","w")
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
	rmap, route, decision = test[0], test[1], test[2]
    
	configs.generate(rmap, route)

	subprocess.run(["bash", "run.sh"])

	bird_attr = parse.set_attributes()

	# print(bird_attr)

	if bird_attr == -1:
		g1 = open("results/err.txt","a")
		g1.write(f"{i},bird error!!!\n")
		g1.close()
		continue



	exp_decision = decision['Allowed']
	router_decision = "False" if bird_attr == {} else "True"
	match_decision = "yes" if exp_decision == router_decision else "no"

	match_attr = "yes" ## if all attributes matching with expected decision
	if (rmap["Set"] != "None") and (decision["Allowed"] == "True") and router_decision == "True":
		if (bird_attr["LP"]!=decision["LP"] or bird_attr["MED"]!=decision["MED"] or sorted(decision["Community"].split(' '))!=sorted(bird_attr["Community"].split(' '))):
			match_attr = "no"


	g = open("results/results.txt","a")
	g.write(f"{i},{match_decision},{match_attr}\n")
	g.close()

	



