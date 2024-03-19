
from __future__ import print_function

from sys import stdout
from time import sleep

messages = [
    'announce route 99.190.185.192/1 next-hop 100.4.8.3 local-preference 261 as-path [1 2222] med 16 origin egp community [1:1 11210:1]',
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
