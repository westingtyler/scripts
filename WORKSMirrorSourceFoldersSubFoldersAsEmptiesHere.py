# made by westingtyler with ChatGPT's help on 2023.02.13 at 9:16 AM.
# https://github.com/westingtyler/scripts/tree/main
# https://www.youtube.com/channel/UCdJHTLc_M3CIssLVmykWn_w
# I hope to add a description HERE soon for what this script does, and hopefully a link to a youtube tutorial I will have made, demonstrating the script's proper use.
# this takes a target directory and mirrors its folder set into the folder of this python script, just creating empty folders that match that set. this is good if you want a second set of folders to match the names of those folders but don't just want to copy the contents over.

import os
def mirror_subfolders_as_empties(source_parent_folder_to_emptyfoldermirror):
    if not os.path.exists(source_parent_folder_to_emptyfoldermirror):
        print("The target folder does not exist.")
        return

    folder_of_this_python_script = os.path.dirname(os.path.abspath(__file__))
    print("Creating subfolders in", folder_of_this_python_script)
    for extant_subfolder in os.listdir(source_parent_folder_to_emptyfoldermirror):
        print("Processing extant_subfolder", extant_subfolder)
        full_extant_subfolder_path = os.path.join(source_parent_folder_to_emptyfoldermirror, extant_subfolder)
        if os.path.isdir(full_extant_subfolder_path):
            new_empty_subfolder_to_create = os.path.join(folder_of_this_python_script, extant_subfolder)
            if not os.path.exists(new_empty_subfolder_to_create):
                try:
                    os.makedirs(new_empty_subfolder_to_create)
                    print("Successfully created empty mirrored subfolder", new_empty_subfolder_to_create)
                except OSError as e:
                    print("Failed to create empty mirrored subfolder", new_empty_subfolder_to_create)
                    print("Error:", e)



if __name__ == '__main__':
    #!!!!!REPLACE THE EXAMPLE PATH BELOW AS NEEDED.!!!!
    source_parent_folder_to_emptyfoldermirror = r"C:\Users\USERNAME\Desktop\foldername\food potion candy\ms food\refset"
    mirror_subfolders_as_empties(source_parent_folder_to_emptyfoldermirror)

