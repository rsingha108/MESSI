with open('correct_quagga.txt','r') as f:
	lines = f.readlines()		
correct_outputs = []
for line in lines:
	correct_outputs.append(line[:-1])
		

			
with open('results_quagga.txt','r') as f:
	lines = f.readlines()
model_outputs = []
for line in lines:
	model_outputs.append(line[:-1])



with open('zen_out.txt','r') as f:
	lines = f.readlines()
lines = lines[5:-1]	
tests = []
test = []
for line in lines:
	if line == '\n' : 
		tests.append(';'.join(test))
		test = []
	else:
		if line.startswith('(') : 
			line = line[1:-1]
			test.append(line)
		if line.startswith(',') : 
			line = line[2:-1]
			test.append(line)
			
	
print("\nFor target router : IP1, IP2, ASNum")

print("For route advertisements : AS Number, Local Preference, AS_Path Length, Origin, MED, IGP Metric, Send/Arrival Time, Router ID, Neighbor Address\n")
		
for i in range(len(correct_outputs)):
	if correct_outputs[i] != model_outputs[i] :
		tst = tests[i].split(';')
		print(f"\nAnomalous Test Case #{i+1} :")
		for l in tst:
			print(l)
		print("\n")
