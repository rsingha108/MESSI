from pybatfish.client.session import Session
from pybatfish.datamodel import *
from pybatfish.datamodel.answer import *
from pybatfish.datamodel.flow import *
from pybatfish.util import get_html

bf = Session(host="localhost")

NETWORK_NAME = 'example'
SNAPSHOT_NAME = 'test_snapshot'

SNAPSHOT_PATH = 'test_snapshot'

bf.set_network(NETWORK_NAME)
bf.init_snapshot(SNAPSHOT_PATH, name=SNAPSHOT_NAME, overwrite=True)

result = bf.q.bgpRib(nodes='RouterD').answer().frame()

res = result.values

routes = []

for row in res:
    routes.append(row[2])
    
print(routes)
