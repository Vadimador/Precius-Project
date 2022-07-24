# Precius-Project
Precius is an OS security solution. 
This solution is accurately an security module "encapsulator" open-source and ultra-configurable for Windows. It's a lightweight solution, it doesn't do anything until it's specifically configured .

Precius allows you to import your own security modules made in any language, whether it's from GitHub, an existing and known module, or a completely home-made module. It has a sector system that allows you to indicate which context folders and modules should be used. This allows you to perfectly control the use and the computing power of the modules, and thus of Precius in general.

# Requirement
A machine on Windows 10

# Installation
- Download the lastest version of Précius from the [Précius_pack](https://github.com/Vadimador/Precius-Project/blob/main/precius_pack.zip) section.
- Extract the program package
- Run the script 'install.ps1' on system to install Précius.

# Configuration
To configure modules (if it does not exist, start Precius service to create it) : 
	- C:\Program Files\Precius\modules.conf
To configure sectors (same)
	- C:\Program Files\Precius\sectors.conf

# Usage
- Go to Services and start Precius service or exec on powershell : Start-Service -name Precius.
