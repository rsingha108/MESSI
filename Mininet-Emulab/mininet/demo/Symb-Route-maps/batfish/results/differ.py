with open('results_tests2.txt','r') as f:
  lines =  f.readlines()

with open('diff.txt','w') as g:
  for line in lines:
    _line = line[:-1]
    sp = _line.split(',')
    if (sp[1] == 'no' or sp[2] == 'no'):
      g.write(line)
