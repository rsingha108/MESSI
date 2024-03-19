import subprocess

subprocess.run("sudo docker stop bird-docker_peer3_1".split())
subprocess.run("sudo docker stop bird-docker_peer2_1".split())
subprocess.run("sudo docker stop bird-docker_peer1_1".split())

subprocess.run("sudo docker rm bird-docker_peer1_1".split())
subprocess.run("sudo docker rm bird-docker_peer2_1".split())
subprocess.run("sudo docker rm bird-docker_peer3_1".split())

subprocess.run("sudo docker rmi bird-docker_peer1".split())
subprocess.run("sudo docker rmi bird-docker_peer2".split())
subprocess.run("sudo docker rmi bird-docker_peer3".split())

