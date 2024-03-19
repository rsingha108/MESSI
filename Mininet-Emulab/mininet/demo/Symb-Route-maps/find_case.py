import json

for i in range(1086):
  with open(f'tests/{i}.json','r') as f:
    obj = json.load(f)
  rmap, route, decision = obj[0], obj[1], obj[2]
  #if rmap["Community"] == "Some": # and rmap["RmapPD"] == "True":
  if rmap["Set"] == "Some" and rmap["RmapPD"] == "True" and decision["Allowed"] == "True":
    print(f"{i}")    
