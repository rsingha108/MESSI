
import os
import json
import glob
from tqdm import tqdm

# Set the directory you want to start from
directory_path = '../../tests/'

files = glob.glob(directory_path + '/*')
for file in tqdm(files):
    with open(file, 'r') as f:
        test = json.load(f)
    if (test["Rmap1"]["Prefix"] != "None"):
        print(test)
        break

