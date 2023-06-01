# made by westingtyler with ChatGPT's help on 2023.02.19 at 12:28PM.
# https://github.com/westingtyler/scripts/tree/main
# https://www.youtube.com/channel/UCdJHTLc_M3CIssLVmykWn_w
# I hope to add a description HERE soon for what this script does, and hopefully a link to a youtube tutorial I will have made, demonstrating the script's proper use.
# THIS WORKS PERFECTLY. ensure it is in the folder of the broken links. and it will do a full search of its great grandparent folder for the file it is looking for. and then update the shortcut to point to the correct file.

import os
import winshell
import win32con
import comtypes
#import comtypes.client
#import comtypes.shelllink
#those last two may be necessary. worked before I commented them as chatgpt said I could do. if this doesn't work in the future, uncomment them and try again.


# Define a function to repair a single shortcut
def repair_shortcut(shortcut):
    # Create a winshell.Shortcut object from the comtypes.client.Dispatch object
    # Get the path to the file that the shortcut points to
    target_path = shortcut.path
    # print the current target path
    # print(target_path)

    # If the file exists, return without doing anything
    if os.path.exists(target_path):
        return
        # print the fact that the file exists
        print('file already exists where the shortcut says it should.')

    # Get the filename of the shortcut and remove the " - Shortcut.lnk" part
    filename = os.path.splitext(os.path.basename(target_path))[0]
    filename = filename.replace(" - Shortcut", "")
    # append .png to the filename.
    # filename = filename + ".png"
    # print the filename to check it
    print("filename is: " + filename)

    # Get the path to the grandparent folder
    grandparent_path = os.path.dirname(os.path.dirname(os.path.dirname(shortcut.lnk_filepath)))

    # Search for the PNG file in the grandparent folder and its subdirectories
    for root, dirs, files in os.walk(grandparent_path):
        png_file = os.path.join(root, filename + ".png")
        if os.path.exists(png_file):
            print('file exists')
            # If it does, update the shortcut target to the new path
            shortcut.path = png_file
            shortcut.working_directory = grandparent_path
            # print what is about to happen.
            print('shortcut will be updated to: ' + png_file)
            shortcut.write()
            # Save the updated shortcut
            print('shortcut updated')
            return

    print('file does not exist so shortcut could not be updated.')


if __name__ == '__main__':
    # Get the directory where the script is located
    script_dir = os.path.dirname(os.path.abspath(__file__))
    print('starting the process')

    # Get all .lnk files in the script directory
    lnk_files = [f for f in os.listdir(script_dir) if f.endswith('.lnk')]

    # Import comtypes.gen.Shell
    # from comtypes.gen.Shell import *

    # Repair each shortcut in turn
    for lnk_file in lnk_files:
        # print the name of the current shortcut
        print(lnk_file)
        shortcut = winshell.shortcut(os.path.join(script_dir, lnk_file))
        repair_shortcut(shortcut)
