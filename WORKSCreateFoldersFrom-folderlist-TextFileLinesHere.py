# made by westingtyler with ChatGPT's help on 2023.02.20 at 10:30AM.
# https://github.com/westingtyler/scripts/tree/main
# https://www.youtube.com/channel/UCdJHTLc_M3CIssLVmykWn_w
# I hope to add a description HERE soon for what this script does, and hopefully a link to a youtube tutorial I will have made, demonstrating the script's proper use.

import os

# Get the current directory where the script is running
current_dir = os.getcwd()

# Find the first .txt file in the directory
text_file = None
for filename in os.listdir(current_dir):
    if filename.endswith(".txt"):
        text_file = filename
        break

# If no text file is found, exit the script
if not text_file:
    print("No text file found in the directory")
    exit()

# Read the text file and create a folder for each line
with open(text_file) as f:
    lines = f.readlines()
    for line in lines:
        # Remove any trailing whitespace or newlines from the line
        line = line.strip()

        # Remove any trailing "." character from the line
        while line.endswith("."):
            line = line[:-1]

        # Try to create the folder for the line
        try:
            os.mkdir(os.path.join(current_dir, line))
        except Exception as e:
            print(f"Could not create folder for '{line}': {e}")

# Print a success message
print("Folders created successfully.")
