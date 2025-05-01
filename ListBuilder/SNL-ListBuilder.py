import math
import os
import shutil
import re

done = "Error: No games found."
total = 0
count = 0
pattern_1 = [b'\x01', b'\x0D']
pattern_2 = [b'\x3B', b'\x31']

def count_iso(folder, game_path):
    global total
    for image in os.listdir(game_path + folder):
        if image.lower().endswith(".iso"):
            total += 1

def process_iso(folder, game_path, game_list, create_vmc, backingDevice, backingFileSystem, backingPrefix):
    global total, count, done

    for image in os.listdir(game_path + folder):
        if image.lower().endswith(".iso"):
            print(f"{math.floor((count * 100) / total)}% complete")
            print(f"Processing {image}")
            index = 0
            string = ""
            with open(game_path + folder + "/" + image, "rb") as file:
                while (byte := file.read(1)):
                    if len(string) < 4:
                        if index == 2:
                            string += byte.decode('utf-8', errors='ignore')
                        elif byte == pattern_1[index]:
                            index += 1
                        else:
                            string = ""
                            index = 0
                    elif len(string) == 4:
                        index = 0
                        if byte == b'\x5F':
                            string += byte.decode('utf-8', errors='ignore')
                        else:
                            string = ""
                    elif len(string) < 8:
                        string += byte.decode('utf-8', errors='ignore')
                    elif len(string) == 8:
                        if byte == b'\x2E':
                            string += byte.decode('utf-8', errors='ignore')
                        else:
                            string = ""
                    elif len(string) < 11:
                        string += byte.decode('utf-8', errors='ignore')
                    elif len(string) == 11:
                        if byte == pattern_2[index]:
                            index += 1
                            if index == 2:
                                break
                        else:
                            string = ""
                            index = 0

                count += 1

            if len(string) == 11:
                print(f"Found title ID {string}")
                tempVMC = ''
                if create_vmc:
                    vmc = f"/VMC/{string}_0.bin"
                    with open("vmc_groups.list", "r") as f:
                        lines = f.readlines()

                    for line in lines:
                        line = line.strip()
                        if line[:4] == "XEBP":
                            size = "8"
                            group = line
                        elif len(line) < 5:
                            size = line[:2]
                        elif line == string:
                            vmc = f"/VMC/{group}_0.bin"
                            break

                    tempVMC = '|-mc0=' + backingPrefix + vmc
                    if create_vmc:
                        if vmc != f"/VMC/{string}_0.bin" and not os.path.isfile(game_path + f"/VMC/{string}_0.bin"):
                            print(f"Creating VMC /VMC/{string}_0.bin (8MB)")
                            shutil.copyfile(".vmc/8.bin", game_path + f"/VMC/{string}_0.bin")

                        if not os.path.isfile(game_path + vmc):
                            print(f"Creating VMC {vmc} ({size}MB)")
                            shutil.copyfile(f".vmc/{size}.bin", game_path + vmc)
                            
                        print(f"Assigned {string} to {vmc}")
                    elif not os.path.isfile(game_path + vmc):
                        vmc = ''
                        tempVMC = ''
                
                lineSplit = re.split('.iso', image, flags=re.IGNORECASE)
                tempFriendlyName = lineSplit[0]
                with open(game_list + 'List.txt', 'a') as output:
                    output.write(tempFriendlyName + '|' + string + backingDevice + backingFileSystem + '|-dvd=' + backingPrefix + folder + '/' + image + tempVMC + '\n')

    done = '\nThe list of games has been converted to SNL format and saved to ' + game_list + 'List.txt\n\n'
    done += 'Copy ' + game_list + 'List.txt to the SimpleNeutrinoLoader folder in mc0, mc1, or mass.\n'

def main():
    print('\nThis program will create the list of installed PS2 games for use with SimpleNeutrinoLoader.\n')
    print('Please type and enter what stoarage device you are using:')
    print('HDD, ILINK, MMCE, MX4, or USB')
    
    backingDevice = ''
    backingFileSystem = ''
    backingPrefix = 'mass:'

    userInput = input()
    upperInput = userInput.upper()
    if 'HDD' in upperInput:
        ListType = 'HDD'
        backingDevice = '|-bsd=ata'
    elif 'HDL' in upperInput:
        ListType = 'HDL'
        backingDevice = '|-bsd=ata'
        backingFileSystem = '|-bsdfs=hdl'
        backingPrefix = 'hdl:'
    elif 'ILINK' in upperInput:
        ListType = 'ILINK'
        backingDevice = '|-bsd=ilink'
    elif 'MMCE' in upperInput:
        ListType = 'MMCE'
        backingDevice = '|-bsd=mmce'
        backingPrefix = 'mmce:'
    elif 'MX4' in upperInput:
        ListType = 'MX4'
        backingDevice = '|-bsd=mx4sio'
    elif 'UDPBD' in upperInput:
        ListType = 'UDPBD'
        backingDevice = '|-bsd=udpbd'
    elif 'USB' in upperInput:
        ListType = 'USB'
        backingDevice = '|-bsd=usb'
    else:
        print("Unable to determine list type.")
        print('Press Enter to Exit:')
        input()
        os._exit(0) 
    
    print('Please type and enter the root of the storage device:')
    print('Example: "D:/"')
    userInput = input()
    
    if not os.path.isdir(userInput):
        print(f'\nERROR: The following path does not exist {userInput}')
        print('Press Enter to Exit:')
        userInput = input()
        os._exit(0)
    game_path = userInput
    
    print('Do you want to assign a virtual memory card for each game or group of games in vmc_groups.list?')
    print('Type Y for yes, N for no')

    userInput = input()
    upperInput = userInput.upper()
    if 'Y' in upperInput:
        create_vmc = True
    else:
        create_vmc = False

    if os.path.isfile(ListType + 'List.txt'):
        os.remove(ListType + 'List.txt')

    if create_vmc and not os.path.isfile(".vmc/8.bin"):
        print("Warning: VMC.bin not found. VMCs will not be created.")
        create_vmc = False

    if create_vmc and not os.path.isfile('vmc_groups.list'):
        print("Warning: vmc_groups.list not found. VMCs will not be enabled.")
        create_vmc = False

    if os.path.isdir(game_path + '/CD'):
        count_iso('/CD', game_path)
    else:
        print(f'CD folder not found at {game_path}')

    if os.path.isdir(game_path + '/DVD'):
        count_iso('/DVD', game_path)
    else:
        print(f'DVD folder not found at {game_path}')

    if create_vmc and not os.path.isdir(game_path + '/VMC'):
        os.mkdir(game_path + '/VMC')

    if os.path.isdir(game_path + '/DVD'):
        process_iso('/DVD', game_path, ListType, create_vmc, backingDevice, backingFileSystem, backingPrefix)

    if os.path.isdir(game_path + '/CD'):
        process_iso('/CD', game_path, ListType, create_vmc, backingDevice, backingFileSystem, backingPrefix)

    print(done)
    print('Press Enter to Exit:')
    input()
    os._exit(0) 

main()

