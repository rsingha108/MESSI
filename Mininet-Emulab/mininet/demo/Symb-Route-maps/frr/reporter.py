import subprocess
import random

with open('results/buckets.txt') as f:
    lines = f.readlines()

for line in lines:
    line = line.strip()
    idxs = line.split(':')[1].strip()[1:-1].split(', ')
    idx = int(random.choice(idxs)[1:-1])
    cmd = ["python3", "one-router.py", f"--test_id={idx}"]
    print(' '.join(cmd))
    subprocess.run(cmd)


