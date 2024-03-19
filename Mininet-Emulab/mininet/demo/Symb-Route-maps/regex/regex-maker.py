############ FUNCTIONS ############

def bases(): ## return a list of base chars, Level 0
    return ['a']

## Level 1 regexes building on top of Level 0 regexes
def rgx1(r1):  
    
    r2 = [] + r1
    x = r1[0]
    r2.append(f'{x}*') ## Kleene star
    r2.append(f'{x}+') ## Kleene plus
    r2.append(f'{x}?') ## Optional
    for i,r in enumerate(r1): 
        for s in (r1[i:]): 
            if r == s and r != x: continue 
            r2.append(f'{r}|{s}') ## Alternation
            # r2.append(f'{r}{s}') ## Concatenation is not counted in this level 
            ## as 'aa' and 'a' are the same regexes in this level
    return r2

## Level 2 regexes building on top of Level 1 regexes
def rgx2(r1): 
    r2 = [] + r1
    for i,r in enumerate(r1):
        if len(r) != 1:  
            r2.append(f'({r})*')
            r2.append(f'({r})+')
            r2.append(f'({r})?')     
        for s in (r1[i:]): 
            if r == s and r == 'a': continue
            r2.append(f'({r})|({s})') 
            r2.append(f'({r})({s})') ## Concatenation
    
    return r2

############ MAIN ############

alphabet = ['(', ')', 'a', 'b', '*', '|']

l0 = bases()
l1 = rgx1(l0)
l2 = rgx2(l1)

for r in l2:
    for s in l2:
        com = f'{r}:{s}'
        
