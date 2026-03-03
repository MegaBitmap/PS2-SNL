#!/usr/bin/env python3

import os

ISO_SECTOR_SIZE = 2048

global list_content

def get_game_id(iso_full_path):
    print("Loading " + iso_full_path)
    if os.path.getsize(iso_full_path) % ISO_SECTOR_SIZE != 0 or os.path.getsize(iso_full_path) == 0:
        print("invalid sector size")
        return ""
    with open(iso_full_path, "rb") as iso:
        iso.seek(0x8000) # volume descriptors start at sector 0x10
        if iso.read(7) != b"\x01CD001\x01":
            print("unable to find primary volume descriptor")
            return ""
        iso.seek(0x80A2) # Location of extent (LBA) in big-endian format https://wiki.osdev.org/ISO_9660
        iso_root_table_sector = int.from_bytes(iso.read(4), "big")
        iso.seek(0x80AA) # Data length (size of extent) in big-endian format
        iso_root_table_length = int.from_bytes(iso.read(4), "big")
        iso_table_end = iso_root_table_sector * ISO_SECTOR_SIZE + iso_root_table_length
        iso.seek(iso_root_table_sector * ISO_SECTOR_SIZE)
        while True:
            table_entry_length = int.from_bytes(iso.read(1))
            if table_entry_length < 16 or iso.tell() + 16 > iso_table_end:
                print("failed to find SYSTEM.CNF")
                return ""
            table_entry = iso.read(table_entry_length - 1)
            if table_entry[32 : 42] != b"SYSTEM.CNF":
                continue
            config_sector = int.from_bytes(table_entry[5 : 9], "big")
            config_length = int.from_bytes(table_entry[13 : 17], "big")
            iso.seek(config_sector * ISO_SECTOR_SIZE)
            confing_content = iso.read(config_length).decode("utf-8")
            if not "BOOT2" in confing_content:
                print("missing BOOT2 in SYSTEM.CNF")
                return ""
            game_id = ""
            for line in confing_content.split("\n"):
                if not "BOOT2" in line:
                    continue
                game_id = line.split(";")[0].split("cdrom0")[1].replace(":", "").replace("\\", "")
                if len(game_id) != 11 or not "_" in game_id or not "." in game_id:
                    print(f"failed to parse game id from {line}")
                    return ""
            return game_id

def process_iso(folder, game_path, create_vmc, backing_device, file_system, path_prefix):
    global list_content
    for iso in os.listdir(game_path + folder):
        if not iso.lower().endswith(".iso"):
            continue
        game_id = get_game_id(game_path + folder + "/" + iso)
        if len(game_id) != 11:
            continue
        print(f"Game ID = {game_id}")
        vmc_list_arg = ""
        if create_vmc:
            vmc_path = f"/VMC/{game_id}_0.bin"
            size = "8"
            with open("vmc_groups.list", "r") as f:
                lines = f.readlines()
            for line in lines:
                line = line.strip()
                if line[:4] == "XEBP":
                    size = "8"
                    group = line
                elif len(line) < 5:
                    size = line[:2]
                elif line == game_id:
                    vmc_path = f"/VMC/{group}_0.bin"
                    break
            vmc_list_arg = f"|-mc0={path_prefix + vmc_path}"
            if not os.path.isfile(game_path + vmc_path):
                print(f"Creating VMC {vmc_path} ({size}MB)")
                os.copyfile(f".vmc/{size}.bin", game_path + vmc_path)
            print(f"Assigned {game_id} to {vmc_path}")
            if not os.path.isfile(game_path + vmc_path):
                vmc_path = ""
                vmc_list_arg = ""
        friendly_name = iso.replace(".iso", "").replace(".ISO", "")
        list_content += f"{friendly_name}|{game_id + backing_device + file_system}|-dvd={path_prefix + folder}/{iso + vmc_list_arg}\n"

print("\nThis program will create the list of installed PS2 games for use with SimpleNeutrinoLoader.\n")
print("Please type and enter what stoarage device you are using:")
print("HDD, ILINK, MMCE, MX4, or USB")

backing_device = ""
file_system = ""
path_prefix = "mass:"

read_line = input().upper()
if "HDD" in read_line:
    list_type = "HDD"
    backing_device = "|-bsd=ata"
elif "ILINK" in read_line:
    list_type = "ILINK"
    backing_device = "|-bsd=ilink"
elif "MMCE" in read_line:
    list_type = "MMCE"
    backing_device = "|-bsd=mmce"
    path_prefix = "mmce:"
elif "MX4" in read_line:
    list_type = "MX4"
    backing_device = "|-bsd=mx4sio"
elif "UDPBD" in read_line:
    list_type = "UDPBD"
    backing_device = "|-bsd=udpbd"
elif "USB" in read_line:
    list_type = "USB"
    backing_device = "|-bsd=usb"
else:
    print("Unable to determine list type.")
    print("Press Enter to Exit:")
    input()
    exit(-1) 

print("Please type and enter the root of the storage device:")
print("Example: 'D:/'")
game_path = input()

if not os.path.isdir(game_path):
    print(f"\nERROR: The following path does not exist {game_path}")
    print("Press Enter to Exit:")
    input()
    exit(-1)

print("Do you want to assign a virtual memory card for each game or group of games in vmc_groups.list?")
print("Type Y for yes, N for no")

read_line = input().upper()
if "Y" in read_line:
    create_vmc = True
else:
    create_vmc = False

if create_vmc and not os.path.isfile(".vmc/8.bin"):
    print("Warning: VMC.bin not found. VMCs will not be created.")
    create_vmc = False

if create_vmc and not os.path.isfile("vmc_groups.list"):
    print("Warning: vmc_groups.list not found. VMCs will not be enabled.")
    create_vmc = False

if not os.path.isdir(game_path + "/CD"):
    print(f"CD folder not found at {game_path}")

if not os.path.isdir(game_path + "/DVD"):
    print(f"DVD folder not found at {game_path}")

if create_vmc and not os.path.isdir(game_path + "/VMC"):
    os.mkdir(game_path + "/VMC")

list_content = ""
if os.path.isdir(game_path + "/DVD"):
    process_iso("/DVD", game_path, create_vmc, backing_device, file_system, path_prefix)

if os.path.isdir(game_path + "/CD"):
    process_iso("/CD", game_path, create_vmc, backing_device, file_system, path_prefix)

if len(list_content) < 11:
    print("Failed to find any games")
    print("Press Enter to Exit:")
    input()
    exit(-1)

if os.path.isfile(list_type + "List.txt"):
    os.remove(list_type + "List.txt")

with open(list_type + "List.txt", "w", encoding="utf-8") as list_file:
    list_file.write(list_content)
print(f"\nThe list of games in SNL format was saved to {list_type}List.txt")
print(f"Copy {list_type}List.txt to the SimpleNeutrinoLoader folder in mc0, mc1, or mass")

print("Press Enter to Exit:")
input()

