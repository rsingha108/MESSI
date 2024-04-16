def generate_subnet_ips(d1,d2,d3,d4):
	nh1 = d1["NH"]
	ip1 = d2["IP1"]
	nh4 = d4["NH"]
	ip4 = d2["IP4"]
	nh3 = d3["NH"]
	ip3 = d2["IP3"]
	return nh1, ip1, nh4, ip4, nh3, ip3