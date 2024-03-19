"""
protocol kernel {
  metric 0;
  import none;
  learn;
  export all;
}
protocol device {
}
protocol direct {
}
protocol bgp peer1 {
  local as 43617;
  neighbor 3.0.0.3 as 43620;
  import all;
  export all;
}

"""

with open("generated_commands_bird1.txt", "w") as g:
    g.write('s2 bash -c "> /etc/bird/bird.conf"\n')
    g.write('s2 bash -c "echo \'protocol kernel {\' >> /etc/bird/bird.conf"\n')
    g.write('s2 bash -c "echo \'  metric 0;\' >> /etc/bird/bird.conf"\n')
    g.write('s2 bash -c "echo \'  import none;\' >> /etc/bird/bird.conf"\n')
    g.write('s2 bash -c "echo \'  learn;\' >> /etc/bird/bird.conf"\n')
    g.write('s2 bash -c "echo \'  export all;\' >> /etc/bird/bird.conf"\n')
    g.write('s2 bash -c "echo \'}\' >> /etc/bird/bird.conf"\n')
    g.write('s2 bash -c "echo \'protocol device {\' >> /etc/bird/bird.conf"\n')
    g.write('s2 bash -c "echo \'}\' >> /etc/bird/bird.conf"\n')
    g.write('s2 bash -c "echo \'protocol direct {\' >> /etc/bird/bird.conf"\n')
    g.write('s2 bash -c "echo \'}\' >> /etc/bird/bird.conf"\n')
    g.write('s2 bash -c "echo \'protocol bgp peer1 {\' >> /etc/bird/bird.conf"\n')
    g.write('s2 bash -c "echo \'  local as 43617;\' >> /etc/bird/bird.conf"\n')
    g.write('s2 bash -c "echo \'  neighbor 7.0.0.3 as 43617;\' >> /etc/bird/bird.conf"\n')
    g.write('s2 bash -c "echo \'  import all;\' >> /etc/bird/bird.conf"\n')
    g.write('s2 bash -c "echo \'  export all;\' >> /etc/bird/bird.conf"\n')
    g.write('s2 bash -c "echo \'}\' >> /etc/bird/bird.conf"\n')
            
   

            
    