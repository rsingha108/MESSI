from pybatfish.client.session import Session
from pybatfish.datamodel import *
from pybatfish.datamodel.answer import *
from pybatfish.datamodel.flow import *
from pybatfish.util import get_html
import os
import json
import snapshot_generator

def run_aggregation():
    bf = Session(host="localhost")
    NETWORK_NAME = 'example'
    SNAPSHOT_NAME = 'test_snapshot'
    SNAPSHOT_PATH = 'test_snapshot'
    bf.set_network(NETWORK_NAME)
    bf.init_snapshot(SNAPSHOT_PATH, name=SNAPSHOT_NAME, overwrite=True)
    result = bf.q.bgpRib(nodes='RouterC').answer().frame()
    res = result.values
    # print(bf.q.initIssues().answer().frame())
    routes = []
    for row in res:
        routes.append(row[2])
    print(routes)
    return routes

tests_dir = "./tests"
g = open(f'results_batfish.txt','w')
g.close()
n_tests = len(os.listdir(tests_dir))
for i in range(n_tests):
    filename = f"{i}.json"
    print(f"Running test {filename}")
    file_path = os.path.join(tests_dir, filename)
    with open(file_path, "r") as file:
        test = json.load(file)
    
    snapshot_generator.snap_gen(test)
    routes = run_aggregation()
    
    ### PARSING ###
    actual_decision = False
    agg_str, agg_ip, agg_mask, agg_mask_length = snapshot_generator.extract_agg_route(test["Router"])
    for d in test["output"]:
        if d["Prefix"] == (agg_ip + "/" + str(agg_mask_length)):
            actual_decision = True
    
    router_decision = False
    for route in routes:
        agg_ip_mask = agg_ip + "/" + str(agg_mask_length)
        print ("agg_ip_mask = ", agg_ip_mask)
        agg_ip_mask_corrected = snapshot_generator.ip_prefix_correction(agg_ip_mask)
        print ("agg_ip_mask corrected = ", agg_ip_mask_corrected)
        if route == agg_ip_mask_corrected:
            router_decision = True
    
    with open(f'results_batfish.txt','a') as f:
        f.write(f"{actual_decision},{router_decision}\n")


