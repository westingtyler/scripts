import os
#made by westingtyler with ChatGPT's help circa 2023.05.01
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
