import re
import os

backingDevice = ''
backingFileSystem = ''
backingPrefix = 'mass:'
FinalList = ''

userInput = input("Please enter the full name of the file you want to convert (example: neutrinoHDD.list):")
if not os.path.isfile(userInput):
    print("Unable to find the file: " + userInput)
    print('Press Enter to Exit:')
    input()
    os._exit(0)

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


with open(userInput, 'r') as file:
    for line in file:
        lineSplit = line.strip().split(' ', 1)
        tempGameID = lineSplit[0]
        
        if len(lineSplit) > 1:
            lineSplit = re.split('00000000000', line.strip())
            lineSplit = re.split('/VMC/', lineSplit[0], flags=re.IGNORECASE)
            tempGameFilePath = lineSplit[0].replace(tempGameID, '').strip()

            if ListType == 'HDL':
                lineSplit = tempGameFilePath.split('/')
                tempGameFilePath = lineSplit[2]

            lineSplit = re.split('.iso', line.strip(), flags=re.IGNORECASE)
            lineSplit = lineSplit[0].split('/')
            tempFriendlyName = lineSplit[2]
            lineSplit = re.split('/VMC/', line.strip())
            tempVMC = ''
            if len(lineSplit) > 1:
                tempVMC = '|-mc0=' + backingPrefix + lineSplit[1]

            FinalList += tempFriendlyName + '|' + tempGameID + backingDevice + backingFileSystem + '|-dvd=' + backingPrefix + tempGameFilePath + tempVMC + '\n'

if len(FinalList) > 11:
    with open(ListType + 'List.txt', 'w', encoding="utf-8") as OutList:
        OutList.write(FinalList)
    print('The list of games has been converted to SNL format and saved to ' + ListType + 'List.txt')
    print('Copy ' + ListType + 'List.txt to the SimpleNeutrinoLoader folder in mc0, mc1, or mass.')
    print('Press Enter to Exit:')
    input()

else:
    print('Failed to find any games.')
    print('Press Enter to Exit:')
    input()

