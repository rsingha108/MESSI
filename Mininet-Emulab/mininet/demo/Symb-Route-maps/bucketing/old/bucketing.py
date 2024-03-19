import json
import os
import pandas as pd
import random

"""
{
    "Prefix": "Some",
    "Prefix0": "100.10.0.0/16",
    "LE0": "None",
    "GE0": "None",
    "PrefixPD0": "True",
    "Prefix1": "99.192.2.64/31",
    "LE1": "None",
    "GE1": "None",
    "PrefixPD1": "False",
    "Prefix2": "100.8.64.4/27",
    "LE2": "None",
    "GE2": "None",
    "PrefixPD2": "False",
    "RmapPD": "True",
    "LP": "None",
    "MED": "4",
    "Community": "None",
    "Community-regex": "None",
    "Community-permit": "None",
    "ASPath": "None",
    "ASPath-regex": "None",
    "ASPath-permit": "None",
    "Set": "None",
    "SetLP": "None",
    "SetMED": "None",
    "SetCommunity": "None",
    "DeleteCommunity": "None"
  }

"""

## function for labeling tests
def label_test(rmap):
    label = ""
    if rmap["Prefix"] == "Some": label += "P"
    if rmap["LP"] != "None": label += "L"
    if rmap["MED"] != "None": label += "M"
    if rmap["Community"] == "Some": label += "C"  
    if rmap["ASPath"] == "Some": label += "A"
    if rmap["Set"] == "Some": label += "_"
    if rmap["SetLP"] != "None": label += "L"
    if rmap["SetMED"] != "None": label += "M"
    if rmap["SetCommunity"] != "None": label += "C"
    if rmap["DeleteCommunity"] != "None": label += "D"
    return label

def label_output(row, impls):
    output = ""
    for i in impls:
        output += ''.join([s[0] for s in row[i].split(',')])
    return output

## make dataframe with results from each implementation
def add_df(d, impl, impls): ## d = {"idx" : [], "frr" : [], "quagga" : [], "bird" : [], "batfish" : [], gobgp : []}
    path = f"../{impl}/results/results.txt"
    if impl == 'frr' or impl == 'quagga':
        path = f"../{impl}/results/results_{impl}.txt"

    with open(path, 'r') as f:
        count = 2500
        while True:
        # while count >= 0:
            # count -= 1
            line = f.readline()
            if not line:
                break
            sp = line.strip().split(',')
            idx,match,attr = sp[0],sp[1],sp[2]
            if match == 'no' or attr == 'no':
                if int(idx) not in d["idx"]:
                    d["idx"].append(int(idx))
                    d[impl].append(f"{match},{attr}")
                    for i in impls:
                        if i != impl:
                            d[i].append("yes,yes")
                else:
                    idx_idx = d["idx"].index(int(idx))
                    d[impl][idx_idx] = f"{match},{attr}"
    return d

def make_cluster(df, tag):
    ## sort by tag
    df = df.sort_values(by=[tag])
    ## group idxs by tag
    clusters = df.groupby(tag)['idx'].apply(list).reset_index(name='idxs')
    ## convert to dict with tag as key and idxs as value
    clusters = dict(zip(eval(f"clusters.{tag}"), clusters.idxs))
    ## convert clusters to dataframe such that each row is a cluster
    clusters_df = pd.DataFrame(clusters.items(), columns=[f'{tag}', 'idxs'])
    clusters_df.to_csv(f"{tag}_clusters.csv", index=False)
    ## randomly selct one idx from each cluster and make dataframe with results from each implementation
    df1 = pd.DataFrame()
    for t, idxs in clusters.items():
        idx = random.choice(idxs)
        ## add row to df1
        df1 = df1.append(df[df['idx'] == idx])
    ## convert to csv
    df1.to_csv(f"{tag}_results.csv", index=False)


#################### Main ####################

impls = ["frr", "quagga", "bird", "batfish", "gobgp"]
d = {"idx" : []}
for impl in impls:
    d[impl] = []
for impl in impls:
    d = add_df(d, impl, impls)

df = pd.DataFrame(d)
## sort by idx
df = df.sort_values(by=['idx'])
## add label column
df["label"] = df.apply(lambda row: label_test(json.load(open(f"../tests/{row['idx']}.json"))[0]), axis=1)
df["output"] = df.apply(lambda row: label_output(row, impls), axis=1)
df.to_csv("results.csv", index=False)

########### Cluster by Label ############
make_cluster(df, "label")
make_cluster(df, "output")

    






