# BGP_Zenv2.0
This folder contains code for generating BGP test cases using the Zen library in C#.

First download .NET 5.0:
```
$ sudo apt-get update
$ wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
$ sudo dpkg -i packages-microsoft-prod.deb
$ sudo apt-get install -y dotnet-sdk-5.0
$ rm packages-microsoft-prod.deb
```

Navigate to the CLI folder and make the **tests** directory:
```
$ cd CLI
$ mkdir tests
```

Here are all the command line options:
```
-l, --length            (Default: 4) The maximum number of regular expressions used for generating AS path and community examples

-s, --path-selection    (Default: false) Flag to enable generation of path selection test cases

-m, --route-map         (Default: false) Flag to enable generation of route-map test cases

-a, --aggregation       (Default: false) Flag to enable generation of aggregation test cases

-d, --dynamic           (Default: false) Flag to enable generation of dynamic test cases. Must be used in combination with a and m flags

-r, --route             (Default: false) Flag to enable generation of route dynamics test-cases. Must be used in combination with a and m flags

--help                  Display this help screen.

--version               Display version information.
```

For example, to generate all test cases for route aggregation with configuration dynamics, you should run:
```
$ dotnet run -a -d
```

Similarly for route aggregation with route dynamics, use the following command:
```
$ dotnet run -a -r
```

All test cases will be saved in separate JSON files within the **tests** directory. The set of regexes used for generating the current set of tests will be written to **regex-pos-neg.txt**.