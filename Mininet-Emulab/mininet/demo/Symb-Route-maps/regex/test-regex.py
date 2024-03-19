import subprocess
from tqdm import tqdm

pr = open('posix-results.txt','w')
pr.close()

with open('regex-ex.txt','r') as f:
    lines = f.readlines()

for line in tqdm(lines):
    line = line.strip()
    sp = line.split(',')
    rgx, com, res = sp[0], sp[1], sp[2]
    # result = subprocess.run(['./posixrgx', '"^[2-3]+$"', '"2233"'], stdout=subprocess.PIPE)
    result = subprocess.run(['./posixrgx', f'{rgx}', f'{com}'], stdout=subprocess.PIPE)
    if result.stdout.decode().strip() != res:
        with open('posix-results.txt','a') as g:
            g.write(f'Error: {rgx},{com},{res},{result.stdout.decode()}\n')
            
    
    