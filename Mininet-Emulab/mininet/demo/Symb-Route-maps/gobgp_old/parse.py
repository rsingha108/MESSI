def set_attributes():

	with open("out.txt","r") as f:
		lines = f.readlines()

	if (len(lines) == 1) and lines[0].startswith("Network not in table"):
		return {}
	elif len(lines) == 1 and lines[0].startswith("Error: gobgp.yml file not generated") or len(lines) == 0:
		return -1
		
	gobgp_attr = {}
	
	sp = lines[1].split("[{")[1].split("}]")[0].split("} {")
	
	# print(lines[1].split())
	
	# print(lines[1].split("\t"))

	med = ""
	comm = ""
	lp = ""
	for att in sp:
		if att.startswith("Med"):
			med = att[5:]
			# print(med)
		if att.startswith("Communities"):
			comm = att[13:].replace(",","")
			# print(comm)
		if att.startswith("LocalPref"):
			lp = att[11:].replace(",","")
	
	asp = []
	for x in lines[1].split()[3:]:
		if ':' not in x:
			asp.append(x)
		else:
			break	
	asp_str = " ".join(asp)
	

	nh = lines[1].split()[2]
	
	gobgp_attr["MED"] = med
	gobgp_attr["Community"] = comm
	gobgp_attr["ASPath"] = asp_str
	gobgp_attr["LP"] = lp
	gobgp_attr["NH"] = nh

	return gobgp_attr
