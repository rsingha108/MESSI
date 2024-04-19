import numpy as np

def convert_mask_length(mask_length):
    octets = [0, 0, 0, 0]
    for i in range(mask_length):
        octets[i // 8] |= 1 << (7 - i % 8)
    return ".".join(map(str, octets))

def convert_ip_subnet(ip_subnet):
    ip, subnet = ip_subnet.split("/")
    ip_octets = ip.split(".")
    last_octet = int(ip_octets[-1])
    last_octet += 1
    ip_octets[-1] = str(last_octet)
    ip1 = ".".join(ip_octets)
    mask_length = int(subnet)
    mask = convert_mask_length(mask_length)
    return ip, ip1, mask, mask_length


def ip_prefix_correction(ip_prefix):
    # Split the IP address and prefix length
    ip, prefix_length = ip_prefix.split('/')
    ip_parts = ip.split('.')

    # Convert IP address parts to integers
    ip_int = 0
    for part in ip_parts:
        ip_int = (ip_int << 8) + int(part)

    # Calculate the corrected IP address
    corrected_ip_int = ip_int & ((2 ** 32 - 1) << (32 - int(prefix_length)))
    corrected_ip_parts = []
    for _ in range(4):
        corrected_ip_parts.append(str(corrected_ip_int >> 24))
        corrected_ip_int = (corrected_ip_int << 8)%(2 ** 32 - 1) 

    corrected_ip = '.'.join(corrected_ip_parts)
    corrected_prefix = f"{corrected_ip}/{prefix_length}"

    return corrected_prefix

"""
hostname routerA

interface eth0/0
    ip address 1.0.0.2 255.255.255.0
interface eth0/1
    ip address 100.0.0.1 255.255.255.0

route-map POLICY permit 10
    set local-preference 200
    set metric 50
    set origin igp | egp 65002
    set as-path prepend 300 400
    set ip next-hop 192.0.0.1

router bgp 65001
    router-id 10.0.0.1
    neighbor 1.0.0.1 remote-as 65000
    neighbor 100.0.0.2 remote-as 65002
    network 100.0.0.0/24
    neighbor 1.0.0.1 route-map POLICY outx
"""
    
    
def snap_gen(test):
    dir_path  = "test_snapshot/configs/"

    with open(dir_path + "A.cfg", "w") as f:
        ip, ip1, mask, mask_length = convert_ip_subnet("100.10.0.0/24")
        # print(test)
        f.write("hostname RouterA\n\n")
        f.write("interface eth0/0\n")
        f.write("   ip address 5.0.0.1 255.255.255.0\n")
        f.write("interface eth0/1\n")
        f.write(f"  ip address {ip1} {mask}\n\n")
        f.write(f"route-map RMAP permit 10\n")
        f.write(f"  set local-preference {test['Route1']['LP']}\n")
        f.write(f"  set metric {test['Route1']['MED']}\n")
        l = list(np.arange(1000, 1000+test['Route1']['ASPathLength'], 1))
        asp = " ".join([str(x) for x in l])
        f.write(f"  set as-path prepend {asp}\n")
        omap = {'i':'igp','e':'egp 8765','?':'incomplete'}
        f.write(f"  set origin {omap[test['Route1']['Origin']]}\n")
        f.write(f"    \n")
        f.write(f"router bgp {test['Route1']['ASN']}\n")
        f.write(f"   router-id {test['Route1']['RID']}\n")
        f.write(f"   neighbor 5.0.0.2 remote-as {test['RouterAS']}\n")
        f.write(f"   neighbor {ip} remote-as 12\n")
        f.write(f"   network {ip}/{mask_length}\n")
        f.write(f"   neighbor 5.0.0.2 route-map RMAP out\n")

    with open(dir_path + "B.cfg", "w") as f:
        ip, ip1, mask, mask_length = convert_ip_subnet("100.10.0.0/24")
        f.write("hostname RouterB\n\n")
        f.write("interface eth0/0\n")
        f.write("   ip address 6.0.0.1 255.255.255.0\n")
        f.write("interface eth0/1\n")
        f.write(f"  ip address {ip1} {mask}\n\n")
        f.write(f"route-map RMAP permit 10\n")
        f.write(f"  set local-preference {test['Route2']['LP']}\n")
        f.write(f"  set metric {test['Route2']['MED']}\n")
        l = list(np.arange(2000, 2000+test['Route2']['ASPathLength'], 1))
        asp = " ".join([str(x) for x in l])
        f.write(f"  set as-path prepend {asp}\n")
        omap = {'i':'igp','e':'egp 8766','?':'incomplete'}
        f.write(f"  set origin {omap[test['Route2']['Origin']]}\n")
        f.write(f"    \n")
        f.write(f"router bgp {test['Route2']['ASN']}\n")
        f.write(f"   neighbor 6.0.0.2 remote-as {test['RouterAS']}\n")
        f.write(f"   neighbor {ip} remote-as 22\n")
        f.write(f"   network {ip}/{mask_length}\n")
        f.write(f"   neighbor 6.0.0.2 route-map RMAP out\n")

    with open(dir_path + "C.cfg", "w") as f:
        f.write("hostname RouterC\n\n")
        f.write("interface eth0/0\n")
        f.write("    ip address 5.0.0.2 255.255.255.0\n")
        f.write("interface eth0/1\n")
        f.write("    ip address 6.0.0.2 255.255.255.0\n")
        f.write(f"router bgp {test['RouterAS']}\n")
        f.write(f"    neighbor 5.0.0.1 remote-as {test['Route1']['ASN']}\n")
        f.write(f"    neighbor 6.0.0.1 remote-as {test['Route2']['ASN']}\n")



