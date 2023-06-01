#made by westingtyler with ChatGPT's help on 2023.04.11 at 5:47 PM.
# https://github.com/westingtyler/scripts/tree/main
# https://www.youtube.com/channel/UCdJHTLc_M3CIssLVmykWn_w
# I hope to add a description HERE soon for what this script does, and hopefully a link to a youtube tutorial I will have made, demonstrating the script's proper use.

import glob
def main():
    # Get a list of all the text files in the current directory
    file_list = glob.glob("*.txt")

    # Open a new file to write the merged content to
    with open("merged.txt", "w") as merged_file:
        # Loop through each file in the list
        for file_name in file_list:
            # Open the file and read its contents
            with open(file_name, "r") as current_file:
                file_content = current_file.read()
            # Write the file's contents to the merged file
            merged_file.write(file_content)

if __name__ == "__main__":
    main()
