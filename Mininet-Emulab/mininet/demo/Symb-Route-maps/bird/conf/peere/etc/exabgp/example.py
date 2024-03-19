
from __future__ import print_function

from sys import stdout
from time import sleep

messages = [
    'announce route 100.23.127.63/6 next-hop 99.218.8.65 local-preference 262 as-path [1 31] med 129 origin egp community [2:2 2:22]',
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
