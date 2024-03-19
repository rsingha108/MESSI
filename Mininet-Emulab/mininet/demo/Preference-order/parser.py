import json
import argparse

parser = argparse.ArgumentParser()
parser.add_argument("--software", type=str, default="quagga")
args = parser.parse_args()

def has_numbers(inputString):
	return any(char.isdigit() for char in inputString)


def is_ip(inputString):
	return True if '.' in inputString else False
	

f = open("out.txt","r")
lines = f.readlines()[:-2]

i = 0
for line in lines:
	i += 1
	if line == '\n':
		break

lines = lines[i+1:]

lines2 = []

for line in lines:
	lines2.append(line.split())
	
#desired_output = {'1.0.0.0': '0.0.0.0', '4.0.0.0': '0.0.0.0', '100.10.0.0/24': '1.1.1.1', '200.20.0.0': '2.2.2.2'}

d = {}
prev = None
print(lines2)
for line in lines2:
	if '>' in line[0]:
		if has_numbers(line[0]):
			c = 0
			for char in line[0]:
				if ord(char) >= ord('0') and ord(char)<=ord('9'):
					break
				else: c+=1
			l = line[0][c:]
			prev = l
			d[prev] = line[1]
		elif not is_ip(line[2]):
			d[prev] = line[1]
		else:
			if 'i' in line[1]:
				d[line[1][1:]] = line[2]
				prev = line[1][1:]
			else:
				d[line[1]] = line[2]
				prev = line[1]
	elif is_ip(line[2]):
		if 'i' in line[1]:
			prev = line[1][1:]
		else: prev = line[1]
			 

with open(f'results_{args.software}.txt','a') as g:
	if "100.10.0.0/24" in d:
		g.write(d["100.10.0.0/24"]+'\n')
	else:
		g.write('-\n')

			 
#print(d)	
#with open('results.json','a') as g:
#	 json.dump(d, g, indent = 4)
#if d == desired_output:
#	print("All matched")
