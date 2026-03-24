

# Simple Neutrino Loader (SNL)

This is a PlayStation2 game loader that runs on the [Enceladus](https://github.com/DanielSant0s/Enceladus) Lua environment.  
SNL is a highly customizable interface that runs [neutrino](https://github.com/rickgaiser/neutrino).  
To use this software you will need a ps2 capable of running homebrew, such as FreeMCBoot or PS2BBL.  
The full-install size (SNL, Enceladus, and all device drivers) is less than 1.4MB and can be installed on a memory card.  
The Lua interface code totals to around 300 lines which makes mods easy.  
LaunchELF's text editor paired with a USB keyboard allows changes to be made from the PS2 itself.  

## Controls:

Hold ↑UP or forward on the left analog stick to scroll up.  
Hold ↓DOWN or backwards on the left analog stick to scroll down.  
Press ❎CROSS, ⏺CIRCLE, or START to play the selected game.  
Press ⏹SQUARE or 🔼TRIANGLE to show what settings neutrino.elf will be passed.  
Hold SELECT to exit to browser.  


## UDPBD Setup:

The SNL-UDPBDG or SNL-CLI application can be used to automatically install Enceladus, SNL, and drivers for UDPBD gameplay.  

To save 210KB of space SNL-UDPBDG only includes drivers for UDPBD.  
For HDD, UDPBD-HDD, HDL, ILINK, MMCE, MX4, or USB use the SNL-Full release.  

Download SNL-UDPBDG from [here](https://github.com/MegaBitmap/PS2-SNL/releases).  
Extract the UDPBDG folder to the Desktop or Documents.  
It is a portable app, do NOT use Program Files or any other system related folder.  
**DIRECT Connection** - Plug in the ethernet cable as shown: ↓  
![ps2-slim-connected-to-laptop](readmeImages/ps2-slim-connected-to-laptop.jpg)  
For a **DIRECT** connection set a manual IPv4 address and subnet mask.  
![pc-ip-settings](readmeImages/pc-ip-settings.jpg)  

**ROUTER Connection** - Plug in the ethernet cables as shown: ↓  
![ps2-slim-connected-to-router-and-laptop](readmeImages/ps2-slim-connected-to-router-and-laptop.jpg)  
For a **ROUTER** connection, set the PC's IP assignment to *Automatic (DHCP)*.  
- On the PC go into network connections or run `ncpa.cpl`.  
- Right click the Ethernet adapter and select properties, then select IPv4 properties.  
- Enable "Obtain an IP address automatically".  

For a **ROUTER** connection the PS2 IP address settings for launchELF need to be changed.  
For the first IP address, the first three parts must match the default gateway of the router or switch.  
Example: 192.168.1.?  
The last part (?) needs to be a unique number between 2 and 254, example: `192.168.1.147`  
Set the middle number, subnet mask, to `255.255.255.0`  
The third IP address or default gateway, must match the default gateway of the router or switch.  
Most likely it will be `192.168.0.1` or `192.168.1.1`  
[View this guide](http://ps2ulaunchelf.pbworks.com/w/page/19520139/ps2ftp) for more info on how to set the IP address on the PS2.  

Make sure that the memory card has at least 1.2MB (1200KB) or more free.  
![uleMCSize](readmeImages/uleMCSize.jpg)  
Open MISC -> PS2Net  
![launchelf-ps2net](readmeImages/launchelf-ps2net.jpg)  
Let the PS2 idle on this screen for the next steps on the PC.  
![launchelf-ftp-enabled](readmeImages/launchelf-ftp-enabled.jpg)


### Windows Setup:

**If you plan to use virtual memory cards please read this:**  
VMCs only work with exFAT partitions.  
In the SNL-UDPBDG Sync window, check the VMC checkbox then resync the game list.  
You must use udpbd-server or udpfs_bd when using VMCs, udpbd-vexfat and udpfs do NOT support VMCs.  

Start the SNL-UDPBDG sync/server app (UDPBDG.exe).  
(optional) Click mount exFAT drive for VMC support.  
Rip/copy any PlayStation 2 disc images you wish to load into a DVD or CD folder.  
Click on choose game path and pick any game in a CD or DVD folder.  
Example:  `E:/DVD/Grand Theft Auto III.iso`  
or        `C:\Users\%USERNAME%\Documents\PS2\DVD\TimeSplitters 2.iso`  

Choose the game path and click Sync to SNL.  
![mainWindow](readmeImages/udpbdg-settings.jpg)  
Type in the IP address and connect to launchELF's PS2Net.  
Then click Install to one of the PS2's memory card (*1.2MB FREE SPACE REQUIRED*).  
Wait for the install to complete, this is what a successful install will show:  
![installed](readmeImages/udpbdg-install.jpg)  
Then click Sync game list.  
![synced](readmeImages/udpbdg-sync.jpg)  
After the sync completes, close the sync window.  
![start-server](readmeImages/udpbdg-start.jpg)  
Click Start Server and make sure to allow in windows firewall.  
![udpbdg-firewall](readmeImages/udpbdg-firewall.jpg)  
This will start the UDPBDG notification tray icon that runs the server.  
If you mistakenly clicked cancel on the firewall prompt, close UDPBDG by clicking Exit on the tray icon.  
Then either move the UDPBDG folder inside a new folder or manually delete the inbound rules for UDPBDG in 'Windows Defender Firewall with Advanced Security' to make the prompt reappear.  

Click the tray icon to show/hide console output, open the settings/sync window, or shutdown the server.  
The server needs to be open and running for the entire play session (Disable sleep on the PC).  

Now you are ready to play, run Enceladus or reboot the PS2.  
![snl](readmeImages/snl.jpg)  

If you want the server to start automatically when the PC is turned on follow these steps:

- Right Click `UDPBDG.exe` → Show more → Create Shortcut
- Move the UDPBDG shortcut to this folder:  
`C:\Users\%USERNAME%\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup`  
Now the server will start automatically.  


### Linux Setup:

This guide uses ubuntu, if you are using a different distribution these steps may vary.  
Compile the udpbd-server  
```
sudo apt update
sudo apt upgrade
sudo apt install git build-essential
git clone https://gitlab.com/ps2max/udpbd-server.git
cd udpbd-server
make
```
Install .NET 10, GParted, and exfatprogs  
```
sudo apt install dotnet-runtime-10.0 gparted exfatprogs
```
Create a new exFAT partition in GParted and note the partition number.  
For this guide the exFAT partition is `/dev/nvme0n1p6`, it will be different on your system.  
![gparted-exfat](readmeImages/gparted-exfat.jpg)
Mount it to `/mnt/ps2`. Your storage device `/dev/nvme0n1p6` will probably be different.
```
sudo mkdir /mnt/ps2/
sudo mount /dev/nvme0n1p6 /mnt/ps2/ -o uid=$USER
```
Create folders named `CD` and `DVD` in the exFAT partition.  
Rip/copy any PlayStation 2 disc images you wish to load into the folder that corresponds with their original source media.  
Example:  `/mnt/ps2/DVD/Grand Theft Auto III.iso`  

Install Enceladus and SimpleNeutrinoLoader.  
The target install device must have 1.2MB or more free space.  
Please note the IP address settings and change the following commands accordingly.  
`dotnet SNL-CLI.dll -install mc0 -ps2ip 192.168.0.10 -boot`  
-install '?' will install Enceladus, neutrino, and SNL to the specified device (mc0, mc1, or mass).  
-ps2ip '?' is the ip address for connecting to the PS2 with PS2Net.  
-boot will modify basic-boot-loader to auto-run SNL.  

Next sync the game list, note that 'enablevmc' is optional.  
`dotnet SNL-CLI.dll -path '/mnt/ps2' -ps2ip 192.168.0.10 -bin2iso -enablevmc`  
-path '?' is the file path to the CD and DVD folder that contain game ISOs.  
-bin2iso enables automatic CD-ROM Bin to ISO conversion.  
-enablevmc will assign a virtual memory card for each game or group of games in 'vmc_groups.list'.  

Unmount the exFAT partition then Start the udpbd-server.  
```
sudo umount /mnt/ps2
sudo ./udpbd-server /dev/nvme0n1p6
```
The server needs to be open and running for the entire play session.  
Now the setup is complete, run mc0:/Enceladus/enceladus_pkd.elf and Play!  

To add or rename/remove games, stop the server then mount the exFAT storage device to `/mnt/ps2`  
```
sudo mount /dev/nvme0n1p6 /mnt/ps2/ -o uid=$USER
```


## HDD, HDL, ILINK, MMCE, MX4, or USB Setup:

Download the latest release of SimpleNeutrinoLoader (SNL-Full) from [here](https://github.com/MegaBitmap/PS2-SNL/releases).  
Extract then copy the Enceladus and SimpleNeutrinoLoader folders onto a PS2 memory card with LaunchELF.  
If using psuInstall, copy the psu files then psuPaste onto the memory card.  
The Enceladus and SimpleNeutrinoLoader folder should be placed in the root directory.  
 
In the root directory of the storage device, create CD and DVD folders.  
Rip/copy any PlayStation 2 ISOs you wish to play into the DVD or CD folder.  
Bin+Cue files must be converted to ISO.  
Please view the README from [XEB+ neutrino loader](https://github.com/sync-on-luma/xebplus-neutrino-loader-plugin) for info specific to the type of storage device you are using.  

Run SNL-ListBuilder.py with python to create a game list specific to your device.  
Alternatively an XEB+neutrino list can be converted to SNL with ListConvertXEBtoSNL.py  
Copy the list file (example: HDDList.txt) into the SimpleNeutrinoLoader folder on the PS2.  
Connect the game storage device to the PS2.  
Edit the configuration for PS2BBL:  
The text editor built into LaunchELF can do this.  
Replace the line LK_AUTO_E1 with this:  
`LK_AUTO_E1 = mc?:/Enceladus/enceladus_pkd.elf`  
or  
`LK_AUTO_E1 = mass:/Enceladus/enceladus_pkd.elf`  
This will set PS2BBL to auto-run SNL.  
LaunchELF can be accessed by holding R1 during PS2BBL startup.  


## SNL List File Editing

The following list files are checked on SNL startup:  
HDDList.txt, HDLList.txt, ILINKList.txt, MMCEList.txt, MX4List.txt, UDPBDList.txt, and USBList.txt.  
Here is an example of the list file contents:  
```
Katamari Damacy|SLUS_210.08|-bsd=udpbd|-dvd=udpbd:/DVD/Katamari Damacy.iso
LEGO Star Wars 2|SLUS_214.09|-bsd=udpbd|-dvd=udpbd:/DVD/LEGO Star Wars 2.iso
Midway Arcade Treasures 3|SLUS_210.94|-bsd=udpbd|-dvd=udpbd:/DVD/Midway Arcade Treasures 3.iso
```
Each line uses `|` as a separator for different parts.  
The first part is the name that will be visible on the SNL interface.  
The second part is the serial game ID found in SYSTEM.CNF.  
All further parts are command line arguments that will tell neutrino what settings to use for the selected game.  
There can be a variable number of parts, for example:  
`Katamari Damacy|SLUS_210.08|-bsd=udpbd|-dvd=udpbd:/DVD/Katamari Damacy.iso|-gc=23`  
The last part `-gc=23`, enables compatibility modes 2 and 3 when playing Katamari.  

Here is a list of all neutrino arguments:  

```
-bsd=<driver>     Backing store drivers, supported are:
                - no        (uses cdvd, default)
                - ata       (block device)
                - usb       (block device)
                - mx4sio    (block device)
                - udpbd     (block device)
                - udpfs     (file system)
                - ilink     (block device)
                - mmce      (file system)
                  The following two require a hdd connected to a real sony network hdd adapter:
                - ata-net   (games are loaded from hdd but with in game Online/LAN networking support)
                - udpbd-hdd (games are loaded from udpbd but with in game hdd support)

-bsdfs=<driver>   Backing store filesystem drivers used for block device, supported are:
                - exfat (default)
                - hdl   (HD Loader)
                - bd    (Block Device)
                NOTE: Used only for block devices (see -bsd)

-dvd=<mode>       DVD emulation mode, supported are:
                - no (default)
                - esr
                - <file>

-ata0=<mode>      ATA HDD 0 emulation mode, supported are:
                - no (default)
                - <file>
                NOTE: only both emulated, or both real.
                        mixing not possible
-ata0id=<mode>    ATA 0 HDD ID emulation mode, supported are:
                - no (default)
                - <file>
                NOTE: only supported if ata0 is present
-ata1=<mode>      See -ata0=<mode>

-mc0=<mode>       MC0 emulation mode, supported are:
                - no (default)
                - <file>
-mc1=<mode>       See -mc0=<mode>

-elf=<file>       ELF file to boot, supported are:
                - auto (elf file from cd/dvd) (default)
                - <file>

-gc=<compat>      Game compatibility modes, supported are:
                - 0: IOP: Fast reads (sceCdRead)
                - 1: dummy
                - 2: IOP: Sync reads (sceCdRead)
                - 3: EE : Unhook syscalls
                - 5: IOP: Emulate DVD-DL
                - 7: IOP: Fix game buffer overrun
                Multiple options possible, for example -gc=23

-gsm=v:c          GS video mode
                Parameter v = Force video mode to:
                -         : don't force (default)  (480i/576i)
                - fp1     : force 240p/288p - auto PAL/NTSC
                - fp2     : force 480p/576p - auto PAL/NTSC
                - 1080ix1 : force 1080i width x1, height x1 (very small!)
                - 1080ix2 : force 1080i width x2, height x2
                - 1080ix3 : force 1080i width x3, height x3

                Parameter c = Compatibility mode:
                -      : no compatibility mode (default)
                - 1    : field flipping type 1 (GSM/OPL)
                - 2    : field flipping type 2
                - 3    : field flipping type 3

                Examples:
                -gsm=fp2      - recommended mode
                -gsm=fp2:1    - recommended mode, with compatibility 1
                -gsm=1080ix2

-cwd=<path>       Change working directory
-cfg=<file>       Load extra user/game specific config file (without .toml extension)
-dbc              Enable debug colors
-logo             Enable logo (adds rom0:PS2LOGO to arguments)
-qb               Quick-Boot directly into load environment
--b               Break, all following parameters are passed to the ELF
```


## Big Thanks to these Developers!  

Alex Parrado & Matías Israelson & Rick Gaiser - udpbd-server  
https://github.com/israpps/udpbd-server  

awaken1ng - udpbd-vexfat  
https://github.com/awaken1ng/udpbd-vexfat  

Damián Parrino - ps2vmc-tool  
https://github.com/bucanero/ps2vmc-tool  

Daniel Santos - Enceladus  
https://github.com/DanielSant0s/Enceladus  

Matías Israelson - PS2-Basic-Bootloader  
https://github.com/israpps/PlayStation2-Basic-BootLoader  

PS2Dev Team - ps2client  
https://github.com/ps2dev/ps2client  

Rick Gaiser - neutrino  
https://github.com/rickgaiser/neutrino  

sync-on-luma - XEB+ neutrino Launcher Plugin  
https://github.com/sync-on-luma/xebplus-neutrino-loader-plugin  

