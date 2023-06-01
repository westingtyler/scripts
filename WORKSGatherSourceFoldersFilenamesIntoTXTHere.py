# made by westingtyler with ChatGPT's help on 2023.02.13 at 12:27AM.
# https://github.com/westingtyler/scripts/tree/main
# https://www.youtube.com/channel/UCdJHTLc_M3CIssLVmykWn_w
# I hope to add a description HERE soon for what this script does, and hopefully a link to a youtube tutorial I will have made, demonstrating the script's proper use.

import os

def get_filenames_without_extension(folder_path):
    filenames = os.listdir(folder_path)
    return [os.path.splitext(filename)[0] for filename in filenames]

def main():
    root_folder = r"C:\Users\Administrator\Desktop\NotSS Capital Data\food potion\ms food\refset"

    with open("filenames.txt", "w") as file:
        for subfolder, _, filenames in os.walk(root_folder):
            subfolder_name = os.path.basename(subfolder)
            file.write(f"{subfolder_name}\n")
            filenames = get_filenames_without_extension(subfolder)
            file.write("\n".join(filenames))
            file.write("\n\n")

if __name__ == "__main__":
    main()
