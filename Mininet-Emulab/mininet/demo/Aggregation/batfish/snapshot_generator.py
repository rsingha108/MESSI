

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


def extract_agg_route(router):
    agg_str = "aggregate-address"
    prefix = router["AggregateRoute"].split(",")[0].split(":")[1].strip()
    ip, ip1, mask, mask_length = convert_ip_subnet(prefix)
    summary_only = bool(router["SummaryOnly"])
    matching_med_only = bool(router["MatchingMEDOnly"])
    agg_str += f" {ip} {mask}"
    if summary_only:
        agg_str += " summary-only"
    return agg_str, ip, mask, mask_length

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

    
    
def snap_gen(test):
    dir_path  = "test_snapshot/configs/"

    with open(dir_path + "A.cfg", "w") as f:
        ip, ip1, mask, mask_length = convert_ip_subnet(test['Route1']['Prefix'])
        f.write("hostname RouterA\n\n")
        f.write("interface eth0/0\n")
        f.write("    ip address 1.0.0.1 255.255.255.0\n")
        f.write("interface eth0/1\n")
        f.write(f"    ip address {ip1} {mask}\n\n")
        f.write("router bgp 10\n")
        f.write("    neighbor 1.0.0.2 remote-as 11\n")
        f.write(f"    neighbor {ip} remote-as 12\n")
        f.write(f"    network {ip}/{mask_length}\n")

    with open(dir_path + "B.cfg", "w") as f:
        ip, ip1, mask, mask_length = convert_ip_subnet(test['Route2']['Prefix'])
        f.write("hostname RouterB\n\n")
        f.write("interface eth0/0\n")
        f.write("    ip address 2.0.0.1 255.255.255.0\n")
        f.write("interface eth0/1\n")
        f.write(f"    ip address {ip1} {mask}\n\n")
        f.write("router bgp 20\n")
        f.write("    neighbor 2.0.0.2 remote-as 11\n")
        f.write(f"    neighbor {ip} remote-as 22\n")
        f.write(f"    network {ip}/{mask_length}\n")

    with open(dir_path + "C.cfg", "w") as f:
        agg_str, ip, mask, mask_length = extract_agg_route(test['Router'])
        f.write("hostname RouterC\n\n")
        f.write("interface eth0/0\n")
        f.write("    ip address 1.0.0.2 255.255.255.0\n")
        f.write("interface eth0/1\n")
        f.write("    ip address 2.0.0.2 255.255.255.0\n")
        f.write("interface eth0/2\n")
        f.write(f"    ip address 3.0.0.1 255.255.255.0\n\n")
        f.write("router bgp 11\n")
        f.write("    neighbor 1.0.0.1 remote-as 10\n")
        f.write("    neighbor 2.0.0.1 remote-as 20\n")
        f.write(f"    neighbor 3.0.0.2 remote-as 30\n")
        f.write(f"    {agg_str}\n")



