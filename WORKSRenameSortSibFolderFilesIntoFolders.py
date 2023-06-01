# made by westingtyler with ChatGPT's help on 2023.06.01 at 10:23 AM.
# https://github.com/westingtyler/scripts/tree/main
# https://www.youtube.com/channel/UCdJHTLc_M3CIssLVmykWn_w
# I used this on my folder full of food SD generations that did not have their names at the start of their filenames. drop into the folder with the pngtxt pairs, ensure the words are specified below that you want to get the name after (if you only want it to return one word, put a period in the prompt after it.), and when clicked it will rename them and make new folders if needed to sort them into. this means you need to ensure your prompts ALL have a string after which the name of the item is typed. in this case it was "delicious savory". you might need to ensure it's two words, not one or three.
#I hope to add a link to a youtube tutorial I will have made, demonstrating the script's proper use.

import os
import shutil

def process_file_pair(png_file, txt_file):
    # Read the contents of the text file
    with open(txt_file, 'r') as f:
        txt_contents = f.read()
    # Find the index of "delicious savory" in the text !!!!CHANGE THIS LINE AS NEEDED(might require exactly two words.)!!!!
    index = txt_contents.find("delicious savory")
    if index != -1:
        # Extract the two words after "delicious savory"
        words = txt_contents[index:].split()[2:4]
        new_name = ' '.join(words)
        if '.' in new_name:
            new_name = new_name.split('.')[0]  # Trim off period and characters after it
        if words[1] == "in":
            new_name = words[0]  # Truncate the second word

    else:
        # Use the first word of the text file contents as the new name
        new_name = txt_contents.split()[0]

    # Check if the file pair already starts with STRINGA
    filename = os.path.basename(png_file)
    txt_filename = os.path.basename(txt_file)
    dirname = os.path.dirname(png_file)
    if new_name in filename or new_name in txt_filename or new_name in dirname:
        print(f"Skipping: {png_file}. Already processed.")
        return

    # Rename the files by appending STRINGA to the beginning
    new_png_file = new_name + ' ' + os.path.basename(png_file)
    new_txt_file = new_name + ' ' + os.path.basename(txt_file)

    # Move the files to the appropriate subfolder
    folder_path = os.path.dirname(png_file)
    subfolder_path = os.path.join(folder_path, new_name)
    os.makedirs(subfolder_path, exist_ok=True)
    shutil.move(png_file, os.path.join(subfolder_path, new_png_file))
    shutil.move(txt_file, os.path.join(subfolder_path, new_txt_file))

def process_folder(folder):
    for root, dirs, files in os.walk(folder):
        for file in files:
            if file.endswith('.png'):
                png_file = os.path.join(root, file)
                txt_file = os.path.join(root, file.rsplit('.', 1)[0] + '.txt')
                if os.path.isfile(txt_file):
                    process_file_pair(png_file, txt_file)
                else:
                    print(f"Warning: Text file missing for {png_file}. Skipping pair.")

# Run the script in the current directory
current_directory = os.getcwd()
process_folder(current_directory)
