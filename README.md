![github-precius-logo](https://user-images.githubusercontent.com/55578225/180665298-f73379a8-5dbf-4cae-aa20-9150686e0241.png)

# Precius-Project V0.9
Precius is an OS security solution. (Our school annual Project for 2021-2022)
This solution is accurately an security module "encapsulator" open-source and ultra-configurable for Windows. It's a lightweight solution, it doesn't do anything until it's specifically configured .

Precius allows you to import your own security modules made in any language, whether it's from GitHub, an existing and known module, or a completely home-made module. It has a sector system that allows you to indicate which context folders and modules should be used. This allows you to perfectly control the use and the computing power of the modules, and thus of Precius in general.

# Requirement
A machine on Windows 10

# Installation
- Download the lastest version of Précius from the [Précius_pack](https://github.com/Vadimador/Precius-Project/blob/main/precius_pack.zip) section.
- Extract the program package
1) Install the minifilter by right clicking and installing the `.inf` file, than with the cmd with admin right :
```
net start FsFilter_Final
```
2) Run the script 'install.ps1' on system to install Précius.
3) configure your own modules and sectors in C:\Program Files\Precius\modules.conf and sectors.conf

# Configuration
To configure modules and sectors (if it does not exist, start Precius service to create it) : 
- C:\Program Files\Precius\modules.conf
- C:\Program Files\Precius\sectors.conf

# Usage
- Go to Services and start Precius service or exec on powershell : Start-Service -name Precius.
- In the precius_packet you will get, the minifilter, the windows service and the Précius Talker for the user to communicate with it.

# Précius Talker Command (same as the help)
```
 show_sectors --> show the sectors table of the Précius service.
 show_modules --> show the modules table of the Précius service.
 module <module name> [argument] --> execute a module configured in Precius service.
 convert_path_to_sector <path> --> Returns the sector number assigned to this path.
 send_signal <signal> --> Simulates a signal from the minifilter.
           => signal example : "filescan|C:\path\to\file.txt"
```
