
from __future__ import print_function

from sys import stdout
from time import sleep

messages = [
    'announce route 99.192.166.32/24 next-hop 99.160.16.3 local-preference 134 as-path [0] med 176 origin egp community [65324:5 6707:1]',
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
