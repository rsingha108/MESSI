def is_subnet(network1, network2):
    """
    Check if network2 is a subnet of network1.
    Both network1 and network2 are tuples of the form (network_address, prefix_length).
    """
    # Convert network addresses to binary representation
    net1_address_bin = ''.join([bin(int(x) + 256)[3:] for x in network1[0].split('.')])
    net2_address_bin = ''.join([bin(int(x) + 256)[3:] for x in network2[0].split('.')])
    
    # Compare the network portion
    return net1_address_bin[:network1[1]] == net2_address_bin[:network1[1]]


