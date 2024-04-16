# MESSI : Behavioral Testing of BGP Implementations

This repository contains the code for our work [MESSI](https://www.usenix.org/conference/nsdi24/presentation/singha), which is the first automatic test generator for black-box BGP implementations.

## Test Generation

Navigate to `BGP_Zenv2.0` folder and follow the instructions provided in the README.

## Testing BGP Implementations

As a first step, install Containernet on your system following the instructions provided [here](https://github.com/rsingha108/MESSI/blob/main/Mininet-Emulab/README.md). Afterwards, navigate to the required folder:

```
$ cd Mininet-Emulab/mininet/demo/
```

Move the `tests` folder from `BGP_Zenv2.0` to this location. Depending upon which BGP feature you generated test cases for, you may do any one of the following:

### Decision Process

```
$ cd Decision-Process
$ sudo python3 main.py
```

### Aggregation

```
$ cd Aggregation
$ sudo python3 main.py --software <software>
```

where `<software>` is the name of the BGP implementation to be tested. The possible values are `frr`, `quagga`

For testing aggregation with Batfish:

```
$ sudo python3 agg_batfish.py
```

### Route Filtering

```
$ cd Symb-Route-Maps
```

#### FRR

```
$ cd frr
```

To run the route filtering tests:
```
$ sudo python3 one-router.py 
```

To run the route-map dynamics tests:
```
$ sudo python3 dynamic-main.py 
```

#### Quagga

```
$ cd quagga
```

To run the route filtering tests:
```
$ sudo python3 one-router.py 
```

To run the route-map dynamics tests:
```
$ sudo python3 dynamic-main.py 
```

#### GoBGP
```
$ cd gobgp
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

#### Batfish
```
$ cd batfish
```

To run the route filtering tests (within the Batfish docker container):
```
$ sudo python3 main.py 
```

#### BIRD
```
$ cd bird
```

To run the route filtering tests:
```
$ bash run.sh
```
