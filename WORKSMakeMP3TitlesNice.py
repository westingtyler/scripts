# made by westingtyler with ChatGPT's help on 2023.05.17 at 4:59 PM.
# https://github.com/westingtyler/scripts/tree/main
# https://www.youtube.com/channel/UCdJHTLc_M3CIssLVmykWn_w
# I hope to add a description HERE soon for what this script does, and hopefully a link to a youtube tutorial I will have made, demonstrating the script's proper use.

import os
import re
from mutagen.easyid3 import EasyID3
# Get the current directory
folder_path = os.path.dirname(os.path.abspath(__file__))

# Iterate through all files in the directory
for filename in os.listdir(folder_path):
    # Check if the file has a .mp3 extension
    if filename.endswith(".mp3"):
        # Remove underscores and replace them with spaces
        new_filename = filename.replace("_", " ")

        # Capitalize each word in the filename
        words = new_filename.split()
        capitalized_words = [
            word.capitalize() if word.lower() not in ["mp3", "wm"] and not re.match(r'^(ii|iii|iv|v|vi|vii|viii|ix|x|ad|oc)$', word.lower()) else word.upper()
            
            for word in words
        ]
        # Iterate through capitalized words and correct the ones starting with "("
        capitalized_words = [
            "(" + word[1:].capitalize() if word.startswith("(") else word
            for word in capitalized_words
]
        # Check for "ost.mp3" and update as "OST.mp3"
        capitalized_words = ["OST.mp3" if word.lower() in ["ost.mp3", "Ost.mp3"] else word for word in capitalized_words]
        new_filename = " ".join(capitalized_words)

        # Check for ost .
        capitalized_words = ["OST" if word.lower() in ["ost", "Ost"] else word for word in capitalized_words]
        new_filename = " ".join(capitalized_words)

        # Check if the filename ends in a string of numbers longer than 8 characters
        base_filename, extension = os.path.splitext(new_filename)
        if re.match(r".*\d{9,}$", base_filename):
            # Trim out the word of numbers
            base_filename = re.sub(r"\d{9,}$", "", base_filename).strip()
            new_filename = base_filename + extension

        # Add spaces around hyphens
        new_filename = re.sub(r'(\w+)-(\w+)', r'\1 - \2', new_filename)

        if new_filename.endswith(" -.mp3"):
            new_filename = new_filename[:-6] + extension


        # Check if the filename ends with " -"
        if new_filename.endswith(" -"):
            new_filename = new_filename[:-2] + extension

        # Get the full file path
        file_path = os.path.join(folder_path, filename)



        audio = EasyID3(file_path)
        # Print the keys
        print(audio.keys())
        print("keys are listed above.")



        # Delete the Comment metadata
        
        if 'comment' in audio:
            audio['comment'] = ''
            audio.save()
        

        # Set the Title metadata to match the new filename
        audio['title'] = new_filename
        audio.save()

        # Rename the file
        os.rename(file_path, os.path.join(folder_path, new_filename))
