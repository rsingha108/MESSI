def set_attributes():

	with open("out.txt","r") as f:
		lines = f.readlines()

	if lines == []:
		return -1
		
	bird_attr = {}
	for line in lines:
		if "BGP.med" in line: 
			bird_attr["MED"] = line.strip().split(" ")[-1]
		if "BGP.local_pref" in line:
			bird_attr["LP"] = line.strip().split(" ")[-1]
		if "BGP.community" in line:
			bird_attr["Community"] = line.strip().split(":")[-1].strip().replace(',',':').replace('(',"").replace(')',"")
			
		else:
			continue

	return bird_attr
