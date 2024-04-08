with open("bird_output.txt","r") as f:
	lines = f.readlines()
	
op = "Null"
ignore = 1
for line in lines:
	if not line.startswith("100.10.0.0/24") and ignore==1 : continue
	else: ignore = 0
	if '*' not in line : continue
	op = line.split('via ')[1].split(' on')[0]
	break
	
with open('bird_result.txt','a') as g:
	g.write(op+'\n')
