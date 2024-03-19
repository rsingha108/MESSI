import utils

def to_dict():
    with open('../../../../../BGP_Zenv2.0/CLI/regex-pos-neg.txt','r') as f:
        lines = f.readlines()

    com_regex_lines =  [1,2,3,4]
    # pos_lines = [7,8,9,10]
    # neg_lines = [19,20,21,22]

    com_regex_dict = {}
    for i in com_regex_lines:
        com_regex_dict[lines[i][:-1]] = {'pos': utils.str2list_com(lines[i+6][:-1]), 'neg': utils.str2list_com(lines[i+18][:-1])}

    as_regex_lines = [52,53,54,55]
    as_regex_dict = {}
    for i in as_regex_lines:
        as_regex_dict[lines[i][:-1]] = {'pos': utils.str2list_asp(lines[i+6][:-1]), 'neg': utils.str2list_asp(lines[i+12][:-1])}

    delcom_lines = [31,32,33,34]
    delcom_dict = {}
    for i in delcom_lines:
        delcom_dict[lines[i][:-1]] = utils.str2list_delcom(lines[i+6][:-1])

    return com_regex_dict, as_regex_dict, delcom_dict

    # for k,v in regex_dict.items():
    #     print(f"{k} : {v}")