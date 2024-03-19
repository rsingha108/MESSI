import re

def cidr_to_bin(cidr):
    """Convert CIDR notation to a binary string."""
    ip, prefix = cidr.split('/')
    prefix = int(prefix)
    # Convert IP to binary
    ip_bin = ''.join([bin(int(x)+256)[3:] for x in ip.split('.')])
    # Get network binary
    net_bin = ip_bin[:prefix]
    return net_bin, prefix

def is_subnet(network1, network2):
    """Check if network2 is a subnet of network1."""
    net1_bin, prefix1 = cidr_to_bin(network1)
    net2_bin, prefix2 = cidr_to_bin(network2)
    
    # Network2 must have a larger prefix (smaller network) and the beginning of both networks must match
    return prefix2 >= prefix1 and net1_bin == net2_bin[:prefix1]


def valid_test(line):

# line = "(Aggregate Route: 100.0.114.131/32, Summary-Only: False, MatchingMEDOnly: True, Router AS: 0, Prefix: 99.211.130.37/22, Local Preference: 161, MED: 292, Community: [22:1, 21112:21], AS Path: [0, 5], Next Hop: 100.0.1.8, Prefix: 99.218.148.47/0, Local Preference: 580, MED: 152, Community: [1:20, 1:100], AS Path: [1, 2], Next Hop: 100.30.0.64, Prefix: 99.218.148.47/0, Local Preference: 580, MED: 292, Community: [1:20, 1:100], AS Path: [1, 2], Next Hop: 100.15.0.3)"

    # Pattern to match the aggregate route
    aggregate_route_pattern = r"Aggregate Route: ([\d.]+/\d+)"
    aggregate_route = re.search(aggregate_route_pattern, line)
    if aggregate_route:
        aggregate_route = aggregate_route.group(1)

    # Pattern to match the prefixes and their details
    prefix_pattern = r"Prefix: ([\d.]+/\d+), Local Preference: (\d+), MED: (\d+), Community: \[([^\]]+)\], AS Path: \[([^\]]+)\], Next Hop: ([\d.]+)"
    prefixes = re.findall(prefix_pattern, line)

    # Extracting details for each prefix
    prefix_details = []
    for prefix in prefixes:
        details = {
            'Prefix': prefix[0],
            'Local Preference': int(prefix[1]),
            'MED': int(prefix[2]),
            'Community': prefix[3],
            'AS Path': prefix[4],
            'Next Hop': prefix[5],
        }
        prefix_details.append(details)

    # Printing the results
    # print(f"Aggregate Route: {aggregate_route}")
    # for i, prefix_detail in enumerate(prefix_details, 1):
    #     print(f"Prefix {i}: {prefix_detail['Prefix']}")

    b0 = is_subnet(aggregate_route, prefix_details[0]['Prefix'])
    b1 = is_subnet(aggregate_route, prefix_details[1]['Prefix'])
    b2 = is_subnet(aggregate_route, prefix_details[2]['Prefix'])

    return(b0 or b1 or b2)


with open("agg_med.txt", "r") as f:
    lines = f.readlines()

count = 0
for line in lines:
    if line.startswith("("):
        if valid_test(line):
            print(line)
            count += 1

print("Number of valid tests: ", count)