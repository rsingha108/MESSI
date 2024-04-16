



def translate(test, subnet_ips):
  nh1, ip1, nh3, ip3, nh4, ip4 = subnet_ips
  # print(test)
  d1,d2,d3,d4 = {},{},{},{}
  ## configs of router s2
  d2["IP1"],d2["IP3"],d2["IP4"],d2["AS"] = ip1, ip3, ip4, test["RouterAS"]

  ## configs of router 
  #AS Number, Local Preference, AS_Path Length, Origin, MED, IGP Metric, Send/Arrival Time, Router ID, Neighbor Address

  # NH = nexthop in network structure, NextHop = nexthop in the route attribute
  ## route 1 coming from s1. 
  d1["AS"],d1["LP"],d1["ASP"],d1["ORG"],d1["MED"],d1["IGP"],d1["AT"],d1["RID"],d1["NH"],d1["NextHop"] = test["Route1"]["ASN"], test["Route1"]["LP"], test["Route1"]["ASPathLength"], test["Route1"]["Origin"], test["Route1"]["MED"], test["Route1"]["IGP"], test["Route1"]["ArrivalTime"], test["Route1"]["RID"], nh1, test["Route1"]["NgbrAddr"]
  ## route 3 coming from s3
  d3["AS"],d3["LP"],d3["ASP"],d3["ORG"],d3["MED"],d3["IGP"],d3["AT"],d3["RID"],d3["NH"],d3["NextHop"] = test["Route2"]["ASN"], test["Route2"]["LP"], test["Route2"]["ASPathLength"], test["Route2"]["Origin"], test["Route2"]["MED"], test["Route2"]["IGP"], test["Route2"]["ArrivalTime"], test["Route2"]["RID"], nh3, test["Route2"]["NgbrAddr"]
  d4["NH"], d4["AS"] = nh4, d2["AS"]
  ## decision
  if test["Decision"]["NgbrAddr"] == test["Route1"]["NgbrAddr"] : dec = 1
  else : dec = 3

  return d1,d2,d3,d4,dec
