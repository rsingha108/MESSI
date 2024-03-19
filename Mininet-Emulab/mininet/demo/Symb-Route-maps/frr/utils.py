########## UTILS ##########

def RewriteIP(ip_addr):
    ip_split = ip_addr.split('/')
    msk = int(ip_split[1])
    p1, p2, p3, p4 = ip_split[0].split('.')
    p1, p2, p3, p4 = int(p1), int(p2), int(p3), int(p4)
    n = p1<<24 | p2<<16 | p3<<8 | p4
    n = n >> (32-msk)
    n = n << (32-msk)
    p1, p2, p3, p4 = n>>24, (n>>16)&0xff, (n>>8)&0xff, n&0xff
    ip_addr = f"{p1}.{p2}.{p3}.{p4}/{msk}"
    return ip_addr

def SortCommunity(community):
    community = community.split(' ')
    community = [(int(c.split(":")[0]), int(c.split(":")[1])) for c in community]
    community.sort()
    community = [str(c[0])+":"+str(c[1]) for c in community]
    community = ' '.join(community)
    return community

def TransformCommunity(com):
    spl_com = {"0:0" : "internet", "65535:65281" : "no-export", "65535:65282" : "no-advertise", "65535:65283" : "local-AS", "65535:65284" : "no-peer", "65535:6" : "llgr-stale"}
    for key in spl_com.keys():
        com = com.replace(key, spl_com[key])
    return com

def SortNTransformCom(com):
    com = SortCommunity(com)
    com = TransformCommunity(com)
    return com
