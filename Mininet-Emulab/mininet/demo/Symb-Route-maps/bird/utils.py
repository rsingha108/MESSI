

def str2list_com(s): ## for community
    s1 = [i.split(',') for i in s[2:-2].split('],[')]
    s2 = [['('+ c.replace(':',',') +')' for c in p] for p in s1]
    return s2

def str2list_delcom(s): ## for delete community
    s1 = s[1:-1].split(',')
    s2 = ['('+c.replace(':',',')+')' for c in s1]
    return s2

def str2list_asp(s): ## for aspath
    s1 = [i.replace(',',' ') for i in s[2:-2].split('],[')]
    return s1

def pref_corrector(p):
    ## convert to binary
    p1 = p.split('/')[0] ## prefix
    p2 = p.split('/')[1] ## len
    p3 = [bin(int(i))[2:].zfill(8) for i in p1.split('.')] ## list of bin strings of len 8
    p4 = ''.join(p3) ## bin string of len 32
    p5 = p4[:int(p2)] + '0'*(32-int(p2)) ## bin string of len 32 with 0s at the end after masking
    p6 = [p5[i:i+8] for i in range(0, len(p5), 8)] ## list of bin strings of len 8
    p7 = '.'.join([str(int(i,2)) for i in p6]) ## prefix in dotted decimal
    return p7 + '/' + p2 ## prefix in dotted decimal with len

def pref_maker(p,g,l):
    p = pref_corrector(p)
    plen = p.split('/')[1]
    ge = g if g != "None" else plen
    le = l if l != "None" else "32"
    if g == "None" and l == "None":
        pref = p
    else:
        pref = '[' + p + "{" + ge + "," + le + "}" + ']' 
    return pref