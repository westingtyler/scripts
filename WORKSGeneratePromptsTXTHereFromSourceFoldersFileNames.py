# made by westingtyler with ChatGPT's help on 2023.02.13 at 12:42 AM.
# https://github.com/westingtyler/scripts/tree/main
# https://www.youtube.com/channel/UCdJHTLc_M3CIssLVmykWn_w
# I hope to add a description HERE soon for what this script does, and hopefully a link to a youtube tutorial I will have made, demonstrating the script's proper use.
# I used this to create 3 prompts each for each food image file found in my reference directory, and add all of them to a big text file here.

import os
import re

def top_down_prompt(folder, file):
    return "a top-down studio of uncropped delicious savory " + file.split(".")[0] + ". photorealistic."

def normal_prompt(folder, file):
    return "a studio of uncropped delicious savory " + file.split(".")[0] + ". photorealistic."

def packaging_prompt(folder, file):
    return "a studio of uncropped delicious savory " + file.split(".")[0] + " in its packaging. photorealistic."

def main():
    SourcePath = 'C:\\Users\\Administrator\\Desktop\\NotSS Capital Data\\food potion candy\\ms food\\refset'
    with open('filenamestoprompts.txt', 'w') as f:
        for root, dirs, files in os.walk(SourcePath): # if SourcePath ends up not working next time this script is used, change both instance back to path
                folder = root.split('\\')[-1].upper()
                for i in range(4):
                    f.write(folder + '\n')
                for file in files:
                    for i in range(4):
                        f.write(top_down_prompt(folder, file) + '\n')
                    for i in range(4):
                        f.write(normal_prompt(folder, file) + '\n')
                    for i in range(4):
                        f.write(packaging_prompt(folder, file) + '\n')

if __name__ == "__main__":
    main()