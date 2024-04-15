# MESSI : Behavioral Testing of BGP Implementations

# Test Generation

Navigate to the BGP_Zenv2.0 folder and follow the instructions in the README.md file.

# Testing BGP Implementations

## Decision Process

* Directory: Mininet-Emulab/mininet/demo/Decision-Process/

```
sudo python3 main.py
```

## Route Filtering

* Directory: Mininet-Emulab/mininet/demo/Symb-Route-maps/

### FRR (Mininet-Emulab/mininet/demo/Symb-Route-maps/frr)

To run the route filtering tests:
```
sudo python3 one-router.py 
```

To run the route-map dynamics tests:
```
sudo python3 dynamic-main.py 
```

### Quagga (Mininet-Emulab/mininet/demo/Symb-Route-maps/quagga)

To run the route filtering tests:
```
sudo python3 one-router.py 
```

To run the route-map dynamics tests:
```
sudo python3 dynamic-main.py 
```

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

To run the route filtering tests (within the Batfish docker container):
```
sudo python3 main.py 
```
