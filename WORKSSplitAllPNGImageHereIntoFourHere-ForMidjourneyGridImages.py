from PIL import Image
import os
#made by westingtyler with ChatGPT's help circa 2023.05.01
# Get all the .png files in the current directory
png_files = [f for f in os.listdir('.') if f.endswith('.png')]

for file in png_files:

    # Open the image
    image = Image.open(file)
    
    # Get the size of the image
    width, height = image.size
    
    # Split the image into 4 parts
    part_width = width // 2
    part_height = height // 2
    
    parts = [(0, 0, part_width, part_height),
             (part_width, 0, width, part_height),
             (0, part_height, part_width, height),
             (part_width, part_height, width, height)]
    
    # Save each part with a different filename
    for i, part in enumerate(parts):
        filename, extension = os.path.splitext(file)
        new_filename = f"{filename}_{i+1}{extension}"
        image.crop(part).save(new_filename)

    # Rename the original file
    os.rename(file, f"_{file}")

