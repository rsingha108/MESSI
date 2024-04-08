three-routers.py : simple topology with three 2 exabgp and 1 router.

three-router-bird.py : bird-variant (not used)

command-generator-3r.py : main file to run for getting preference order result for Quagga & FRR (results_quagga.txt & results_frr.txt). This runs three-routers.py and subsequnetly parser.py.
						
						takes input 'zen_out.txt' which is output of the Zen (test cases)
						
						creates 'generated_command.txt', 'correct.txt' (correct outputs from my decision_maker function), and results for quagga & frr.
						
						running command : "sudo python command-generator-3r.py" (need not worry about the command-line arguments, those are taken care within this file)
						
						change the global variable 'sw' for changing software : Quagga/FRR
						
evaluator.py : 	This is optional. If you want to check correct outputs and model outputs automatically.

topo.png : Topology for Preference order of Quagga/FRR; this is a bit different than topo of BIRD (here s2 is the target, in BIRD s1 is the target)

peer*.txt : bird configs to incorporate by source command (unsuccessful)
