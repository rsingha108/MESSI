import argparse
import json
import subprocess

## helper functions

def find_tag_by_index(tests, idx):
    with open(f"../../{tests}/{idx}.json", 'r') as f:
        test = json.load(f)
    decision = test[2]
    tag = decision['Tag']
    return tag

## take software and tests as argument
parser = argparse.ArgumentParser()
parser.add_argument('--software', type=str, default='frr', help='software results to be bucketed')
parser.add_argument('--tests', type=str, default='tests2', help='test suite to be bucketed')
args = parser.parse_args()
software = args.software
tests = args.tests

## read results and bucket them
buckets = {}
with open(f"../../{software}/results/results.txt", 'r') as f:
    lines = f.readlines()
    for line in lines:
        sp = line.strip().split(',')
        idx,match,attr = sp[0],sp[1],sp[2]
        if match == 'yes' and attr == 'yes':
            continue
        tag = find_tag_by_index(tests, idx)
        tag = f"{tag}-{match}-{attr}"
        if tag not in buckets:
            buckets[tag] = []
        buckets[tag].append(idx)

## write buckets to file
with open(f"../../{software}/results/buckets.txt", 'w') as f:
    for tag, idxs in buckets.items():
        f.write(f"{tag}: {idxs}\n")

        
    

