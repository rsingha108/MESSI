
from __future__ import print_function

from sys import stdout
from time import sleep

messages = [
    'announce route 100.0.1.128/0 next-hop self local-preference 161 as-path [0] med 33 origin egp community [11011:11 12:10]',
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
