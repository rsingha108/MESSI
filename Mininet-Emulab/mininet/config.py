import mininet.node

class Subnet:
    def __init__(self, ipStr=None, prefixLen=None):
        self.prefixLen = prefixLen
        self.ipStr = self.extractPrefix(ipStr, prefixLen)
        self.ip = Subnet.strToIp(self.ipStr)
        self.ptr = 0 if prefixLen == 32 else 1
        self.limit = pow(2, 32 - prefixLen)
        self.bitmap = [0] * self.limit
        self.nodeList = []

        if self.prefixLen is None or self.ipStr is None:
            print("Configuration is invalid, prefixLen: %s, ipStr: %s" % (prefixLen, ipStr))
    
    def allocateIPAddr(self):
        """
        Allocate an ip address automatically(with netmask)
        """
        if self.ptr >= self.limit:
            print("Subnet %s/%d has run out of address space!" % {self.ipStr, self.prefixLen})
            return None

        # search for an available address
        while self.bitmap[self.ptr]:
            self.ptr += 1
        # compute the new ip and update bitmap & pointer
        newIp = self.ip + self.ptr
        self.bitmap[self.ptr] = True
        self.ptr = self.ptr + 1

        return Subnet.ipToStr(newIp) + '/' + str(self.prefixLen)
    
    def assignIpAddr(self, ipStr):
        """
        Assign a certain ip address designated by arg:ipStr.
        Return the ip address with netmask if the operation succeeds, none otherwise.
        """
        if self.ipStr != Subnet.extractPrefix(ipStr, self.prefixLen):
            print("Mismatched IP prefix!")
            return None
        
        segmentIndex = Subnet.strToIp(ipStr) - self.ip
        if self.bitmap[segmentIndex]:
            print("The IP address has been allocated")
            return None
        else:
            self.bitmap[segmentIndex] = True
            return ipStr + "/" + str(self.prefixLen)

    def getNetworkPrefix(self):
        return self.ipStr + "/" + str(self.prefixLen)

    def getPrefixLen(self):
        return self.prefixLen

    def addNode(self, node1, node2=None):
        """
        Add one node or a node pair into subnet 
        """
        assert node1 != None
        self.nodeList.append(node1)
        if node2 != None:
            self.nodeList.append(node2)
    
    def installSubnetTable(self):
        """
        Install MAC table entries for each node in the subnet
        """
        if self.limit == 1:
            print("No need to install mac tables")
            return

        macTable = []
        for i in range(0, self.limit):
            if self.bitmap[i]:
                ip = self.ip + i
                macTable.append([Subnet.ipToStr(ip), Subnet.ipToMac(Subnet.ipToStr(ip))])

        for node in self.nodeList:
            if issubclass(type(node), mininet.node.DockerRouter):
                node.installSubnetTable(macTable, self)

    @staticmethod
    def ipToMac(ipStr):
        if '/' in ipStr:
            ipStr = ipStr.split('/')[0]
        macSuffix = ['0'] * 8
        ip = Subnet.strToIp(ipStr)
        pos = 8
        while ip != 0:
            pos = pos - 1
            macSuffix[pos] = hex(ip % 16)[2:]
            ip = ip >> 4

        macStr = "00:00"
        for i in range(0, 8, 2):
            macStr += ":{}{}".format(macSuffix[i], macSuffix[i+1])
        return macStr

    @staticmethod
    def extractPrefix(ipStr, prefixLen):
        """
        Extract the prefix of the arg:ipStr based on arg:prefixLen
        """
        # transform the str into subnet prefix
        ip = Subnet.strToIp(ipStr)
        ip = (ip >> (32 - prefixLen)) << (32 - prefixLen)

        # transform the subnet prefix into str
        return Subnet.ipToStr(ip)

    @staticmethod
    def strToIp(ipStr):
        """
        Transform ip string into ip integer
        """
        segments = str(ipStr).split('.')
        nums = [int(i) for i in segments]
        ip = nums[0] * pow(2, 24) + nums[1] * pow(2, 16) + nums[2] * pow(2, 8) + nums[3]
        return ip

    @staticmethod
    def ipToStr(ip):
        """
        Transform ip integer into ip string
        """
        nums = [0] * 4
        for i in range(4):
            nums[i] = ((ip << (i * 8) & 0xffffffff) >> 24)

        return "%d.%d.%d.%d" % (nums[0], nums[1], nums[2], nums[3])

class NodeList:
    def __init__(self):
        self.nodeDict = dict()

    def addNode(self, name, ip, nodeType):
        if name in self.nodeDict.keys():
            return
            
        self.nodeDict[name] = [0, ip.split("/")[0], nodeType]

    def addLink(self, name1, name2, ip1, ip2):
        port1 = self.nodeDict[name1][0]
        self.nodeDict[name1].append("{}-{}-{}".format(port1, ip1.split("/")[0], name2))
        self.nodeDict[name1][0] += 1

        port2 = self.nodeDict[name2][0]
        self.nodeDict[name2].append("{}-{}-{}".format(port2, ip2.split("/")[0], name1))
        self.nodeDict[name2][0] += 1

    def writeFile(self, filepath):
        with open(filepath, "w") as file:
            for node in self.nodeDict.keys():
                file.write(node)
                for item in self.nodeDict[node][1:]:
                    file.write(" " + str(item))

                file.write("\n")

# used for test
if __name__ == "__main__":
    snet = Subnet(ipStr="10.1.0.0", prefixLen=24)
    a = snet.allocateIPAddr()
    b = snet.allocateIPAddr()
    print(a)
    print(b)
    print(Subnet.ipToMac(a))