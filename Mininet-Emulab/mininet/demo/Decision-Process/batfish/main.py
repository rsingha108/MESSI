from pybatfish.client.session import Session
from pybatfish.datamodel import *
from pybatfish.datamodel.answer import *
from pybatfish.datamodel.flow import *
from pybatfish.util import get_html
import os
import json
import snapshot_generator

def run_decision():
    bf = Session(host="localhost")
    NETWORK_NAME = 'example'
    SNAPSHOT_NAME = 'test_snapshot'
    SNAPSHOT_PATH = 'test_snapshot'
    bf.q.initIssues()
    bf.set_network(NETWORK_NAME)
    bf.init_snapshot(SNAPSHOT_PATH, name=SNAPSHOT_NAME, overwrite=True)
    # print(bf.q.initIssues().answer().frame())
    result = bf.q.bgpRib(nodes='RouterC').answer().frame()
    result.to_csv('result.csv', index=False)
    return result

tests_dir = "../tests"
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
    result = run_decision()
    # print(result)
    # print(result.at[0,"Originator_Id"])
    
    ### PARSING ###
    if test["Decision"]["NgbrAddr"] == test["Route1"]["NgbrAddr"] : actual_decision = 1
    else : actual_decision = 3
    
    router_decision = 1 if test["Route1"]["NgbrAddr"] == result.at[0,"Originator_Id"] else 3
    
    with open(f'results_batfish.txt','a') as f:
        f.write(f"{actual_decision},{router_decision}\n")

