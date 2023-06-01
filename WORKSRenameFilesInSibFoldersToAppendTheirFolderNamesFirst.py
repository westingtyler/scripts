# made by westingtyler with ChatGPT's help on 2023.06.01 at 10:11 AM.
# https://github.com/westingtyler/scripts/tree/main
# https://www.youtube.com/channel/UCdJHTLc_M3CIssLVmykWn_w
# I hope to add a description HERE soon for what this script does, and hopefully a link to a youtube tutorial I will have made, demonstrating the script's proper use.

import os

def rename_files_in_folder(folder_path):
    folder_name = os.path.basename(folder_path)
    for root, _, files in os.walk(folder_path):
        for file_name in files:
            original_file_path = os.path.join(root, file_name)
            new_file_name = f"{folder_name} {file_name}"
            new_file_path = os.path.join(root, new_file_name)
            os.rename(original_file_path, new_file_path)
            print(f"Renamed '{file_name}' to '{new_file_name}'")

current_folder = os.path.dirname(os.path.abspath(__file__))
for folder in os.listdir(current_folder):
    folder_path = os.path.join(current_folder, folder)
    if os.path.isdir(folder_path):
        rename_files_in_folder(folder_path)
