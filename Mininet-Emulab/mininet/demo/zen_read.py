with open('zen_out.txt','r') as f:
	lines = f.readlines()
		
lines = lines[3:-1]	
tests = []
test = ""
for line in lines:
	if line == '\n' : 
		tests.append(test)
		test = ""
	else:
		if line.startswith('(') : 
			line = line[1:-1]
			test += line + ';'
		if line.startswith(',') : 
			line = line[2:-1]
			test += line

print(tests,len(tests))

for line in tests:

	line = line[:-1]
	sp = line.split(';')
	lp1,asn1,med1 = sp[0].split(',')
	lp2,asn2,med2 = sp[1].split(',')
	asn1,asn2 = int(asn1), int(asn2)
	asp1 = 	'[' + ' '.join(['100']*asn1) + ']'
	asp2 =  '[' + ' '.join(['100']*asn2) + ']'
	print(asp1,asp2)
