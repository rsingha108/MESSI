
from __future__ import print_function

from sys import stdout
from time import sleep

messages = [
    'announce route 99.192.1.4/0 next-hop 100.0.0.3 local-preference 129 as-path [0 4] med 136 origin egp community [1:1 2:1]',
]

sleep(5)

#Iterate through messages
for message in messages:
    stdout.write(message + '\n')
    stdout.flush()
    sleep(1)

#Loop endlessly to allow ExaBGP to continue running
while True:
    sleep(1)
