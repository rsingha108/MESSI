err = False
g = open('multi_run_result.txt','a')

with open('../routes.txt','r') as f:
    routes = f.readlines()
routes = routes[1:]

with open('../rmap.txt','r') as f:
    rmap = f.readlines()


with open('exp_frr.txt','r') as f:
    exps = f.readlines()

with open('results_frr.txt','r') as f:
    frrs = f.readlines()

with open('results_quagga.txt','r') as f:
    qggas = f.readlines()


for i in range(len(exps)):
    l = [exps[i],frrs[i],qggas[i]]
    if len(set(l)) != 1:
        err = True
        # print(f"ERROR! in Route {i+2} : exp={exps[i][:-1]}, frr={frrs[i][:-1]}, quagga={qggas[i][:-1]}\n")
        g.write(f"ERROR! in Route : {routes[i]} : exp={exps[i][:-1]}, frr={frrs[i][:-1]}, quagga={qggas[i][:-1]}\n")

if err:
    g.write('\n')
    for line in rmap:
        g.write(line)
    g.write("-"*20+'\n')
