# MESSI : Behavioral Testing of BGP Implementations

## Decision Process

* Directory: Mininet-Emulab/mininet/demo/Decision-Process/

* three-routers.py : simple topology with three 2 exabgp and 1 router.

* command-generator-3r.py : main file to run for getting preference order result for Quagga & FRR (results_quagga.txt & results_frr.txt). This runs three-routers.py and subsequnetly parser.py. takes input 'zen_out.txt' which is output of the Zen (test cases) creates 'generated_command.txt', 'correct.txt' (correct outputs from my decision_maker function), and results for quagga & frr. 

running command : "sudo python command-generator-3r.py" (need not worry about the command-line arguments, those are taken care within this file). change the global variable 'sw' for changing software : Quagga/FRR

## Route Filtering

* Directory: Mininet-Emulab/mininet/demo/Symb-Route-maps/

### FRR (Mininet-Emulab/mininet/demo/Symb-Route-maps/frr)

* one-router.py : For running route filtering tests (results stored in /results directory)

* dynamic-main.py : For testing dynamics of Route maps

### Quagga (Mininet-Emulab/mininet/demo/Symb-Route-maps/quagga)

* one-router.py : For running route filtering tests (results stored in /results directory)

* dynamic-main.py : For testing dynamics of Route maps

### GoBGP
The [GoBGP Github page](https://github.com/osrg/gobgp) provides no documentation for enabling route aggregation. Hence we do not have
a setup for testing it.

Navigate to the GoBGP folder:
```
$ cd Mininet-Emulab/mininet/demo/Symb-Route-Maps/gobgp
```
For running the route-map test cases, ensure that the **tests** directory is located outside the **gobgp** directory.

Inside the **gobgp** directory, create the results folder:
```
$ mkdir results
```

Route-map test cases can be run with the following command:
```
$ python3 main.py
```

For route-map dynamics, use the following:
```
$ python3 compare_main.py
```

### Batfish (Mininet-Emulab/mininet/demo/Symb-Route-maps/batfish)

main.py : For running route filtering tests (results stored in /results directory)

