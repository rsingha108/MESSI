def convert_prefix_to_uint(prefix):
    prefix = prefix.split(".")
    p1 = int(prefix[0])
    p2 = int(prefix[1])
    p3 = int(prefix[2])
    p4 = int(prefix[3])

    return  p1 * (1 << 24) + p2 * (1 << 16) + p3 * (1 << 8) + p4


def convert_uint_to_prefix(num):
    p1 = num // (1 << 24)
    num %= (1 << 24)
    p2 = num // (1 << 16)
    num %= (1 << 16)
    p3 = num // (1 << 8)
    num %= (1 << 8)
    p4 = num

    return str(p1) + "." + str(p2) + "." + str(p3) + "." + str(p4)

def get_mask(mask):
    num = 0
    k = 31
    for i in range(mask):
        num |= (1 << k)
        k -= 1

    return num

def inv(i):
    if i == "1":
        return "0"
    else:
        return "1"
    
def dec_to_binary(num, digits):
    s = ""
    for _ in range(digits):
        s = s + str(num & 1)
        num >>= 1
    return s[::-1]
    

def convert_prefix_to_binary(prefix_list):
    binary_set = []
    for p in prefix_list:
        prefix = convert_prefix_to_uint(p["prefix"])
        mask = get_mask(p["mask"])
        prefix = prefix & mask
        binary_set.append(
            (('' if p['mask'] == 0 else dec_to_binary(prefix >> (32 - p["mask"]), p["mask"]),
            p["ge"],
            p["le"]),
            p["permit"])
        )

    # print(binary_set)

    return binary_set


def convert_binary_to_prefix(binary_set):
    prefix_set = []
    # print(binary_set)
    for b in binary_set:
        mask = len(b[0])
        ge = b[1]
        le = b[2]
        prefix = convert_uint_to_prefix(0 if b[0] is '' else (int(b[0], 2) << (32 -mask)))
        prefix_set.append(
            {
                "prefix": prefix,
                "mask": mask,
                "le": max(le,mask),
                "ge": max(ge,mask)
            }
        )

    return prefix_set

def LeGeOverlapHandler(pre, ge1, le1, ge2, le2):
    _s = []
    
    if ge2 <= ge1 and le2 >= le1:
        pass
    elif ge2 > ge1 and le2 < le1:
        _s.append((pre, ge1, ge2-1))
        _s.append((pre, le2+1, le1))
    elif ge2 <= ge1 and le2 < le1:
        _s.append((pre, le2+1, le1))
    elif ge2 > ge1 and le2 >= le1:
        _s.append((pre, ge1, ge2-1))
    return _s

def subs(a,b):
    r = []
    if len(a) < len(b):
        if b.startswith(a):
            x = b[len(a):]
            for i in range(len(x)):
                _x = x
                _x = _x[:i] + inv(_x[i]) + _x[i+1:]
                r.append(a + _x[:i+1])
        else:
            r.append(a)
    else:
        if not a.startswith(b):
            r.append(a)
    return r

def subs_prefix(p,s):
    r = []
    for q in p:
        a = q[0]
        b = s[0]
        ge1 = q[1]
        le1 = q[2]
        ge2 = s[1]
        le2 = s[2]

        if ge2 <= ge1 and le2 >= le1:
            if subs(a,b) != []:
                r += [(i, ge1, le1) for i in subs(a,b)]
        elif ge2 > ge1 and le2 < le1:
            r += [(a, ge1, ge2-1), (a, le2+1, le1)]
            r += [(i, ge2, le2) for i in subs(a,b)]
        elif ge2 <= ge1 and le2 < le1 and le2 >= ge1:
            r += [(a, le2+1, le1)]
            r += [(i, ge1, le2) for i in subs(a,b)]
        elif ge2 > ge1 and le2 >= le1 and ge2 <= le1:
            r += [(a, ge1, ge2-1)]
            r += [(i, ge2, le1) for i in subs(a,b)]
        else:
            r += [(a, ge1, le1)]
    return r
        
    
            
    
    
def subs_rectangle(recs, new_rec):
    _recs = []    
    ge2 = new_rec[1]
    le2 = new_rec[2]
        
    for q in recs:
        ge1 = q[1]
        le1 = q[2]
        
        if len(q[0]) < len(new_rec[0]):
            if new_rec[0].startswith(q[0]):
                if ge1 <= le2 and ge2 <= le1:
                    x = new_rec[0][len(q[0]):]
                    for i in range(len(x)):
                        _x = x
                        _x = _x[:i] + inv(_x[i]) + _x[i+1:]
                        _recs.append((q[0] + _x[:i+1], ge1, le1))
                    
                    _recs.extend(LeGeOverlapHandler(new_rec[0], ge1, le1, ge2, le2))
                        
                else:
                    _recs.append(q)
            else:
                _recs.append(q)
                
        else:
            if q[0].startswith(new_rec[0]):
                if ge1 <= le2 and ge2 <= le1:
                    _recs.extend(LeGeOverlapHandler(new_rec[0], ge1, le1, ge2, le2))
                else:
                    _recs.append(q)
            else:
                _recs.append(q)
    return _recs


def convert_prefix_list_to_prefix_set(prefix_list):
    pref_set = []
    pd = [] ## allow/deny
    prefix_list = convert_prefix_to_binary(prefix_list)
    print("@@@prefix-list: ",prefix_list)
    for i in prefix_list:
        p = [i[0]]
        for s in pref_set:
            # p = subs_rectangle(p,s)
            p = subs_prefix(p,s)
        pref_set = pref_set + p
        print("@@@pref_set: ",pref_set)
        if p!=[]:
            for _ in range(len(p)):
                pd.append(i[1])

    final_pref_set = []
    for i in range(len(pd)):
        x = pd[i]
        if x:
            final_pref_set.append(pref_set[i])

    print("@@@final: ",final_pref_set)

    return convert_binary_to_prefix(final_pref_set)

