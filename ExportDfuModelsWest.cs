#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Text;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallConnect.Utility;

using UnityEditor;
using UnityEngine;

using Races = DaggerfallWorkshop.Game.Entity.Races;
using Genders = DaggerfallWorkshop.Game.Entity.Genders;
namespace NexusDev
{
    public class ExportDfuModelsWest : EditorWindow
    {
private const string ExportFolderAssetRelative = "Assets/Exports/DaggerfallAssets";
private const string TempTextureFolderAssetRelative = "Assets/Exports/DaggerfallAssets/__TempTexture";
private const string AtlasFolderAssetRelative = "Assets/Exports/DaggerfallAssets/_TextureAtlas";
private const string MaterialFolderAssetRelative = "Assets/Exports/DaggerfallAssets/_Material";
private const string FbxFolderAssetRelative = "Assets/Exports/DaggerfallAssets/FBX";
private const string FbxModelFolderAssetRelative = "Assets/Exports/DaggerfallAssets/FBX/Model";
private const string FbxFlatFolderAssetRelative = "Assets/Exports/DaggerfallAssets/FBX/Flat";

private const string PrefTxtPath = "NexusDev.ExportDfuModelsWest.TxtPath";
private const string PrefDeleteTemps = "NexusDev.ExportDfuModelsWest.DeleteTemps";
private const string PrefSingleModelTxtPath = "NexusDev.ExportDfuModelsWest.SingleModelTxtPath";
private const string PrefSingleFlatTxtPath = "NexusDev.ExportDfuModelsWest.SingleFlatTxtPath";
private const string PrefSingleModelBatchRangeText = "NexusDev.ExportDfuModelsWest.SingleModelBatchRangeText";
private const string PrefSingleFlatBatchRangeText = "NexusDev.ExportDfuModelsWest.SingleFlatBatchRangeText";
private const string PrefSingleId = "NexusDev.ExportDfuModelsWest.SingleId";
private const string PrefSingleFlatId = "NexusDev.ExportDfuModelsWest.SingleFlatId";
private const string PrefExportScaleMultiplier = "NexusDev.ExportDfuModelsWest.ExportScaleMultiplier";
private const string PrefAtlasBatchExportedMats = "NexusDev.ExportDfuModelsWest.AtlasBatchExportedMats";
private const string PrefBatchAtlasMaxSize = "NexusDev.ExportDfuModelsWest.BatchAtlasMaxSize";
private const string PrefBatchRangeText = "NexusDev.ExportDfuModelsWest.BatchRangeText";
private const string PrefCategoryBatchExpanded = "NexusDev.ExportDfuModelsWest.CategoryBatchExpanded";
private const string PrefCategoryBatchSelected = "NexusDev.ExportDfuModelsWest.CategoryBatchSelected";
private const string PrefFlatCategoryBatchExpanded = "NexusDev.ExportDfuModelsWest.FlatCategoryBatchExpanded";
private const string PrefFlatCategoryBatchSelected = "NexusDev.ExportDfuModelsWest.FlatCategoryBatchSelected";

private const string PrefOverwriteExistingExportAssets = "NexusDev.ExportDfuModelsWest.OverwriteExistingExportAssets";
private const string PrefExportGroupedFamilyMembers = "NexusDev.ExportDfuModelsWest.ExportGroupedFamilyMembers";
private const string PrefCategoryBatchFbxSubfolders = "NexusDev.ExportDfuModelsWest.CategoryBatchFbxSubfolders";
private const string PrefCategoryLastDurationSecondsPrefix = "NexusDev.ExportDfuModelsWest.CategoryLastDurationSeconds.";
private const string PrefFlatCategoryLastDurationSecondsPrefix = "NexusDev.ExportDfuModelsWest.FlatCategoryLastDurationSeconds.";
private const string PrefMobileSpriteSortMode = "NexusDev.ExportDfuModelsWest.MobileSpriteSortMode";


private string txtPath;
private string singleModelTxtPath = string.Empty;
private string singleFlatTxtPath = string.Empty;
private string singleModelBatchRangeText = string.Empty;
private string singleFlatBatchRangeText = string.Empty;
private bool deleteTempTextureFilesAfterExport = true;
private uint singleModelId = 40001;
private string singleFlatIdText = "210.2";

private float exportScaleMultiplier = 1f;
private bool atlasBatchExportedMats = false;
private int batchAtlasMaxSize = 8192;
private string batchRangeText = string.Empty;
private bool categoryBatchExpanded = false;
private List<string> selectedCategoryBatchSources = new List<string>();
private bool flatCategoryBatchExpanded = false;
private List<string> selectedFlatCategoryBatchSources = new List<string>();
private bool overwriteExistingExportAssets = true;
private bool exportGroupedFamilyMembers = false;
private bool categoryBatchFbxSubfolders = false;
private bool infoExpanded = false;
private bool debugToolsExpanded = false;
private int debugDeleteGroupIndex = 0;
private int debugDeleteFlatGroupIndex = 0;
private int topTabIndex = 0;
private bool blenderImportScriptExpanded = false;

private const string BlenderTextEditorImportScriptDescription =
    @"Below is my current Blender-side import/helper script reference for bringing exported assets into Blender from the DFU export pipeline. To use it, copy the entire script text and paste it into a new Text block in Blender's Text Editor. 

Set the two paths toward the top of the Import script, to where your exported assets are. The script supports different import modes for flexibility. If you choose something like ONE_CATEGORY, ensure at least one folder is uncommented or it will make an error in the Blender Console Window.

Then you can run the script to import exported FBX files from the specified export root folder. Blender may look frozen for seconds or even a minute or two depending on how many assets you exported and are importing, but that's normal. Just give it time. If the script has an error it'll show up immediately and abort, and be visible in the Blender Console Window.

After several frozen seconds, depending on how many things you imported, Blender should show you a nicely organized grid array of the assets, ready for modeling or whatever else you wanna do with them.

WARNING: After you get everything into the blend file, it's recommended you pack then unpack the texture images into the actual Blend file so they don't get erased - along with any changes you made - the next time you use the Unity export script and overwrite what's in the folders.

If you're not sure how to use a Blender text editor python script or how to pack and unpack textures in blender, just ask chatGPT. Don't worry, setting these settings in Blender is pretty easy once you ge the hang of it. Learning Blender itself has a learning curve, but just take it one step at a time and you can climb any mountain! That's what I did! Now I'm a pro.... at using ChatGPT to write scripts...and verifying with Gemini and Claude that they are solid and professional and performant.... then doing trial and error testing them for hours... man you have no idea how dumb it was to get a python script embedded into a Unity C sharp editor script then have it also work as python inside Blender..but it IS possible! The key with that was using a verbatim string with double/quadruple quotes - two on each side of string. I think that was it.

Tested with Blender 4.5.7 LTS.";

private const string BlenderTextEditorImportScriptText =
    @"# Blender 4.5.7 LTS
# Text Editor script: ImportDFUExportBatchFBX
#
# Imports DFU-exported FBX files from:
#   <EXPORT_ROOT>\FBX\...
#
# Import modes:
#   - ONE_MODEL       : import one specific FBX file
#   - ONE_CATEGORY    : import one category folder under FBX recursively
#   - FOLDER_LIST     : import a list of category folders under FBX recursively
#   - ALL             : import everything under FBX except excluded prefixes
#
# Expected root structure:
#   <EXPORT_ROOT>\
#       FBX\
#       Material\
#       TextureAtlas\
#
# IMPORTANT:
# Manually verify Blender scale in Blender against in-game / play-mode scale.

import bpy
import os
import math
from pathlib import Path
from mathutils import Vector

# =========================================================
# USER SETTINGS
# =========================================================

EXPORT_ROOT = r""YOUR_DFU_PROJECT_PATH_HERE\daggerfall-unity-1.1.1\Assets\Exports\DaggerfallAssets""

# ---------------------------------------------------------
# IMPORT MODE
# ---------------------------------------------------------
# Valid values:
#   ""ONE_MODEL""
#   ""ONE_CATEGORY""
#   ""FOLDER_LIST""
#   ""ALL""
IMPORT_MODE = ""FOLDER_LIST""

# ONE_MODEL: set this path yourself if needed.
ONE_MODEL_PATH = r""YOUR_DFU_PROJECT_PATH_HERE\Assets\Exports\DaggerfallAssets\FBX\Furniture\Bench_id43307_df.fbx""

# ONE_CATEGORY:
# Relative folder path under FBX.
ONE_CATEGORY_RELATIVE = r""Furniture""

# FOLDER_LIST:
# Relative folder paths under FBX.

FOLDER_LIST_RELATIVE = [
    #r""Flat\Interior"",
    r""Flat\Lights"",
    #r""Flat\Markers"",
    #r""Flat\Nature"",
    #r""Flat\NPCS"",
    #r""Flat\Treasure"",
    #r""Model\Clutter"",
    #r""Model\Dungeon"",
    #r""Model\DungeonPartsCaves"",
    #r""Model\DungeonPartsCorridors"",
    #r""Model\DungeonPartsDoors"",
    #r""Model\DungeonPartsMisc"",
    #r""Model\DungeonPartsRooms"",
    #r""Model\Furniture"",
    #r""Model\Graveyard"",
    #r""Model\HouseParts"",
    #r""Model\Ships"",
    #r""Model\Structure"",
]


# ALL:
# Exclude FBX subfolders whose relative path under FBX starts with any of these prefixes.
EXCLUDED_FBX_SUBFOLDER_PREFIXES = [
    ""Dungeon"",
]

# ---------------------------------------------------------
# SCALE / LAYOUT
# ---------------------------------------------------------

# Global import multiplier to bake into imported mesh data.
IMPORT_SCALE_MULTIPLIER = 1.0

CATEGORY_GAP_X = 12.0
CATEGORY_GAP_Y = 12.0
OBJECT_CELL_SIZE = 2.0

SORT_SMALLEST_TO_LARGEST = True

ENABLE_BACKFACE_CULLING = True
TEXTURE_INTERPOLATION = 'Closest'   # 'Closest' or 'Linear'
USE_SHADE_FLAT = True

PROCESS_MESH_OBJECTS_ONLY = True

# =========================================================
# HELPERS
# =========================================================

def require_saved_blend():
    if not bpy.data.filepath:
        raise RuntimeError(""Save the .blend file first."")
    return os.path.dirname(bpy.data.filepath)

def ensure_folder_exists(path: str, label: str):
    if not os.path.isdir(path):
        raise RuntimeError(f""{label} folder does not exist:\n{path}"")

def get_expected_folders(export_root: str):
    fbx_root = os.path.join(export_root, ""FBX"")
    material_root = os.path.join(export_root, ""_Material"")
    atlas_root = os.path.join(export_root, ""_TextureAtlas"")

    ensure_folder_exists(export_root, ""Export root"")
    ensure_folder_exists(fbx_root, ""FBX"")

    if not os.path.isdir(material_root):
        print(f""Warning: Material folder missing:\n{material_root}"")
    if not os.path.isdir(atlas_root):
        print(f""Warning: TextureAtlas folder missing:\n{atlas_root}"")

    return fbx_root, material_root, atlas_root

def sanitize_name(name: str) -> str:
    return name.strip().replace(""\\\\"", ""_"").replace(""/"", ""_"")

def normalize_rel_folder_for_match(rel_folder: str) -> str:
   return rel_folder.replace(""\\"", ""/"").strip().lower().strip(""/"")

def normalize_rel_folder_for_path(rel_folder: str) -> str:
    return rel_folder.replace(""\\"", ""/"").strip().strip(""/"")

# Collection helper removed on purpose.
# This importer now links roots/objects directly to the scene root collection.

def create_empty(name: str, location=(0.0, 0.0, 0.0)):
    obj = bpy.data.objects.get(name)
    if obj is None:
        obj = bpy.data.objects.new(name, None)
        obj.empty_display_type = 'PLAIN_AXES'
        obj.empty_display_size = 0.5
        bpy.context.scene.collection.objects.link(obj)

    if bpy.context.scene.collection not in obj.users_collection:
        bpy.context.scene.collection.objects.link(obj)

    for c in list(obj.users_collection):
        if c != bpy.context.scene.collection:
            c.objects.unlink(obj)

    obj.location = location
    obj.rotation_euler = (0.0, 0.0, 0.0)
    obj.scale = (1.0, 1.0, 1.0)
    return obj

def set_active_only(obj):
    bpy.ops.object.select_all(action='DESELECT')
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj

def snapshot_state():
    return {
        ""objects"": set(bpy.data.objects),
        ""materials"": set(bpy.data.materials),
        ""images"": set(bpy.data.images),
    }
    
def snapshot_object_names():
    return set(obj.name for obj in bpy.data.objects)

def diff_state(before):
    return {
        ""objects"": set(bpy.data.objects) - before[""objects""],
        ""materials"": set(bpy.data.materials) - before[""materials""],
        ""images"": set(bpy.data.images) - before[""images""],
    }

def is_excluded_fbx_relative_folder(rel_folder: str) -> bool:
    normalized_folder = normalize_rel_folder_for_match(rel_folder)

    for prefix in EXCLUDED_FBX_SUBFOLDER_PREFIXES:
        normalized_prefix = normalize_rel_folder_for_match(prefix)
        if not normalized_prefix:
            continue
        if normalized_folder == normalized_prefix or normalized_folder.startswith(normalized_prefix + ""/""):
            return True

    return False

def clear_parent_keep_world(obj):
    world_matrix = obj.matrix_world.copy()
    obj.parent = None
    obj.matrix_world = world_matrix

def apply_rotation_and_scale_with_ops(obj):
    if obj.type != 'MESH':
        return
    set_active_only(obj)
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)

def apply_uniform_import_multiplier_with_ops(obj, multiplier: float):
    if obj.type != 'MESH':
        return
    if abs(multiplier - 1.0) < 1e-8:
        return
    obj.scale = (
        obj.scale.x * multiplier,
        obj.scale.y * multiplier,
        obj.scale.z * multiplier,
    )
    set_active_only(obj)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)

def get_object_bottom_center_world(obj):
    world_corners = [obj.matrix_world @ Vector(corner) for corner in obj.bound_box]
    min_z = min(v.z for v in world_corners)
    min_x = min(v.x for v in world_corners)
    max_x = max(v.x for v in world_corners)
    min_y = min(v.y for v in world_corners)
    max_y = max(v.y for v in world_corners)
    return Vector(((min_x + max_x) * 0.5, (min_y + max_y) * 0.5, min_z))

def set_origin_bottom_center_keep_visible(obj):
    if obj.type != 'MESH':
        return

    cursor_prev_loc = bpy.context.scene.cursor.location.copy()
    cursor_prev_rot = bpy.context.scene.cursor.rotation_euler.copy()

    bottom_center_world = get_object_bottom_center_world(obj)
    bpy.context.scene.cursor.location = bottom_center_world
    bpy.context.scene.cursor.rotation_euler = (0.0, 0.0, 0.0)

    set_active_only(obj)
    bpy.ops.object.origin_set(type='ORIGIN_CURSOR', center='MEDIAN')

    bpy.context.scene.cursor.location = cursor_prev_loc
    bpy.context.scene.cursor.rotation_euler = cursor_prev_rot

def force_mesh_data_name_to_match_object(obj):
    if obj.type != 'MESH' or obj.data is None:
        return
    obj.data.name = obj.name

def set_shading_flat(obj):
    if obj.type != 'MESH' or obj.data is None:
        return
    for poly in obj.data.polygons:
        poly.use_smooth = False

def get_object_size_metric(obj):
    if obj.type != 'MESH':
        return 0.0
    dims = obj.dimensions
    return dims.x * dims.y * dims.z

def relink_images_to_known_files(imported_images, atlas_root: str, material_root: str):
    search_roots = []
    if os.path.isdir(atlas_root):
        search_roots.append(atlas_root)
    if os.path.isdir(material_root):
        search_roots.append(material_root)

    if not search_roots:
        return

    basename_to_path = {}
    for root in search_roots:
        for ext in (""*.png"", ""*.jpg"", ""*.jpeg"", ""*.tga"", ""*.bmp"", ""*.tif"", ""*.tiff""):
            for fp in Path(root).rglob(ext):
                basename_to_path.setdefault(fp.name.lower(), str(fp))

    for img in imported_images:
        file_name = """"
        try:
            if img.filepath:
                file_name = Path(bpy.path.abspath(img.filepath)).name.lower()
        except Exception:
            file_name = """"

        if not file_name:
            file_name = img.name.lower()

        resolved = basename_to_path.get(file_name)
        if resolved:
            try:
                img.filepath = resolved
                img.reload()
            except Exception as ex:
                print(f""Warning: could not relink image {img.name} -> {resolved}: {ex}"")

def get_blender_dedupe_base_name(name: str):
    if not name:
        return """"

    result = name.strip()

    if result.startswith("".""):
        result = result[1:]

    if len(result) > 4 and result[-4] == ""."" and result[-3:].isdigit():
        result = result[:-4]

    return result

def rename_material_images_to_match_material(materials):
    for mat in materials:
        if mat is None or not mat.use_nodes or not mat.node_tree:
            continue

        tex_nodes = [n for n in mat.node_tree.nodes if n.type == 'TEX_IMAGE' and n.image]
        if not tex_nodes:
            continue

        base_mat_name = get_blender_dedupe_base_name(mat.name)

        if len(tex_nodes) == 1:
            tex_nodes[0].image.name = base_mat_name
            continue

        base_color_node = None
        principled_nodes = [n for n in mat.node_tree.nodes if n.type == 'BSDF_PRINCIPLED']

        for principled in principled_nodes:
            base_color_input = principled.inputs.get(""Base Color"")
            if base_color_input is None:
                continue

            for link in base_color_input.links:
                from_node = link.from_node
                if from_node and from_node.type == 'TEX_IMAGE' and from_node.image:
                    base_color_node = from_node
                    break

            if base_color_node is not None:
                break

        if base_color_node is not None:
            base_color_node.image.name = base_mat_name

            suffix_index = 1
            for node in tex_nodes:
                if node == base_color_node:
                    continue
                node.image.name = f""{base_mat_name}_img{suffix_index:02d}""
                suffix_index += 1
        else:
            tex_nodes[0].image.name = base_mat_name

            suffix_index = 1
            for i in range(1, len(tex_nodes)):
                tex_nodes[i].image.name = f""{base_mat_name}_img{suffix_index:02d}""
                suffix_index += 1
def material_signature(mat):
    if mat is None:
        return None

    image_keys = []
    if mat.use_nodes and mat.node_tree:
        for node in mat.node_tree.nodes:
            if node.type == 'TEX_IMAGE' and node.image:
                try:
                    abs_fp = bpy.path.abspath(node.image.filepath)
                except Exception:
                    abs_fp = """"
                image_keys.append((node.image.name.lower(), abs_fp.lower()))

    image_keys.sort()
    return (
        mat.name.split(""."")[0].lower(),
        tuple(image_keys),
    )

def deduplicate_images():
    canonical = {}
    removed = 0

    for img in list(bpy.data.images):
        try:
            abs_fp = bpy.path.abspath(img.filepath).lower() if img.filepath else """"
        except Exception:
            abs_fp = """"

        key = abs_fp if abs_fp else img.name.lower()

        if key not in canonical:
            canonical[key] = img
            continue

        winner = canonical[key]
        if winner == img:
            continue

        for mat in bpy.data.materials:
            if not mat.use_nodes or not mat.node_tree:
                continue
            for node in mat.node_tree.nodes:
                if node.type == 'TEX_IMAGE' and node.image == img:
                    node.image = winner

        try:
            bpy.data.images.remove(img, do_unlink=True)
            removed += 1
        except Exception as ex:
            print(f""Warning: could not remove duplicate image {img.name}: {ex}"")

    print(f""Deduplicated images: removed {removed}"")

def deduplicate_materials(objects_to_fix):
    canonical = {}
    removed = 0

    for obj in objects_to_fix:
        if obj.type != 'MESH' or obj.data is None:
            continue

        for i, mat in enumerate(obj.data.materials):
            if mat is None:
                continue

            sig = material_signature(mat)
            if sig is None:
                continue

            if sig not in canonical:
                canonical[sig] = mat
                continue

            winner = canonical[sig]
            if winner != mat:
                obj.data.materials[i] = winner

    for mat in list(bpy.data.materials):
        if mat.users == 0:
            try:
                bpy.data.materials.remove(mat, do_unlink=True)
                removed += 1
            except Exception:
                pass

    print(f""Deduplicated materials: removed {removed} unused duplicates"")

def configure_classic_material_look(materials_to_fix):
    for mat in materials_to_fix:
        if mat is None:
            continue

        mat.use_backface_culling = ENABLE_BACKFACE_CULLING

        if not mat.use_nodes or not mat.node_tree:
            continue

        nt = mat.node_tree
        principled = None

        for node in nt.nodes:
            if node.type == 'TEX_IMAGE':
                node.interpolation = TEXTURE_INTERPOLATION
            if node.type == 'BSDF_PRINCIPLED':
                principled = node

        if principled is not None:
            try:
                if ""Metallic"" in principled.inputs:
                    principled.inputs[""Metallic""].default_value = 0.0
                if ""Roughness"" in principled.inputs:
                    principled.inputs[""Roughness""].default_value = 1.0
                if ""Specular IOR Level"" in principled.inputs:
                    principled.inputs[""Specular IOR Level""].default_value = 0.0
                elif ""Specular"" in principled.inputs:
                    principled.inputs[""Specular""].default_value = 0.0
            except Exception as ex:
                print(f""Warning: material tweak failed on {mat.name}: {ex}"")

def collect_mesh_objects(new_objects):
    results = []
    for obj in new_objects:
        if PROCESS_MESH_OBJECTS_ONLY:
            if obj.type == 'MESH':
                results.append(obj)
        else:
            results.append(obj)
    return results

def flatten_imported_hierarchy(mesh_objects):
    for obj in mesh_objects:
        clear_parent_keep_world(obj)

def get_target_object_names_for_fbx(imported_meshes, fbx_path: Path):
    imported_meshes_sorted = sorted(imported_meshes, key=lambda o: o.name.lower())
    stem = sanitize_name(os.path.splitext(os.path.basename(str(fbx_path)))[0])

    target_names = []
    if len(imported_meshes_sorted) == 1:
        target_names.append(stem)
    else:
        for idx, _obj in enumerate(imported_meshes_sorted, start=1):
            if idx == 1:
                target_names.append(stem)
            else:
                target_names.append(f""{stem}_p{idx:02d}"")

    return imported_meshes_sorted, target_names

def any_target_name_already_exists(target_names, existing_object_names_before_import):
    collisions = []
    for name in target_names:
        if name in existing_object_names_before_import:
            collisions.append(name)
    return collisions

def assign_imported_object_names(imported_meshes_sorted, target_names):
    for obj, target_name in zip(imported_meshes_sorted, target_names):
        obj.name = target_name
        if obj.type == 'MESH' and obj.data is not None:
            obj.data.name = target_name

def process_mesh_object(obj):
    apply_rotation_and_scale_with_ops(obj)
    apply_uniform_import_multiplier_with_ops(obj, IMPORT_SCALE_MULTIPLIER)
    set_origin_bottom_center_keep_visible(obj)

    if USE_SHADE_FLAT:
        set_shading_flat(obj)

    force_mesh_data_name_to_match_object(obj)

def choose_grid(n: int):
    cols = max(1, math.ceil(math.sqrt(n)))
    rows = max(1, math.ceil(n / cols))
    return cols, rows

def get_object_planar_size_with_min_cell(obj):
    if obj.type != 'MESH':
        return OBJECT_CELL_SIZE, OBJECT_CELL_SIZE

    min_cell = 2.5
    gap = 0.25

    width = max(obj.dimensions.x, min_cell) + gap
    depth = max(obj.dimensions.y, min_cell) + gap
    return width, depth

def is_large_layout_object(obj, threshold=5.0):
    if obj.type != 'MESH':
        return False
    dims = obj.dimensions
    return dims.x > threshold or dims.y > threshold

def is_medium_layout_object(obj, threshold=1.8):
    if obj.type != 'MESH':
        return False
    dims = obj.dimensions
    return dims.x > threshold or dims.y > threshold

def layout_objects_under_category_parent(parent_obj, objects_in_category):
    if not objects_in_category:
        return

    if SORT_SMALLEST_TO_LARGEST:
        ordered = sorted(objects_in_category, key=get_object_size_metric)
    else:
        ordered = sorted(objects_in_category, key=lambda o: o.name.lower())

    small_objects = []
    medium_objects = []
    large_objects = []

    for obj in ordered:
        if is_large_layout_object(obj, threshold=5.0):
            large_objects.append(obj)
        elif is_medium_layout_object(obj, threshold=1.8):
            medium_objects.append(obj)
        else:
            small_objects.append(obj)

    def layout_regular_block(block_objects, start_x, start_y, cell_size):
        if not block_objects:
            return 0.0, 0.0

        cols, rows = choose_grid(len(block_objects))

        for i, obj in enumerate(block_objects):
            gx = i % cols
            gy = i // cols

            obj.location = (start_x + (gx * cell_size), start_y + (gy * cell_size), 0.0)
            obj.rotation_euler = (0.0, 0.0, 0.0)
            obj.scale = (1.0, 1.0, 1.0)
            obj.parent = parent_obj

        block_width = cols * cell_size
        block_height = rows * cell_size
        return block_width, block_height

    def layout_dynamic_block(block_objects, start_x, start_y):
        if not block_objects:
            return 0.0, 0.0

        cols, rows = choose_grid(len(block_objects))

        row_max_depths = [0.0] * rows
        col_max_widths = [0.0] * cols
        object_sizes = []

        for i, obj in enumerate(block_objects):
            gx = i % cols
            gy = i // cols

            width, depth = get_object_planar_size_with_min_cell(obj)
            object_sizes.append((obj, gx, gy, width, depth))

            if depth > row_max_depths[gy]:
                row_max_depths[gy] = depth
            if width > col_max_widths[gx]:
                col_max_widths[gx] = width

        x_positions = [0.0] * cols
        running_x = start_x
        for gx in range(cols):
            x_positions[gx] = running_x
            running_x += col_max_widths[gx]

        y_positions = [0.0] * rows
        running_y = start_y
        for gy in range(rows):
            y_positions[gy] = running_y
            running_y += row_max_depths[gy]

        for obj, gx, gy, width, depth in object_sizes:
            obj.location = (x_positions[gx], y_positions[gy], 0.0)
            obj.rotation_euler = (0.0, 0.0, 0.0)
            obj.scale = (1.0, 1.0, 1.0)
            obj.parent = parent_obj

        block_width = sum(col_max_widths)
        block_height = sum(row_max_depths)
        return block_width, block_height

    small_width, small_height = layout_regular_block(
        small_objects,
        start_x=0.0,
        start_y=0.0,
        cell_size=OBJECT_CELL_SIZE
    )

    vertical_gap = OBJECT_CELL_SIZE

    medium_width, medium_height = layout_dynamic_block(
        medium_objects,
        start_x=0.0,
        start_y=small_height + vertical_gap
    )

    large_start_y = small_height + vertical_gap
    if medium_objects:
        large_start_y += medium_height + vertical_gap

    large_width, large_height = layout_dynamic_block(
        large_objects,
        start_x=0.0,
        start_y=large_start_y
    )
def choose_category_empty_positions(category_names):
    ordered = sorted(category_names, key=lambda s: s.lower())
    cols, rows = choose_grid(len(ordered))

    positions = {}
    for i, name in enumerate(ordered):
        gx = i % cols
        gy = i // cols
        positions[name] = (gx * CATEGORY_GAP_X, -(gy * CATEGORY_GAP_Y), 0.0)
    return positions

def import_one_fbx(filepath: Path):
    before = snapshot_state()

    bpy.ops.import_scene.fbx(
        filepath=str(filepath),
        automatic_bone_orientation=False,
        use_image_search=True
    )

    return diff_state(before)

def delete_imported_data(imported):
    for obj in list(imported[""objects""]):
        try:
            bpy.data.objects.remove(obj, do_unlink=True)
        except Exception:
            pass

    for mat in list(imported[""materials""]):
        try:
            if mat.users == 0:
                bpy.data.materials.remove(mat, do_unlink=True)
        except Exception:
            pass

    for img in list(imported[""images""]):
        try:
            if img.users == 0:
                bpy.data.images.remove(img, do_unlink=True)
        except Exception:
            pass

def make_category_name_from_rel_folder(rel_folder: str) -> str:
    normalized = normalize_rel_folder_for_path(rel_folder)
    if not normalized:
        return ""_Root""
    return normalized.replace(""/"", ""_"")
def make_category_empty_name(category_name: str, category_files) -> str:
    if category_files:
        endpoint_folder_name = Path(category_files[0]).parent.name.strip()
        if endpoint_folder_name:
            suffix = """"
            first_stem = Path(category_files[0]).stem
            last_underscore_index = first_stem.rfind(""_"")
            if last_underscore_index >= 0:
                suffix = first_stem[last_underscore_index:]
            return f""{endpoint_folder_name}{suffix}""

    return f""_{category_name}""
def get_rel_folder_for_fbx(fbx_root: str, filepath: Path) -> str:
    rel_parent = filepath.parent.relative_to(Path(fbx_root))
    if str(rel_parent) == ""."":
        return """"
    return str(rel_parent).replace(""\\"", ""/"")

def gather_fbx_files_for_one_model(fbx_root: str):
    fp = Path(ONE_MODEL_PATH)
    if not fp.is_file():
        raise RuntimeError(f""ONE_MODEL_PATH does not exist:\n{ONE_MODEL_PATH}"")
    if fp.suffix.lower() != "".fbx"":
        raise RuntimeError(f""ONE_MODEL_PATH is not an FBX:\n{ONE_MODEL_PATH}"")

    try:
        fp.relative_to(Path(fbx_root))
    except ValueError:
        raise RuntimeError(f""ONE_MODEL_PATH is not under FBX root:\n{ONE_MODEL_PATH}\n\nFBX root:\n{fbx_root}"")

    rel_folder = get_rel_folder_for_fbx(fbx_root, fp)
    category_name = make_category_name_from_rel_folder(rel_folder)
    return {category_name: [fp]}

def gather_fbx_files_for_one_category(fbx_root: str):
    rel_folder = normalize_rel_folder_for_path(ONE_CATEGORY_RELATIVE)
    if not rel_folder:
        raise RuntimeError(""ONE_CATEGORY_RELATIVE is empty."")

    category_dir = Path(fbx_root) / rel_folder
    if not category_dir.is_dir():
        raise RuntimeError(f""ONE_CATEGORY_RELATIVE folder does not exist:\n{category_dir}"")

    files = sorted(category_dir.rglob(""*.fbx""))
    if not files:
        raise RuntimeError(f""No FBX files found in ONE_CATEGORY_RELATIVE:\n{category_dir}"")

    category_name = make_category_name_from_rel_folder(rel_folder)
    return {category_name: files}

def gather_fbx_files_for_folder_list(fbx_root: str):
    category_map = {}

    if not FOLDER_LIST_RELATIVE:
        raise RuntimeError(""FOLDER_LIST_RELATIVE is empty."")

    for rel in FOLDER_LIST_RELATIVE:
        rel_folder = normalize_rel_folder_for_path(rel)
        if not rel_folder:
            continue

        category_dir = Path(fbx_root) / rel_folder
        if not category_dir.is_dir():
            raise RuntimeError(f""FOLDER_LIST_RELATIVE folder does not exist:\n{category_dir}"")

        files = sorted(category_dir.rglob(""*.fbx""))
        if not files:
            print(f""Warning: no FBX files found in folder list entry:\n{category_dir}"")
            continue

        category_name = make_category_name_from_rel_folder(rel_folder)
        category_map[category_name] = files

    if not category_map:
        raise RuntimeError(""FOLDER_LIST mode found no FBX files."")

    return category_map

def gather_fbx_files_for_all(fbx_root: str):
    category_map = {}
    root_path = Path(fbx_root)

    for fp in sorted(root_path.rglob(""*.fbx"")):
        rel_folder = get_rel_folder_for_fbx(fbx_root, fp)

        if rel_folder and is_excluded_fbx_relative_folder(rel_folder):
            print(f""Skipping excluded FBX folder: {rel_folder}"")
            continue

        category_name = make_category_name_from_rel_folder(rel_folder)
        category_map.setdefault(category_name, [])
        category_map[category_name].append(fp)

    if not category_map:
        raise RuntimeError(f""No FBX files found under:\n{fbx_root}"")

    return category_map

def gather_category_map_for_mode(fbx_root: str):
    mode = IMPORT_MODE.strip().upper()

    if mode == ""ONE_MODEL"":
        return gather_fbx_files_for_one_model(fbx_root)
    if mode == ""ONE_CATEGORY"":
        return gather_fbx_files_for_one_category(fbx_root)
    if mode == ""FOLDER_LIST"":
        return gather_fbx_files_for_folder_list(fbx_root)
    if mode == ""ALL"":
        return gather_fbx_files_for_all(fbx_root)

    raise RuntimeError(
        f""Unsupported IMPORT_MODE: {IMPORT_MODE}\n""
        f""Valid values: ONE_MODEL, ONE_CATEGORY, FOLDER_LIST, ALL""
    )

# =========================================================
# MAIN
# =========================================================

def main():
    require_saved_blend()

    fbx_root, material_root, atlas_root = get_expected_folders(EXPORT_ROOT)

    category_map = gather_category_map_for_mode(fbx_root)
    category_positions = choose_category_empty_positions(category_map.keys())

    all_new_mesh_objects = []
    all_new_materials = set()
    all_new_images = set()
    skipped_imports = []

    for category_name in sorted(category_map.keys(), key=lambda s: s.lower()):
        category_files = category_map[category_name]
        empty_name = make_category_empty_name(category_name, category_files)
        empty_loc = category_positions[category_name]
        category_empty = create_empty(empty_name, location=empty_loc)

        category_mesh_objects = []

        print(f""--- Importing category: {category_name} ---"")

        for fbx_path in category_map[category_name]:
            print(f""Importing: {fbx_path}"")

            existing_object_names_before_import = snapshot_object_names()
            imported = import_one_fbx(fbx_path)

            imported_images = list(imported[""images""])
            imported_materials = list(imported[""materials""])
            imported_meshes = collect_mesh_objects(imported[""objects""])

            flatten_imported_hierarchy(imported_meshes)

            if not imported_meshes:
                skipped_imports.append({
                    ""path"": str(fbx_path),
                    ""reason"": ""No mesh objects found after FBX import."",
                })
                delete_imported_data(imported)
                continue

            imported_meshes_sorted, target_names = get_target_object_names_for_fbx(imported_meshes, fbx_path)
            collisions = any_target_name_already_exists(target_names, existing_object_names_before_import)

            if collisions:
                skipped_imports.append({
                    ""path"": str(fbx_path),
                    ""reason"": f""Target object name already exists in scene: {', '.join(collisions)}"",
                })
                delete_imported_data(imported)
                continue

            relink_images_to_known_files(imported_images, atlas_root, material_root)
            rename_material_images_to_match_material(imported_materials)
            assign_imported_object_names(imported_meshes_sorted, target_names)

            for obj, target_name in zip(imported_meshes_sorted, target_names):
                if bpy.context.scene.collection not in obj.users_collection:
                    bpy.context.scene.collection.objects.link(obj)

                for c in list(obj.users_collection):
                    if c != bpy.context.scene.collection:
                        c.objects.unlink(obj)

                process_mesh_object(obj)

                obj.name = target_name
                if obj.type == 'MESH' and obj.data is not None:
                    obj.data.name = target_name

                category_mesh_objects.append(obj)
                all_new_mesh_objects.append(obj)

            all_new_materials.update(imported[""materials""])
            all_new_images.update(imported[""images""])

        layout_objects_under_category_parent(category_empty, category_mesh_objects)

    deduplicate_images()
    deduplicate_materials(all_new_mesh_objects)
    configure_classic_material_look(list(bpy.data.materials))

    for obj in all_new_mesh_objects:
        if obj.type == 'MESH' and obj.data is not None:
            obj.rotation_euler = (0.0, 0.0, 0.0)
            obj.scale = (1.0, 1.0, 1.0)
            obj.data.name = obj.name

    print("""")
    print(""Done."")
    print(f""Import mode: {IMPORT_MODE}"")
    print(f""Export root: {EXPORT_ROOT}"")
    print(f""FBX root: {fbx_root}"")
    print(f""Imported mesh objects: {len(all_new_mesh_objects)}"")
    print(f""Imported materials seen: {len(all_new_materials)}"")
    print(f""Imported images seen: {len(all_new_images)}"")

    if skipped_imports:
        print("""")
        print(""Skipped imports:"")
        for item in skipped_imports:
            print(f""- {item['path']}"")
            print(f""  Reason: {item['reason']}"")
    
    print("""")
    print(""Reminder: manually verify Blender scale against in-game/play-mode scale."")

if __name__ == ""__main__"":
    main()";
    
    
    
    
    
    
    
private const string MobileSpritesFolderAssetRelative = "Assets/Exports/DaggerfallAssets/MobileSprites";
private const string MobileTownNpcFolderAssetRelative = "Assets/Exports/DaggerfallAssets/MobileSprites/TownNPC";
private const string MobileEnemyFolderAssetRelative = "Assets/Exports/DaggerfallAssets/MobileSprites/Enemy";

private Races mobileTownNpcRace = Races.Nord;
private Genders mobileTownNpcGender = Genders.Male;
private int mobileTownNpcVariant = 2;
private bool mobileTownNpcIsGuard = false;
private int mobileTownNpcSelectionIndex = 0;
private bool mobileTownNpcIncludeMove = true;
private bool mobileTownNpcIncludeIdle = true;

private int mobileEnemySelectionIndex = 0;
private bool mobileEnemyIncludeMove = true;
private bool mobileEnemyIncludeIdle = true;
private bool mobileEnemyIncludePrimaryAttack = true;
private bool mobileEnemyIncludeHurt = true;
private bool mobileEnemyIncludeRangedAttack1 = true;
private bool mobileEnemyIncludeRangedAttack2 = true;
private int mobileSpriteSortMode = 0;
private bool mobileSpriteApplyImportSettingsAfterExport = true;



private bool mobileSpriteImportUsePointFilter = true;
private int mobileSpriteImportMaxTextureSize = 512;
private GUIStyle cachedGroupedWarningStyle;
private string[] cachedFlatCategoryBatchSources = new string[0];
private bool flatCategoryBatchSourcesDirty = true;

private List<TownNpcMobileListEntry> cachedTownNpcMobileEntries = new List<TownNpcMobileListEntry>();
private string[] cachedTownNpcMobileLabels = new string[0];
private int cachedTownNpcMobileSortMode = -1;

private List<EnemyMobileListEntry> cachedEnemyMobileEntries = new List<EnemyMobileListEntry>();
private string[] cachedEnemyMobileLabels = new string[0];
private int cachedEnemyMobileSortMode = -1;

private int assetEditingDepth = 0;
private int deferredRefreshDepth = 0;





[Serializable]
private class TownNpcExportManifest
{
    public string exportType;
    public string entityName;
    public string race;
    public string gender;
    public int outfitVariant;
    public bool isGuard;
    public int archive;
    public List<TownNpcExportClip> clips = new List<TownNpcExportClip>();
}

[Serializable]
private class TownNpcExportClip
{
    public string animationName;
    public string orientationName;
    public int record;
    public bool flipLeftRight;
    public int frameCount;
    public List<string> files = new List<string>();
    public List<TownNpcExportFrame> frames = new List<TownNpcExportFrame>();
}

[Serializable]
private class TownNpcExportFrame
{
    public string fileName;
    public int archive;
    public int record;
    public int frame;
    public bool flipLeftRight;
}
[Serializable]
private class MobileSpriteExportManifest
{
    public string exportType;
    public string category;
    public string entityName;
    public int archive;
    public int enemyId = -1;
    public bool isFemaleVariant;
    public bool hasSpellState;
    public bool hasSeducerTransform1;
    public bool hasSeducerTransform2;
    public List<TownNpcExportClip> clips = new List<TownNpcExportClip>();
}

private struct MobileSpriteAnimSpec
{
    public string AnimationName;
    public string OrientationName;
    public int Record;
    public bool FlipLeftRight;

    public MobileSpriteAnimSpec(string animationName, string orientationName, int record, bool flipLeftRight)
    {
        AnimationName = animationName;
        OrientationName = orientationName;
        Record = record;
        FlipLeftRight = flipLeftRight;
    }
}

private struct TownNpcAnimSpec
{
    public string AnimationName;
    public string OrientationName;
    public int Record;
    public bool FlipLeftRight;

    public TownNpcAnimSpec(string animationName, string orientationName, int record, bool flipLeftRight)
    {
        AnimationName = animationName;
        OrientationName = orientationName;
        Record = record;
        FlipLeftRight = flipLeftRight;
    }
}
private static readonly string[] TopTabLabels = new[]
{
    "Single & Range FBX",
    "Category FBX",
    "Mobile Sprites PNG",
    "Settings & Paths",
    "Debug & Asset Deletion",
    "Readme & Info"
};
private static readonly string[] FixedCategoryBatchSources = new[]
{
    "dungeonParts_caves",
    "dungeonParts_corridors",
    "dungeonParts_doors",
    "dungeonParts_misc",
    "dungeonParts_rooms",
    "houseParts",
    "models_clutter",
    "models_dungeon",
    "models_furniture",
    "models_graveyard",
    "ships",
    "models_structure"
};

private readonly Dictionary<string, double> categoryLastDurationSeconds =
    new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

private struct HardcodedCategoryCount
{
    public int BaseCount;
    public int GroupedCount;

    public HardcodedCategoryCount(int baseCount, int groupedCount)
    {
        BaseCount = baseCount;
        GroupedCount = groupedCount;
    }
}

private static readonly Dictionary<string, HardcodedCategoryCount> HardcodedCategoryCounts =
    new Dictionary<string, HardcodedCategoryCount>(StringComparer.OrdinalIgnoreCase)
{
    { "dungeonParts_caves",      new HardcodedCategoryCount(24, 52) },
    { "dungeonParts_corridors",  new HardcodedCategoryCount(116, 538) },
    { "dungeonParts_doors",      new HardcodedCategoryCount(9, 30) },
    { "dungeonParts_misc",       new HardcodedCategoryCount(52, 75) },
    { "dungeonParts_rooms",      new HardcodedCategoryCount(134, 516) },
    { "houseParts",              new HardcodedCategoryCount(245, 3009) },
    { "models_clutter",          new HardcodedCategoryCount(51, 169) },
    { "models_dungeon",          new HardcodedCategoryCount(14, 44) },
    { "models_furniture",        new HardcodedCategoryCount(22, 91) },
    { "models_graveyard",        new HardcodedCategoryCount(39, 152) },
};

private static readonly Regex TextureNameRegex =
    new Regex(@"TEXTURE_(\d+)_Index_(\d+)_?", RegexOptions.IgnoreCase | RegexOptions.Compiled);

private static readonly Regex TextureNameBracketRegex =
    new Regex(@"TEXTURE\.(\d+)\s*\[Index=(\d+)\]", RegexOptions.IgnoreCase | RegexOptions.Compiled);


private static Type cachedExporterType;
private static MethodInfo cachedExportMethod;

private static Type cachedIExportOptionsType;
private static Type cachedExportModelSettingsSerializeType;
private static Type cachedExportSettingsType;
private static PropertyInfo cachedExportFormatProperty;
private static MethodInfo cachedSetExportFormatMethod;
private static object cachedBinaryExportFormatValue;


   private struct VertexKey : IEquatable<VertexKey>
{
    public int SourceVertexIndex;
    public int SubmeshIndex;

    public VertexKey(int sourceVertexIndex, int submeshIndex)
    {
        SourceVertexIndex = sourceVertexIndex;
        SubmeshIndex = submeshIndex;
    }

    public bool Equals(VertexKey other)
    {
        return SourceVertexIndex == other.SourceVertexIndex && SubmeshIndex == other.SubmeshIndex;
    }

    public override bool Equals(object obj)
    {
        return obj is VertexKey && Equals((VertexKey)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (SourceVertexIndex * 397) ^ SubmeshIndex;
        }
    }
}

private struct BatchTexturePlacement
{
    public int PageIndex;
    public Rect Rect;
    public int PixelX;
    public int PixelY;
    public int Width;
    public int Height;
}

private class BatchTexturePage
{
    public int PageIndex;
    public int FinalWidth;
    public int FinalHeight;
    public Texture2D AtlasTexture;
    public string AtlasAssetPath;
    public string MaterialAssetPath;
    public Material MaterialAsset;
    public List<string> TextureKeys = new List<string>();
}

private class BatchModelPlan
{
    public uint ModelId;
    public string ExportBaseName;
    public string CategoryFieldName;
    public bool UseCategorySubfolder;
    public Mesh Mesh;
    public string[] TextureKeys;
}

private class BatchFlatPlan
{
    public int FlatArchive;
    public int FlatRecord;
    public string ExportBaseName;
    public string CategoryFieldName;
    public bool UseCategorySubfolder;
    public Mesh Mesh;
    public string TextureKey;
}

private class CategoryBatchSourceInfo
{
    public string FieldName;
    public string DisplayName;
    public FieldInfo FieldInfo;
}
private class ExportRequest
{
    public bool IsFlat;
    public uint ModelId;
    public int FlatArchive;
    public int FlatRecord;
}
private enum InfoDocRowType
{
    Header,
    Script
}

private struct InfoDocRow
{
    public InfoDocRowType RowType;
    public string HeaderText;
    public ScriptDocEntry ScriptEntry;

    public static InfoDocRow MakeHeader(string text)
    {
        InfoDocRow row = new InfoDocRow();
        row.RowType = InfoDocRowType.Header;
        row.HeaderText = text;
        row.ScriptEntry = default(ScriptDocEntry);
        return row;
    }

    public static InfoDocRow MakeScript(string displayText, string copyScriptName)
    {
        InfoDocRow row = new InfoDocRow();
        row.RowType = InfoDocRowType.Script;
        row.HeaderText = null;
        row.ScriptEntry = new ScriptDocEntry(displayText, copyScriptName);
        return row;
    }
}

private string BuildInfoReadmeText(InfoDocRow[] rows)
{
    StringBuilder sb = new StringBuilder();

    sb.AppendLine("DFU Export Readme / Info");
    sb.AppendLine();

    sb.AppendLine("Version 7, April 2, 2026. Made by westingtyler. Requires Unity FBX Exporter package. Tested in Unity 2019.4.40f1 with Daggerfall Unity 1.1.1. Exported FBX files import directly into Blender 4.5.7 LTS.");
    sb.AppendLine();

    if (rows != null)
    {
        for (int i = 0; i < rows.Length; i++)
        {
            if (rows[i].RowType == InfoDocRowType.Header)
            {
                if (!string.IsNullOrWhiteSpace(rows[i].HeaderText))
                {
                    sb.AppendLine(rows[i].HeaderText + ":");
                    sb.AppendLine();
                }
            }
            else
            {
                string displayText = rows[i].ScriptEntry.DisplayText ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(displayText))
                {
                    sb.AppendLine(displayText);
                    sb.AppendLine();
                }
            }
        }
    }

    sb.AppendLine("Blender Text Editor Import Script:");
    sb.AppendLine();
    sb.AppendLine(BlenderTextEditorImportScriptDescription);
    sb.AppendLine();
    sb.AppendLine(BlenderTextEditorImportScriptText);

    return sb.ToString();
}
private void DrawBlenderImportScriptFoldout()
{
    EditorGUILayout.LabelField("Blender Text Editor Import Script", EditorStyles.boldLabel);
    GUILayout.Space(4);

    EditorGUILayout.LabelField(
        BlenderTextEditorImportScriptDescription,
        EditorStyles.wordWrappedLabel);

    GUILayout.Space(4);

    if (GUILayout.Button("Copy Blender Text Editor Importer Script", GUILayout.Width(260), GUILayout.Height(22)))
        EditorGUIUtility.systemCopyBuffer = BlenderTextEditorImportScriptText ?? string.Empty;
}
private struct BatchVertexKey : IEquatable<BatchVertexKey>
{
    public int SourceVertexIndex;
    public int SubmeshIndex;
    public int LocalMaterialSlot;

    public BatchVertexKey(int sourceVertexIndex, int submeshIndex, int localMaterialSlot)
    {
        SourceVertexIndex = sourceVertexIndex;
        SubmeshIndex = submeshIndex;
        LocalMaterialSlot = localMaterialSlot;
    }

    public bool Equals(BatchVertexKey other)
    {
        return SourceVertexIndex == other.SourceVertexIndex &&
               SubmeshIndex == other.SubmeshIndex &&
               LocalMaterialSlot == other.LocalMaterialSlot;
    }

    public override bool Equals(object obj)
    {
        return obj is BatchVertexKey && Equals((BatchVertexKey)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = SourceVertexIndex;
            hash = (hash * 397) ^ SubmeshIndex;
            hash = (hash * 397) ^ LocalMaterialSlot;
            return hash;
        }
    }
}

[MenuItem("NexusDev/DFU Export/Open Export Window")]
public static void OpenWindow()
        {
            ExportDfuModelsWest window = GetWindow<ExportDfuModelsWest>("DFU Export");
            window.minSize = new Vector2(620f, 340f);
            window.LoadPrefs();
            window.Show();
        }

        [MenuItem("NexusDev/DFU Export/Open Export Folder")]
        public static void OpenExportFolder()
        {
            EnsureFolderExists(ExportFolderAssetRelative);
            string abs = GetAbsolutePathFromAssetPath(ExportFolderAssetRelative);
            if (!string.IsNullOrEmpty(abs))
                EditorUtility.RevealInFinder(abs);
        }
private void OnEnable()
{
    LoadPrefs();
    MarkFlatCategorySourcesDirty();
    RefreshFlatCategorySourcesCacheIfNeeded();
    InvalidateMobileListCaches();
}

private Vector2 windowScroll;

private GUIStyle GetGroupedWarningStyle()
{
    if (cachedGroupedWarningStyle == null)
    {
        cachedGroupedWarningStyle = new GUIStyle(EditorStyles.wordWrappedMiniLabel);
        cachedGroupedWarningStyle.fontStyle = FontStyle.Bold;
        cachedGroupedWarningStyle.wordWrap = true;
        cachedGroupedWarningStyle.richText = false;
        cachedGroupedWarningStyle.normal.textColor = new Color(1f, 0.58f, 0.78f, 1f);
    }

    return cachedGroupedWarningStyle;
}

private struct ScriptDocEntry
{
    public string DisplayText;
    public string CopyScriptName;

    public ScriptDocEntry(string displayText, string copyScriptName)
    {
        DisplayText = displayText;
        CopyScriptName = copyScriptName;
    }
}

private static string ResolveScriptNameForCopy(string displayText)
{
    if (string.IsNullOrWhiteSpace(displayText))
        return string.Empty;

    string text = displayText.Trim();

    int parenIndex = text.IndexOf('(');
    if (parenIndex >= 0)
        text = text.Substring(0, parenIndex).Trim();

    string[] parts = text.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
    if (parts.Length == 0)
        return text;

    if (parts.Length >= 2)
        return parts[parts.Length - 2];

    return parts[0];
}


private void MarkFlatCategorySourcesDirty()
{
    flatCategoryBatchSourcesDirty = true;
}

private void RefreshFlatCategorySourcesCacheIfNeeded()
{
    if (!flatCategoryBatchSourcesDirty && cachedFlatCategoryBatchSources != null)
        return;

    cachedFlatCategoryBatchSources = CollectAvailableFlatCategoryBatchSources();
    flatCategoryBatchSourcesDirty = false;
}

private string[] GetAvailableFlatCategoryBatchSources()
{
    RefreshFlatCategorySourcesCacheIfNeeded();
    return cachedFlatCategoryBatchSources ?? new string[0];
}

private void InvalidateMobileListCaches()
{
    cachedTownNpcMobileSortMode = -1;
    cachedEnemyMobileSortMode = -1;
}

private void RefreshTownNpcMobileCacheIfNeeded()
{
    if (cachedTownNpcMobileSortMode == mobileSpriteSortMode &&
        cachedTownNpcMobileEntries != null &&
        cachedTownNpcMobileLabels != null)
    {
        return;
    }

    cachedTownNpcMobileEntries = BuildTownNpcMobileListEntriesUncached();
    cachedTownNpcMobileLabels = new string[cachedTownNpcMobileEntries.Count];

    for (int i = 0; i < cachedTownNpcMobileEntries.Count; i++)
        cachedTownNpcMobileLabels[i] = cachedTownNpcMobileEntries[i].Label;

    cachedTownNpcMobileSortMode = mobileSpriteSortMode;
}

private void RefreshEnemyMobileCacheIfNeeded()
{
    if (cachedEnemyMobileSortMode == mobileSpriteSortMode &&
        cachedEnemyMobileEntries != null &&
        cachedEnemyMobileLabels != null)
    {
        return;
    }

    cachedEnemyMobileEntries = BuildEnemyMobileListEntriesUncached();
    cachedEnemyMobileLabels = new string[cachedEnemyMobileEntries.Count];

    for (int i = 0; i < cachedEnemyMobileEntries.Count; i++)
        cachedEnemyMobileLabels[i] = cachedEnemyMobileEntries[i].Label;

    cachedEnemyMobileSortMode = mobileSpriteSortMode;
}

private List<TownNpcMobileListEntry> GetTownNpcMobileEntriesCached()
{
    RefreshTownNpcMobileCacheIfNeeded();
    return cachedTownNpcMobileEntries;
}
private string[] GetTownNpcMobileOptionLabels()
{
    RefreshTownNpcMobileCacheIfNeeded();
    return cachedTownNpcMobileLabels ?? new string[0];
}

private string[] GetEnemyMobileOptionLabels()
{
    RefreshEnemyMobileCacheIfNeeded();
    return cachedEnemyMobileLabels ?? new string[0];
}

private List<EnemyMobileListEntry> GetEnemyMobileEntriesCached()
{
    RefreshEnemyMobileCacheIfNeeded();
    return cachedEnemyMobileEntries;
}


private void BeginDeferredAssetEditing()
{
    if (assetEditingDepth == 0)
        AssetDatabase.StartAssetEditing();

    assetEditingDepth++;
    deferredRefreshDepth++;
}

private void EndDeferredAssetEditing(bool refreshAndSave)
{
    if (deferredRefreshDepth > 0)
        deferredRefreshDepth--;

    if (assetEditingDepth <= 0)
        return;

    assetEditingDepth--;

    if (assetEditingDepth == 0)
    {
        AssetDatabase.StopAssetEditing();

        if (refreshAndSave)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}

private bool ShouldPerformImmediateAssetRefresh()
{
    return deferredRefreshDepth <= 0;
}

private bool PauseDeferredAssetEditingForImmediateImports()
{
    if (assetEditingDepth <= 0)
        return false;

    AssetDatabase.StopAssetEditing();
    return true;
}

private void ResumeDeferredAssetEditingAfterImmediateImports(bool wasPaused)
{
    if (!wasPaused)
        return;

    AssetDatabase.StartAssetEditing();
}

private void DrawSingleTabIntro()
{
    EditorGUILayout.HelpBox(
        "One-off and manual FBX export. Models and Flats are separated below so each has its own single export, manual list/range area, and txt-file area.",
        MessageType.None);
    GUILayout.Space(8);
}

private void DrawCategoryTabIntro()
{
    EditorGUILayout.HelpBox(
        "Bulk category/group FBX export. Use this tab for full model or flat category passes.",
        MessageType.None);
    GUILayout.Space(8);
}

private void DrawSettingsTabIntro()
{
    EditorGUILayout.HelpBox(
        "General export settings and folder actions. The optional single-export txt file paths now live on the Single tab under each section.",
        MessageType.None);
    GUILayout.Space(8);
}

private void DrawDebugTabIntro()
{
    EditorGUILayout.HelpBox(
        "Destructive cleanup tools and timing export live here so they stay away from normal export actions.",
        MessageType.Warning);
    GUILayout.Space(8);
}

private void DrawInfoTabIntro()
{
    EditorGUILayout.LabelField(
    "Readme, data sources, naming sources, and script-method notes for how this window derives export names, IDs, categories, flats, and mobile data.",
    EditorStyles.wordWrappedLabel);
    GUILayout.Space(8);
}

private void DrawGroupedFamilyToggleShared()
{
    bool newValue = EditorGUILayout.ToggleLeft(
        "Export grouped family members from WorldDataEditorObjectData.modelGroups / flatGroups when available",
        exportGroupedFamilyMembers);

    if (newValue != exportGroupedFamilyMembers)
    {
        exportGroupedFamilyMembers = newValue;
        SavePrefs();
        Repaint();
    }
}

private void DrawSingleModelGroupedWarning()
{
    if (!exportGroupedFamilyMembers)
        return;

    int groupedCountForSingle = GetGroupedFamilyCountForModelId(singleModelId);

    EditorGUILayout.BeginHorizontal();
    GUILayout.Space(16f);
    EditorGUILayout.BeginVertical();
    GUILayout.Label(
        "Grouped family export is ON. This single model request resolves to " +
        groupedCountForSingle.ToString(CultureInfo.InvariantCulture) +
        " model(s) when this ID belongs to a modelGroups family.",
        GetGroupedWarningStyle());
    EditorGUILayout.EndVertical();
    EditorGUILayout.EndHorizontal();

    GUILayout.Space(4f);
}

private void DrawSingleExportTab()
{
    EditorGUILayout.LabelField("Single / Manual FBX Export", EditorStyles.boldLabel);
    GUILayout.Space(6);

    DrawSingleTabIntro();

    EditorGUILayout.LabelField("Models FBX", EditorStyles.boldLabel);
    DrawGroupedFamilyToggleShared();
    DrawSingleModelGroupedWarning();
    GUILayout.Space(4);

    EditorGUILayout.BeginHorizontal();
    EditorGUILayout.BeginVertical();

    EditorGUILayout.LabelField("Single Model ID", EditorStyles.boldLabel);
    string idString = EditorGUILayout.TextField(singleModelId.ToString(CultureInfo.InvariantCulture));
    uint parsedId;
    if (uint.TryParse(idString, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsedId))
        singleModelId = parsedId;

    EditorGUILayout.EndVertical();
    GUILayout.FlexibleSpace();

    if (GUILayout.Button("Export Single Model FBX", GUILayout.Width(170), GUILayout.Height(34)))
    {
        SavePrefs();
        ExportSingleConfiguredModel();
    }

    EditorGUILayout.EndHorizontal();

    GUILayout.Space(6);

    EditorGUILayout.BeginHorizontal();
    EditorGUILayout.BeginVertical();

    EditorGUILayout.LabelField("Model IDs / Ranges", EditorStyles.boldLabel);
    singleModelBatchRangeText = EditorGUILayout.TextField(singleModelBatchRangeText ?? string.Empty);
    EditorGUILayout.LabelField("Examples: 1-19 or 5,7,10-14", EditorStyles.miniLabel);

    EditorGUILayout.EndVertical();
    GUILayout.FlexibleSpace();

    if (GUILayout.Button("Export Models From Range", GUILayout.Width(170), GUILayout.Height(34)))
    {
        SavePrefs();
        ExportModelIdsFromManualRange();
    }

    EditorGUILayout.EndHorizontal();

    GUILayout.Space(6);

    EditorGUILayout.BeginHorizontal();
    EditorGUILayout.BeginVertical();

    EditorGUILayout.LabelField("Models From Txt File", EditorStyles.boldLabel);
    EditorGUILayout.LabelField("Uses only model IDs/ranges found in the txt file.", EditorStyles.miniLabel);

    EditorGUILayout.EndVertical();
    GUILayout.FlexibleSpace();

    if (GUILayout.Button("Export Models From Txt", GUILayout.Width(170), GUILayout.Height(34)))
    {
        SavePrefs();
        ExportModelIdsFromTxtOnly();
    }

    EditorGUILayout.EndHorizontal();

    GUILayout.Space(4);

    EditorGUILayout.LabelField("(Optional) Model Txt File Path", EditorStyles.boldLabel);
    EditorGUILayout.BeginHorizontal();
    singleModelTxtPath = EditorGUILayout.TextField(singleModelTxtPath ?? string.Empty);
    if (GUILayout.Button("Browse...", GUILayout.Width(90)))
    {
        string chosen = EditorUtility.OpenFilePanel("Choose model txt file", GetReasonableStartingFolder(singleModelTxtPath), "txt");
        if (!string.IsNullOrEmpty(chosen))
            singleModelTxtPath = chosen;
    }
    EditorGUILayout.EndHorizontal();

    GUILayout.Space(14);
    EditorGUILayout.LabelField("Flats FBX", EditorStyles.boldLabel);
    DrawGroupedFamilyToggleShared();
    GUILayout.Space(4);

    EditorGUILayout.BeginHorizontal();
    EditorGUILayout.BeginVertical();

    EditorGUILayout.LabelField("Single Flat Archive.Record", EditorStyles.boldLabel);
    singleFlatIdText = EditorGUILayout.TextField(singleFlatIdText ?? "210.2");
    EditorGUILayout.LabelField("Examples: 210.2, 254.26, 182.30", EditorStyles.miniLabel);

    EditorGUILayout.EndVertical();
    GUILayout.FlexibleSpace();

    if (GUILayout.Button("Export Single Flat FBX", GUILayout.Width(170), GUILayout.Height(34)))
    {
        SavePrefs();
        ExportSingleConfiguredFlat();
    }

    EditorGUILayout.EndHorizontal();

    GUILayout.Space(6);

    EditorGUILayout.BeginHorizontal();
    EditorGUILayout.BeginVertical();

    EditorGUILayout.LabelField("Flat Archive.Record List", EditorStyles.boldLabel);
    singleFlatBatchRangeText = EditorGUILayout.TextField(singleFlatBatchRangeText ?? string.Empty);
    EditorGUILayout.LabelField("Examples: 210.2,254.26,182.30", EditorStyles.miniLabel);

    EditorGUILayout.EndVertical();
    GUILayout.FlexibleSpace();

    if (GUILayout.Button("Export Flats From List", GUILayout.Width(170), GUILayout.Height(34)))
    {
        SavePrefs();
        ExportFlatIdsFromManualRange();
    }

    EditorGUILayout.EndHorizontal();

    GUILayout.Space(6);

    EditorGUILayout.BeginHorizontal();
    EditorGUILayout.BeginVertical();

    EditorGUILayout.LabelField("Flats From Txt File", EditorStyles.boldLabel);
    EditorGUILayout.LabelField("Uses only flat archive.record entries found in the txt file.", EditorStyles.miniLabel);

    EditorGUILayout.EndVertical();
    GUILayout.FlexibleSpace();

    if (GUILayout.Button("Export Flats From Txt", GUILayout.Width(170), GUILayout.Height(34)))
    {
        SavePrefs();
        ExportFlatIdsFromTxtOnly();
    }

    EditorGUILayout.EndHorizontal();

    GUILayout.Space(4);

    EditorGUILayout.LabelField("(Optional) Flat Txt File Path", EditorStyles.boldLabel);
    EditorGUILayout.BeginHorizontal();
    singleFlatTxtPath = EditorGUILayout.TextField(singleFlatTxtPath ?? string.Empty);
    if (GUILayout.Button("Browse...", GUILayout.Width(90)))
    {
        string chosen = EditorUtility.OpenFilePanel("Choose flat txt file", GetReasonableStartingFolder(singleFlatTxtPath), "txt");
        if (!string.IsNullOrEmpty(chosen))
            singleFlatTxtPath = chosen;
    }
    EditorGUILayout.EndHorizontal();
}

private void DrawBatchExportTab()
{
    EditorGUILayout.LabelField("Category / Group FBX Export", EditorStyles.boldLabel);
    GUILayout.Space(6);

    DrawCategoryTabIntro();

    categoryBatchExpanded = EditorGUILayout.Foldout(
        categoryBatchExpanded,
        "Model Categories",
        true);

    if (categoryBatchExpanded)
    {
        EditorGUI.indentLevel++;

        DrawGroupedFamilyToggleShared();

        categoryBatchFbxSubfolders = EditorGUILayout.ToggleLeft(
            "Put category batch FBX files into category subfolders",
            categoryBatchFbxSubfolders);

        GUILayout.Space(6);
        EditorGUILayout.LabelField("Model Categories", EditorStyles.boldLabel);

        EnsureAtLeastOneCategoryBatchRow();

        for (int i = 0; i < selectedCategoryBatchSources.Count; i++)
        {
            string fieldName = selectedCategoryBatchSources[i];
            int selectedIndex = Array.IndexOf(FixedCategoryBatchSources, fieldName);
            if (selectedIndex < 0)
                selectedIndex = 0;

            EditorGUILayout.BeginHorizontal();

            int nextIndex = EditorGUILayout.Popup(selectedIndex, FixedCategoryBatchSources);
            if (nextIndex >= 0 && nextIndex < FixedCategoryBatchSources.Length)
                selectedCategoryBatchSources[i] = FixedCategoryBatchSources[nextIndex];

            string selectedFieldName = selectedCategoryBatchSources[i];
            int baseCount = GetCategoryBaseModelCount(selectedFieldName);
            int groupedCount = GetCategoryGroupedModelCount(selectedFieldName);
            string durationText = GetLastDurationText(selectedFieldName);

            string rowInfo = "[" + baseCount.ToString(CultureInfo.InvariantCulture) + "]";
            if (exportGroupedFamilyMembers)
                rowInfo += " grouped: " + groupedCount.ToString(CultureInfo.InvariantCulture);
            rowInfo += " last: " + durationText;

            EditorGUILayout.LabelField(rowInfo, EditorStyles.miniLabel, GUILayout.Width(230));

            GUI.enabled = selectedCategoryBatchSources.Count > 1;
            if (GUILayout.Button("-", GUILayout.Width(28)))
            {
                selectedCategoryBatchSources.RemoveAt(i);
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();
                break;
            }
            GUI.enabled = true;

            if (GUILayout.Button("+", GUILayout.Width(28)))
            {
                selectedCategoryBatchSources.Insert(i + 1, FixedCategoryBatchSources[0]);
                EditorGUILayout.EndHorizontal();
                break;
            }

            EditorGUILayout.EndHorizontal();
        }

        GUILayout.Space(4);

        bool selectedModelsHasMissingEstimate;
        double selectedModelsEstimatedSeconds = GetSelectedCategoryEstimatedSeconds(out selectedModelsHasMissingEstimate);

        EditorGUILayout.LabelField(
            "Selected Models ETA: " + FormatDurationSecondsWithPlus(selectedModelsEstimatedSeconds, selectedModelsHasMissingEstimate),
            EditorStyles.miniBoldLabel);

        if (selectedModelsHasMissingEstimate)
            EditorGUILayout.LabelField("+ means one or more selected categories have no saved timing yet.", EditorStyles.miniLabel);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Export Selected Model Categories", GUILayout.Height(24), GUILayout.ExpandWidth(false)))
        {
            if (ConfirmBulkExportWithEstimate(
                "Export Selected Model Categories",
                "Export the currently selected model categories?",
                selectedModelsEstimatedSeconds,
                selectedModelsHasMissingEstimate,
                true))
            {
                SavePrefs();
                ExportSelectedCategoryBatches();
            }
        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(8);

        bool allModelsHasMissingEstimate;
        double totalEstimatedSeconds = GetAllCategoryEstimatedSeconds(out allModelsHasMissingEstimate);
        string totalEstimatedText = FormatDurationSecondsWithPlus(totalEstimatedSeconds, allModelsHasMissingEstimate);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(
            "Export ALL Models: ETA " + totalEstimatedText,
            EditorStyles.miniBoldLabel,
            GUILayout.Width(280));

        if (GUILayout.Button("Export ALL Models", GUILayout.Width(140), GUILayout.Height(28)))
        {
            if (ConfirmBulkExportWithEstimate(
                "Export ALL Model Categories",
                "Export all model categories?",
                totalEstimatedSeconds,
                allModelsHasMissingEstimate,
                true))
            {
                SavePrefs();
                ExportAllFixedCategories();
            }
        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        if (allModelsHasMissingEstimate)
            EditorGUILayout.LabelField("+ means one or more model categories have no saved timing yet.", EditorStyles.miniLabel);

        GUILayout.Space(12);
        flatCategoryBatchExpanded = EditorGUILayout.Foldout(flatCategoryBatchExpanded, "Flat Categories", true);
        if (flatCategoryBatchExpanded)
        {
            EditorGUI.indentLevel++;

            DrawGroupedFamilyToggleShared();

            EnsureAtLeastOneFlatCategoryBatchRow();
            string[] flatSources = GetAvailableFlatCategoryBatchSources();

            if (flatSources.Length == 0)
            {
                EditorGUILayout.HelpBox(
                    "No flat category fields with archive.record entries were found in WorldDataEditorObjectData.",
                    MessageType.Warning);
            }
            else
            {
                for (int i = 0; i < selectedFlatCategoryBatchSources.Count; i++)
                {
                    string fieldName = selectedFlatCategoryBatchSources[i];
                    int selectedIndex = Array.IndexOf(flatSources, fieldName);
                    if (selectedIndex < 0)
                        selectedIndex = 0;

                    EditorGUILayout.BeginHorizontal();

                    int nextIndex = EditorGUILayout.Popup(selectedIndex, flatSources);
                    if (nextIndex >= 0 && nextIndex < flatSources.Length)
                        selectedFlatCategoryBatchSources[i] = flatSources[nextIndex];

                    string selectedFieldName = selectedFlatCategoryBatchSources[i];
                    int baseCount = GetFlatCategoryBaseCount(selectedFieldName);
                    int groupedCount = GetFlatCategoryGroupedCount(selectedFieldName);
                    string durationText = GetFlatLastDurationText(selectedFieldName);

                    string rowInfo = "[" + baseCount.ToString(CultureInfo.InvariantCulture) + "]";
                    if (exportGroupedFamilyMembers)
                        rowInfo += " grouped: " + groupedCount.ToString(CultureInfo.InvariantCulture);
                    rowInfo += " last: " + durationText;

                    EditorGUILayout.LabelField(rowInfo, EditorStyles.miniLabel, GUILayout.Width(230));

                    GUI.enabled = selectedFlatCategoryBatchSources.Count > 1;
                    if (GUILayout.Button("-", GUILayout.Width(28)))
                    {
                        selectedFlatCategoryBatchSources.RemoveAt(i);
                        GUI.enabled = true;
                        EditorGUILayout.EndHorizontal();
                        break;
                    }
                    GUI.enabled = true;

                    if (GUILayout.Button("+", GUILayout.Width(28)))
                    {
                        selectedFlatCategoryBatchSources.Insert(i + 1, flatSources[0]);
                        EditorGUILayout.EndHorizontal();
                        break;
                    }

                    EditorGUILayout.EndHorizontal();
                }

                GUILayout.Space(4);

                bool selectedFlatsHasMissingEstimate;
                double selectedFlatsEstimatedSeconds = GetSelectedFlatCategoryEstimatedSeconds(out selectedFlatsHasMissingEstimate);

                EditorGUILayout.LabelField(
                    "Selected Flats ETA: " + FormatDurationSecondsWithPlus(selectedFlatsEstimatedSeconds, selectedFlatsHasMissingEstimate),
                    EditorStyles.miniBoldLabel);

                if (selectedFlatsHasMissingEstimate)
                    EditorGUILayout.LabelField("+ means one or more selected flat categories have no saved timing yet.", EditorStyles.miniLabel);

                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Export Selected Flat Categories", GUILayout.Height(24), GUILayout.ExpandWidth(false)))
                {
                    if (ConfirmBulkExportWithEstimate(
                        "Export Selected Flat Categories",
                        "Export the currently selected flat categories?",
                        selectedFlatsEstimatedSeconds,
                        selectedFlatsHasMissingEstimate,
                        true))
                    {
                        SavePrefs();
                        ExportSelectedFlatCategoryBatches();
                    }
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(8);

                bool allFlatsHasMissingEstimate;
                double totalFlatEstimatedSeconds = GetAllFlatCategoryEstimatedSeconds(out allFlatsHasMissingEstimate);
                string totalFlatEstimatedText = FormatDurationSecondsWithPlus(totalFlatEstimatedSeconds, allFlatsHasMissingEstimate);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(
                    "Export ALL Flats: ETA " + totalFlatEstimatedText,
                    EditorStyles.miniBoldLabel,
                    GUILayout.Width(280));

                if (GUILayout.Button("Export ALL Flats", GUILayout.Width(140), GUILayout.Height(28)))
                {
                    if (ConfirmBulkExportWithEstimate(
                        "Export ALL Flat Categories",
                        "Export all flat categories?",
                        totalFlatEstimatedSeconds,
                        allFlatsHasMissingEstimate,
                        true))
                    {
                        SavePrefs();
                        ExportAllFlatCategories();
                    }
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                if (allFlatsHasMissingEstimate)
                    EditorGUILayout.LabelField("+ means one or more flat categories have no saved timing yet.", EditorStyles.miniLabel);
            }

            EditorGUI.indentLevel--;
        }

        EditorGUI.indentLevel--;
    }
}

private void DrawSettingsPathsTab()
{
    EditorGUILayout.LabelField("Settings / Paths", EditorStyles.boldLabel);
    GUILayout.Space(6);

    DrawSettingsTabIntro();

    deleteTempTextureFilesAfterExport = EditorGUILayout.ToggleLeft(
        "Delete extracted temp individual texture files after export",
        deleteTempTextureFilesAfterExport);

    overwriteExistingExportAssets = EditorGUILayout.ToggleLeft(
        "Overwrite previously-exportedFBX, material, and texture if it exists",
        overwriteExistingExportAssets);

    GUILayout.Space(6);

    EditorGUILayout.LabelField("Export Scale Multiplier", EditorStyles.boldLabel);
    exportScaleMultiplier = EditorGUILayout.FloatField(exportScaleMultiplier);
    if (exportScaleMultiplier <= 0f)
        exportScaleMultiplier = 1f;

    GUILayout.Space(6);

    atlasBatchExportedMats = EditorGUILayout.ToggleLeft(
        "Atlas Batch Exported Mats",
        atlasBatchExportedMats);

    if (atlasBatchExportedMats)
    {
        EditorGUILayout.LabelField("Batch Atlas Max Size", EditorStyles.boldLabel);
        batchAtlasMaxSize = EditorGUILayout.IntField(batchAtlasMaxSize);

        if (batchAtlasMaxSize < 256) batchAtlasMaxSize = 256;
        if (batchAtlasMaxSize > 8192) batchAtlasMaxSize = 8192;
    }

    GUILayout.Space(12);

    EditorGUILayout.BeginHorizontal();

    if (GUILayout.Button("Save Settings", GUILayout.Height(30), GUILayout.ExpandWidth(false)))
    {
        SavePrefs();
        Debug.Log("[NexusDev][DFU Export] Settings saved.");
    }

    GUILayout.Space(8);

    if (GUILayout.Button("Open Export Folder", GUILayout.Height(30), GUILayout.ExpandWidth(false)))
    {
        OpenExportFolder();
    }

    GUILayout.FlexibleSpace();
    EditorGUILayout.EndHorizontal();
}

private void DrawDebugTab()
{
    EditorGUILayout.LabelField("Debug / Cleanup", EditorStyles.boldLabel);
    GUILayout.Space(6);

    DrawDebugTabIntro();

    debugDeleteGroupIndex = EditorGUILayout.Popup(
        "Delete Exported Files For This Model Group",
        debugDeleteGroupIndex,
        FixedCategoryBatchSources);

    if (debugDeleteGroupIndex < 0)
        debugDeleteGroupIndex = 0;
    if (debugDeleteGroupIndex >= FixedCategoryBatchSources.Length)
        debugDeleteGroupIndex = FixedCategoryBatchSources.Length - 1;

    if (GUILayout.Button("Delete Exported Files For Above Selected Model Group", GUILayout.Height(24), GUILayout.ExpandWidth(false)))
    {
        string fieldName = FixedCategoryBatchSources[debugDeleteGroupIndex];
        bool confirmed = EditorUtility.DisplayDialog(
            "Delete Exported Model Files",
            "Delete previously-exported model FBX files, atlases, and materials for group '" + fieldName + "'?\n\nThis cannot be undone.",
            "Delete",
            "Cancel");

        if (confirmed)
            DeleteExportedFilesForGroup(fieldName);
    }

    GUILayout.Space(8);

    string[] flatSources = GetAvailableFlatCategoryBatchSources();
    if (flatSources.Length > 0)
    {
        debugDeleteFlatGroupIndex = EditorGUILayout.Popup(
            "Delete Exported Files For This Flat Group",
            Mathf.Clamp(debugDeleteFlatGroupIndex, 0, flatSources.Length - 1),
            flatSources);

        if (debugDeleteFlatGroupIndex < 0)
            debugDeleteFlatGroupIndex = 0;
        if (debugDeleteFlatGroupIndex >= flatSources.Length)
            debugDeleteFlatGroupIndex = flatSources.Length - 1;

        if (GUILayout.Button("Delete Exported Files For Above Selected Flats Group", GUILayout.Height(24), GUILayout.ExpandWidth(false)))
        {
            string fieldName = flatSources[debugDeleteFlatGroupIndex];
            bool confirmed = EditorUtility.DisplayDialog(
                "Delete Exported Flat Files",
                "Delete previously-exported flat FBX files, atlases, and materials for flat group '" + fieldName + "'?\n\nThis cannot be undone.",
                "Delete",
                "Cancel");

            if (confirmed)
                DeleteExportedFilesForFlatGroup(fieldName);
        }
    }

    GUILayout.Space(8);

    if (GUILayout.Button("Delete ALL Previously-Exported Model and Flat FBX Assets", GUILayout.Height(24), GUILayout.ExpandWidth(false)))
    {
        bool confirmed = EditorUtility.DisplayDialog(
            "Delete ALL Exported Assets",
            "Delete ALL exported FBX files, atlas textures, materials, and temp export folders?\n\nThis cannot be undone.",
            "Delete All",
            "Cancel");

        if (confirmed)
            DeleteAllExportedAssets();
    }

    GUILayout.Space(8);

    if (GUILayout.Button("Export Saved Duration Estimates To TXT", GUILayout.Height(24), GUILayout.ExpandWidth(false)))
    {
        ExportStoredDurationDebugReport();
    }

    GUILayout.Space(8);

    if (GUILayout.Button("Delete ALL Previously-Exported MobileSprites", GUILayout.Height(24), GUILayout.ExpandWidth(false)))
    {
        bool confirmed = EditorUtility.DisplayDialog(
            "Delete ALL MobileSprites",
            "Delete the entire previously-exported MobileSprites folder contents?\n\nThis will remove all TownNPC and Enemy mobile PNGs and JSON manifests.\n\nThis cannot be undone.",
            "Delete All MobileSprites",
            "Cancel");

        if (confirmed)
            DeleteAllExportedMobileSprites();
    }
}

private void DrawInfoTab()
{
    EditorGUILayout.LabelField("Readme / Info", EditorStyles.boldLabel);
    GUILayout.Space(6);

    DrawInfoTabIntro();

    InfoDocRow[] rows = new InfoDocRow[]
    {
        InfoDocRow.MakeHeader("Model geometry source"),
        InfoDocRow.MakeScript(
            "GameObjectHelper.cs - creates the DFU model GameObject used as the source export object.",
            "GameObjectHelper"),
        InfoDocRow.MakeScript(
            "MeshReader.cs - gets model data, including DF mesh/submesh texture archive and record info.",
            "MeshReader"),

        InfoDocRow.MakeHeader("Flat geometry source"),
        InfoDocRow.MakeScript(
            "DaggerfallBillboard.cs - builds flat billboard mesh/material and AlignToBase placement.",
            "DaggerfallBillboard"),

        InfoDocRow.MakeHeader("Category and naming source"),
        InfoDocRow.MakeScript(
            "WorldDataEditorObjectData.cs - supplies model categories, flat-capable categories, modelGroups, and flatGroups.",
            "WorldDataEditorObjectData"),

        InfoDocRow.MakeHeader("Texture source"),
        InfoDocRow.MakeScript(
            "TextureReader.cs - reads DFU textures from Arena2 for direct export texture creation.",
            "TextureReader"),
        InfoDocRow.MakeScript(
            "TextureFile.cs - reads Arena2 texture archives and frames for mobile sprite export.",
            "TextureFile"),

        InfoDocRow.MakeHeader("Material naming / archive-record parsing"),
        InfoDocRow.MakeScript(
            "ExportDfuModelsWest.cs - parses names like TEXTURE_069_Index_3_ and TEXTURE.112 [Index=1] when resolving texture identity.",
            "ExportDfuModelsWest"),

        InfoDocRow.MakeHeader("Mobile sprite source"),
        InfoDocRow.MakeScript(
            "EnemyBasics.cs - supplies enemy definitions and animation arrays for enemy/class mobile exports.",
            "EnemyBasics"),
        InfoDocRow.MakeScript(
            "TextureFile.cs - extracts animation frames from Arena2 texture archives for mobile sprite PNG export.",
            "TextureFile"),
        InfoDocRow.MakeScript(
            "ExportDfuModelsWest.cs - contains the Town NPC archive mappings used by this exporter.",
            "ExportDfuModelsWest"),

        InfoDocRow.MakeHeader("Name cleanup / final export naming"),
        InfoDocRow.MakeScript(
            "ExportDfuModelsWest.cs - applies replacement tables, overrides, and category-based naming cleanup.",
            "ExportDfuModelsWest"),

        InfoDocRow.MakeHeader("Shared-atlas batch export"),
        InfoDocRow.MakeScript(
            "ExportDfuModelsWest.cs - packs readable DFU textures into atlas pages and remaps mesh UVs/materials.",
            "ExportDfuModelsWest"),

        InfoDocRow.MakeHeader("Useful external inspection tools"),
        InfoDocRow.MakeScript(
            "RMB and Building viewers - useful for inspecting IDs and source assets while validating exports.",
            "RMB"),
        InfoDocRow.MakeScript(
            "Daggerfall Modeling - useful for external inspection of source models and IDs.",
            "Daggerfall Modeling"),
    };

    string fullReadmeText = BuildInfoReadmeText(rows);

    if (GUILayout.Button(
        "Copy this entire readme",
        GUILayout.Width(180),
        GUILayout.Height(24)))
    {
        EditorGUIUtility.systemCopyBuffer = fullReadmeText;
    }

    GUILayout.Space(10);

    EditorGUILayout.HelpBox(
        "Version 7, April 2, 2026. Made by westingtyler. Requires Unity FBX Exporter package. Tested in Unity 2019.4.40f1 with Daggerfall Unity 1.1.1. Exported FBX files import directly into Blender 4.5.7 LTS.",
        MessageType.None);

    GUILayout.Space(10);

    for (int i = 0; i < rows.Length; i++)
    {
        if (rows[i].RowType == InfoDocRowType.Header)
        {
            EditorGUILayout.LabelField(rows[i].HeaderText, EditorStyles.boldLabel);
            GUILayout.Space(4);
        }
        else
        {
            string displayText = rows[i].ScriptEntry.DisplayText ?? string.Empty;
            string copyText = string.IsNullOrWhiteSpace(rows[i].ScriptEntry.CopyScriptName)
                ? ResolveScriptNameForCopy(displayText)
                : rows[i].ScriptEntry.CopyScriptName.Trim();

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(displayText, EditorStyles.wordWrappedLabel);
            GUILayout.Space(2);

            if (GUILayout.Button("Copy script name", GUILayout.Width(130), GUILayout.Height(22)))
                EditorGUIUtility.systemCopyBuffer = copyText;

            EditorGUILayout.EndVertical();
            GUILayout.Space(10);
        }
    }

    GUILayout.Space(6);
    DrawBlenderImportScriptFoldout();
}


private void DrawInfoSection(string header, string body)
{
    if (!string.IsNullOrWhiteSpace(header))
        EditorGUILayout.LabelField(header + ":", EditorStyles.boldLabel);

    EditorGUILayout.SelectableLabel(
        body ?? string.Empty,
        EditorStyles.wordWrappedLabel,
        GUILayout.MinHeight(GetSelectableTextHeight(body)));

    GUILayout.Space(10);
}
private void DrawScriptNameSection(string header, string[] scriptNames)
{
    if (scriptNames == null || scriptNames.Length == 0)
    {
        if (!string.IsNullOrWhiteSpace(header))
            EditorGUILayout.LabelField(header, EditorStyles.boldLabel);

        GUILayout.Space(10);
        return;
    }

    ScriptDocEntry[] entries = new ScriptDocEntry[scriptNames.Length];
    for (int i = 0; i < scriptNames.Length; i++)
    {
        string displayText = scriptNames[i] ?? string.Empty;
        entries[i] = new ScriptDocEntry(displayText, ResolveScriptNameForCopy(displayText));
    }

    DrawScriptNameSection(header, entries);
}

private void DrawScriptNameSection(string header, ScriptDocEntry[] entries)
{
    if (!string.IsNullOrWhiteSpace(header))
    {
        EditorGUILayout.LabelField(header, EditorStyles.boldLabel);
        GUILayout.Space(4);
    }

    if (entries == null || entries.Length == 0)
    {
        GUILayout.Space(8);
        return;
    }

    for (int i = 0; i < entries.Length; i++)
    {
        string displayText = entries[i].DisplayText ?? string.Empty;
        string copyText = string.IsNullOrWhiteSpace(entries[i].CopyScriptName)
            ? ResolveScriptNameForCopy(displayText)
            : entries[i].CopyScriptName.Trim();

        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField(
            displayText,
            EditorStyles.wordWrappedLabel);

        GUILayout.Space(2);

        if (GUILayout.Button("Copy script name", GUILayout.Width(130), GUILayout.Height(22)))
            EditorGUIUtility.systemCopyBuffer = copyText;

        EditorGUILayout.EndVertical();
        GUILayout.Space(10);
    }
}
private float GetSelectableTextHeight(string text)
{
    string safe = string.IsNullOrEmpty(text) ? " " : text;
    GUIStyle style = EditorStyles.wordWrappedLabel;
    float viewWidth = position.width - 150f;
    if (viewWidth < 200f)
        viewWidth = 200f;

    float height = style.CalcHeight(new GUIContent(safe), viewWidth);
    if (height < EditorGUIUtility.singleLineHeight + 4f)
        height = EditorGUIUtility.singleLineHeight + 4f;

    return height + 4f;
}

private void OnGUI()
{
    windowScroll = EditorGUILayout.BeginScrollView(windowScroll);

    GUILayout.Space(6);
    topTabIndex = GUILayout.Toolbar(topTabIndex, TopTabLabels);
    GUILayout.Space(8);

    switch (topTabIndex)
    {
        case 0:
            DrawSingleExportTab();
            break;

        case 1:
            DrawBatchExportTab();
            break;

        case 2:
            DrawMobileSpritesTab();
            break;

        case 3:
            DrawSettingsPathsTab();
            break;

        case 4:
            DrawDebugTab();
            break;

        case 5:
            DrawInfoTab();
            break;

        default:
            DrawSingleExportTab();
            break;
    }

    EditorGUILayout.EndScrollView();
}
private void DrawMobileSpritesTab()
{
    EditorGUILayout.LabelField("Mobile Sprite Export", EditorStyles.boldLabel);
    EditorGUILayout.HelpBox(
        "Exports organized PNG frame sets plus one JSON manifest per entity folder. Town NPCs use semantic archive mappings. Enemies and enemy classes are driven from EnemyBasics.",
        MessageType.None);
    GUILayout.Space(6);

    EditorGUILayout.LabelField("Post-Export PNG Import Settings", EditorStyles.boldLabel);
    mobileSpriteApplyImportSettingsAfterExport = EditorGUILayout.ToggleLeft(
        "Apply import settings after export finishes",
        mobileSpriteApplyImportSettingsAfterExport);

    mobileSpriteImportUsePointFilter = EditorGUILayout.ToggleLeft(
        "Use Point/Nearest filter mode",
        mobileSpriteImportUsePointFilter);

    mobileSpriteImportMaxTextureSize = EditorGUILayout.IntField(
        "Max Texture Size",
        mobileSpriteImportMaxTextureSize);

    if (mobileSpriteImportMaxTextureSize < 32)
        mobileSpriteImportMaxTextureSize = 32;
    if (mobileSpriteImportMaxTextureSize > 8192)
        mobileSpriteImportMaxTextureSize = 8192;

    bool fullMobileHasMissingEstimate;
    double fullMobileEstimatedSeconds = GetTotalEstimatedMobileExportSeconds(out fullMobileHasMissingEstimate);

    GUILayout.Space(6);
    EditorGUILayout.BeginHorizontal();

    if (GUILayout.Button("Open MobileSprites Folder", GUILayout.Height(24), GUILayout.ExpandWidth(false)))
    {
        string abs = GetAbsolutePathFromAssetPath(MobileSpritesFolderAssetRelative);
        if (!string.IsNullOrEmpty(abs))
            EditorUtility.RevealInFinder(abs);
    }

    GUILayout.Space(8);

    if (GUILayout.Button("Set Import Settings For Exported MobileSprites", GUILayout.Height(24), GUILayout.ExpandWidth(false)))
    {
        double importEstimateSeconds;
        bool hasImportEstimate = false;
        importEstimateSeconds = GetMobileEstimatedSeconds("ImportSettingsAll", out hasImportEstimate);

        if (ConfirmBulkExportWithEstimate(
            "Apply MobileSprites Import Settings",
            "Apply import settings to all exported MobileSprites PNGs?",
            importEstimateSeconds,
            !hasImportEstimate,
            false))
        {
            ApplyImportSettingsToAllExportedMobileSprites();
        }
    }

    GUILayout.Space(10);
    EditorGUILayout.LabelField("Last: " + GetMobileLastDurationText("ImportSettingsAll"), EditorStyles.miniLabel, GUILayout.Width(140));
    GUILayout.FlexibleSpace();
    EditorGUILayout.EndHorizontal();

    GUILayout.Space(4);
    EditorGUILayout.LabelField(
        "Export ALL MobileSprites: ETA " + FormatDurationSecondsWithPlus(fullMobileEstimatedSeconds, fullMobileHasMissingEstimate),
        EditorStyles.miniBoldLabel);

    if (fullMobileHasMissingEstimate)
        EditorGUILayout.LabelField("+ means one or more mobile export parts have no saved timing yet.", EditorStyles.miniLabel);

    EditorGUILayout.BeginHorizontal();

    if (GUILayout.Button("Export ALL MobileSprites Full Sets", GUILayout.Height(24), GUILayout.ExpandWidth(false)))
    {
        if (ConfirmBulkExportWithEstimate(
            "Export ALL MobileSprites",
            "Export all relevant MobileSprites in one pass?\n\nThis will export:\n- ALL Town NPC sets\n- Guard set\n- ALL Enemy/Class sets\n\nExisting files with matching names will be overwritten if your overwrite toggle allows it.",
            fullMobileEstimatedSeconds,
            fullMobileHasMissingEstimate,
            true))
        {
            ExportAllMobileSpritesFullSets();
        }
    }

    GUILayout.Space(8);

    if (GUILayout.Button("Clear/Delete ALL Previously-Exported MobileSprites", GUILayout.Height(24), GUILayout.ExpandWidth(false)))
    {
        bool confirmed = EditorUtility.DisplayDialog(
            "Delete ALL MobileSprites",
            "Delete the entire previously-exported MobileSprites folder contents?\n\nThis will remove all TownNPC and Enemy mobile PNGs and JSON manifests.\n\nThis cannot be undone.",
            "Delete All MobileSprites",
            "Cancel");

        if (confirmed)
            DeleteAllExportedMobileSprites();
    }

    GUILayout.FlexibleSpace();
    EditorGUILayout.EndHorizontal();

    GUILayout.Space(10);
    EditorGUILayout.LabelField("Town NPC Set", EditorStyles.boldLabel);

    List<TownNpcMobileListEntry> townNpcEntries = GetTownNpcMobileEntriesCached();
    string[] townNpcLabels = GetTownNpcMobileOptionLabels();

    if (townNpcLabels.Length == 0)
    {
        EditorGUILayout.HelpBox("Town NPC mobile list returned no entries.", MessageType.Warning);
    }
    else
    {
        int sortedTownNpcIndex = GetSortedTownNpcPopupIndexFromCurrentSelection(townNpcEntries);
        sortedTownNpcIndex = Mathf.Clamp(sortedTownNpcIndex, 0, townNpcLabels.Length - 1);
        sortedTownNpcIndex = EditorGUILayout.Popup("Town NPC", sortedTownNpcIndex, townNpcLabels);
        ApplySelectedTownNpcEntry(townNpcEntries, sortedTownNpcIndex);

        mobileTownNpcIncludeMove = EditorGUILayout.ToggleLeft("Include Move", mobileTownNpcIncludeMove);
        mobileTownNpcIncludeIdle = EditorGUILayout.ToggleLeft("Include Idle", mobileTownNpcIncludeIdle);

        string previewEntityName = BuildTownNpcEntityName(
            mobileTownNpcRace,
            mobileTownNpcGender,
            mobileTownNpcVariant,
            mobileTownNpcIsGuard);

        EditorGUILayout.LabelField(
            "Folder Preview",
            MobileTownNpcFolderAssetRelative + "/" + previewEntityName + "/",
            EditorStyles.miniLabel);

        GUILayout.Space(8);

        double townNpcCurrentEstimateSeconds;
        bool hasTownNpcCurrentEstimate;
        townNpcCurrentEstimateSeconds = GetMobileEstimatedSeconds("TownNpcCurrent", out hasTownNpcCurrentEstimate);

        double townNpcAllEstimateSeconds;
        bool hasTownNpcAllEstimate;
        townNpcAllEstimateSeconds = GetMobileEstimatedSeconds("TownNpcAll", out hasTownNpcAllEstimate);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Export Current Town NPC Set (One Character)", GUILayout.Height(28), GUILayout.ExpandWidth(false)))
        {
            if (ConfirmBulkExportWithEstimate(
                "Export Current Town NPC Set",
                "Export the current Town NPC mobile sprite set?",
                townNpcCurrentEstimateSeconds,
                !hasTownNpcCurrentEstimate,
                false))
            {
                ExportCurrentTownNpcMobileSet();
            }
        }

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Last: " + GetMobileLastDurationText("TownNpcCurrent"), EditorStyles.miniLabel, GUILayout.Width(130));
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Export ALL Town NPC Sets (All Town Characters)", GUILayout.Height(28), GUILayout.ExpandWidth(false)))
        {
            if (ConfirmBulkExportWithEstimate(
                "Export ALL Town NPC Sets",
                "Export all Town NPC mobile sprite sets?",
                townNpcAllEstimateSeconds,
                !hasTownNpcAllEstimate,
                true))
            {
                ExportAllTownNpcMobileSets();
            }
        }

        GUILayout.Space(10);
        EditorGUILayout.LabelField("ETA/Last: " + FormatDurationSecondsWithPlus(townNpcAllEstimateSeconds, !hasTownNpcAllEstimate), EditorStyles.miniLabel, GUILayout.Width(150));
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(4);
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Delete Current Town NPC Set", GUILayout.Height(24), GUILayout.ExpandWidth(false)))
        {
            bool confirmed = EditorUtility.DisplayDialog(
                "Delete Previous Export of Currently-Selected Town NPC Set",
                "Delete previous export of the currently selected Town NPC mobile sprite folder?\n\n" +
                previewEntityName + "\n\n" +
                "This cannot be undone.",
                "elete previous export of the Current Town NPC Set",
                "Cancel");

            if (confirmed)
                DeleteCurrentTownNpcMobileSet();
        }

        GUILayout.Space(8);

        if (GUILayout.Button("Delete Previous Exports of ALL Town NPC Sets", GUILayout.Height(24), GUILayout.ExpandWidth(false)))
        {
            bool confirmed = EditorUtility.DisplayDialog(
                "Delete Previous Exports of ALL Town NPC Sets",
                "Delete all previously-exported Town NPC mobile sprite folders and manifests?\n\nThis cannot be undone.",
                "Delete All Town NPC Sets",
                "Cancel");

            if (confirmed)
                DeleteAllTownNpcMobileSprites();
        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    GUILayout.Space(14);

    mobileSpriteSortMode = EditorGUILayout.Popup(
        "Sort Mobile Lists By",
        Mathf.Clamp(mobileSpriteSortMode, 0, 1),
        new[] { "Name", "ID Number" });

    EditorGUILayout.LabelField("Enemy / Enemy Class Set", EditorStyles.boldLabel);
    List<EnemyMobileListEntry> enemyEntries = GetEnemyMobileEntriesCached();
    string[] enemyLabels = GetEnemyMobileOptionLabels();

    if (enemyLabels.Length == 0)
    {
        EditorGUILayout.HelpBox("EnemyBasics returned no mobile entries.", MessageType.Warning);
    }
    else
    {
        int sortedPopupIndex = GetSortedEnemyPopupIndexFromOriginalIndex(enemyEntries, mobileEnemySelectionIndex);
        sortedPopupIndex = Mathf.Clamp(sortedPopupIndex, 0, enemyLabels.Length - 1);
        sortedPopupIndex = EditorGUILayout.Popup("Enemy", sortedPopupIndex, enemyLabels);
        mobileEnemySelectionIndex = GetOriginalEnemyIndexFromSortedPopupIndex(enemyEntries, sortedPopupIndex);

        mobileEnemyIncludeMove = EditorGUILayout.ToggleLeft("Include Move", mobileEnemyIncludeMove);
        mobileEnemyIncludeIdle = EditorGUILayout.ToggleLeft("Include Idle", mobileEnemyIncludeIdle);
        mobileEnemyIncludePrimaryAttack = EditorGUILayout.ToggleLeft("Include Primary Attack", mobileEnemyIncludePrimaryAttack);
        mobileEnemyIncludeHurt = EditorGUILayout.ToggleLeft("Include Hurt", mobileEnemyIncludeHurt);
        mobileEnemyIncludeRangedAttack1 = EditorGUILayout.ToggleLeft("Include Ranged Attack 1", mobileEnemyIncludeRangedAttack1);
        mobileEnemyIncludeRangedAttack2 = EditorGUILayout.ToggleLeft("Include Ranged Attack 2", mobileEnemyIncludeRangedAttack2);

        string previewEnemyFolderLabel = GetEnemyMobilePreviewFolderLabel(mobileEnemySelectionIndex);

        EditorGUILayout.LabelField(
            "Folder Preview",
            MobileEnemyFolderAssetRelative + "/" + previewEnemyFolderLabel + "/",
            EditorStyles.miniLabel);

        GUILayout.Space(8);

        double enemyCurrentEstimateSeconds;
        bool hasEnemyCurrentEstimate;
        enemyCurrentEstimateSeconds = GetMobileEstimatedSeconds("EnemyCurrent", out hasEnemyCurrentEstimate);

        double enemyAllEstimateSeconds;
        bool hasEnemyAllEstimate;
        enemyAllEstimateSeconds = GetMobileEstimatedSeconds("EnemyAll", out hasEnemyAllEstimate);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Export Current Enemy/Class Set", GUILayout.Height(28), GUILayout.ExpandWidth(false)))
        {
            if (ConfirmBulkExportWithEstimate(
                "Export Current Enemy/Class Set",
                "Export the current enemy/class mobile sprite set?",
                enemyCurrentEstimateSeconds,
                !hasEnemyCurrentEstimate,
                false))
            {
                ExportCurrentEnemyMobileSet();
            }
        }

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Last: " + GetMobileLastDurationText("EnemyCurrent"), EditorStyles.miniLabel, GUILayout.Width(130));
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Export ALL Enemy/Class Sets", GUILayout.Height(28), GUILayout.ExpandWidth(false)))
        {
            if (ConfirmBulkExportWithEstimate(
                "Export ALL Enemy/Class Sets",
                "Export all enemy/class mobile sprite sets?",
                enemyAllEstimateSeconds,
                !hasEnemyAllEstimate,
                true))
            {
                ExportAllEnemyMobileSets();
            }
        }

        GUILayout.Space(10);
        EditorGUILayout.LabelField("ETA/Last: " + FormatDurationSecondsWithPlus(enemyAllEstimateSeconds, !hasEnemyAllEstimate), EditorStyles.miniLabel, GUILayout.Width(150));
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(4);
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Delete Current Enemy/Class Set", GUILayout.Height(24), GUILayout.ExpandWidth(false)))
        {
            bool confirmed = EditorUtility.DisplayDialog(
                "Delete Current Enemy/Class Set",
                "Delete the currently selected enemy/class mobile sprite folder or folders?\n\n" +
                previewEnemyFolderLabel + "\n\n" +
                "This cannot be undone.",
                "Delete Current Enemy/Class Set",
                "Cancel");

            if (confirmed)
                DeleteCurrentEnemyMobileSet();
        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    GUILayout.Space(8);

    EditorGUILayout.HelpBox(
        "Filename format: {EntityName}_{Animation}_{Orientation}_id{Archive}_{Record:000}_{Frame:00}_{Flip:0|1}_df.png",
        MessageType.None);
}
private void ExportCurrentTownNpcMobileSet()
{
    if (!ValidateBeforeTownNpcMobileExport())
        return;

    DateTime startUtc = DateTime.UtcNow;

    BeginDeferredAssetEditing();
    try
    {
        ExportTownNpcMobileSpriteSet(
            mobileTownNpcRace,
            mobileTownNpcGender,
            mobileTownNpcVariant,
            mobileTownNpcIsGuard);
    }
    catch (Exception ex)
    {
        Debug.LogError("[NexusDev][DFU Export] Town NPC mobile export failed. " + ex);
    }
    finally
    {
        EndDeferredAssetEditing(true);
    }

    if (mobileSpriteApplyImportSettingsAfterExport)
        ApplyImportSettingsToAllExportedMobileSprites();

    SaveMobileDuration("TownNpcCurrent", (DateTime.UtcNow - startUtc).TotalSeconds);
}

private void ExportAllTownNpcMobileSets()
{
    if (!ValidateBeforeTownNpcMobileExport())
        return;

    DateTime startUtc = DateTime.UtcNow;

    Races[] races = new Races[]
    {
        Races.Breton,
        Races.Redguard,
        Races.Nord,
    };

    Genders[] genders = new Genders[]
    {
        Genders.Male,
        Genders.Female,
    };

    int total = (races.Length * genders.Length * 4) + 1;
    int current = 0;

    BeginDeferredAssetEditing();
    try
    {
        for (int r = 0; r < races.Length; r++)
        {
            for (int g = 0; g < genders.Length; g++)
            {
                for (int variant = 0; variant < 4; variant++)
                {
                    current++;

                    string label = BuildTownNpcEntityName(races[r], genders[g], variant, false);
                    EditorUtility.DisplayProgressBar(
                        "DFU Export",
                        "Exporting town mobile sprites: " + label,
                        total > 0 ? (float)current / total : 0f);

                    ExportTownNpcMobileSpriteSet(races[r], genders[g], variant, false);
                }
            }
        }

        current++;
        EditorUtility.DisplayProgressBar(
            "DFU Export",
            "Exporting town mobile sprites: GuardMaleVariant00",
            total > 0 ? (float)current / total : 0f);

        ExportTownNpcMobileSpriteSet(
            Races.Breton,
            Genders.Male,
            0,
            true);

        Debug.Log("[NexusDev][DFU Export] ALL town NPC mobile sprite sets exported.");
    }
    catch (Exception ex)
    {
        Debug.LogError("[NexusDev][DFU Export] ALL town NPC mobile export failed. " + ex);
    }
    finally
    {
        EditorUtility.ClearProgressBar();
        EndDeferredAssetEditing(true);
    }

    if (mobileSpriteApplyImportSettingsAfterExport)
        ApplyImportSettingsToAllExportedMobileSprites();

    SaveMobileDuration("TownNpcAll", (DateTime.UtcNow - startUtc).TotalSeconds);
}

private void ExportAllEnemyMobileSets()
{
    if (!ValidateBeforeEnemyMobileExport())
        return;

    DateTime startUtc = DateTime.UtcNow;

    var enemies = global::DaggerfallWorkshop.Utility.EnemyBasics.Enemies;
    if (enemies == null || enemies.Length == 0)
    {
        Debug.LogWarning("[NexusDev][DFU Export] EnemyBasics.Enemies was empty.");
        return;
    }

    BeginDeferredAssetEditing();
    try
    {
        for (int i = 0; i < enemies.Length; i++)
        {
            string label = GetEnemyMobilePreviewFolderLabel(i);

            EditorUtility.DisplayProgressBar(
                "DFU Export",
                "Exporting enemy/class mobile sprites: " + label,
                enemies.Length > 0 ? (float)(i + 1) / enemies.Length : 0f);

            ExportEnemyMobileSetByIndex(i);
        }

        Debug.Log("[NexusDev][DFU Export] ALL enemy/class mobile sprite sets exported.");
    }
    catch (Exception ex)
    {
        Debug.LogError("[NexusDev][DFU Export] ALL enemy/class mobile export failed. " + ex);
    }
    finally
    {
        EditorUtility.ClearProgressBar();
        EndDeferredAssetEditing(true);
    }

    if (mobileSpriteApplyImportSettingsAfterExport)
        ApplyImportSettingsToAllExportedMobileSprites();

    SaveMobileDuration("EnemyAll", (DateTime.UtcNow - startUtc).TotalSeconds);
}
private void ExportCurrentEnemyMobileSet()
{
    if (!ValidateBeforeEnemyMobileExport())
        return;

    DateTime startUtc = DateTime.UtcNow;

    BeginDeferredAssetEditing();
    try
    {
        ExportEnemyMobileSetByIndex(mobileEnemySelectionIndex);
    }
    catch (Exception ex)
    {
        Debug.LogError("[NexusDev][DFU Export] Enemy/class mobile export failed. " + ex);
    }
    finally
    {
        EndDeferredAssetEditing(true);
    }

    if (mobileSpriteApplyImportSettingsAfterExport)
        ApplyImportSettingsToAllExportedMobileSprites();

    SaveMobileDuration("EnemyCurrent", (DateTime.UtcNow - startUtc).TotalSeconds);
}
private void ExportAllMobileSpritesFullSets()
{
    if (!ValidateBeforeMobileSpriteExport())
        return;

    DateTime startUtc = DateTime.UtcNow;
    bool previousApplyAfterExport = mobileSpriteApplyImportSettingsAfterExport;

    try
    {
        mobileSpriteApplyImportSettingsAfterExport = false;

        ExportAllTownNpcMobileSets();
        ExportAllEnemyMobileSets();

        if (previousApplyAfterExport)
            ApplyImportSettingsToAllExportedMobileSprites();

        AssetDatabase.Refresh();
        Debug.Log("[NexusDev][DFU Export] ALL relevant MobileSprites exported.");
    }
    catch (Exception ex)
    {
        Debug.LogError("[NexusDev][DFU Export] ALL MobileSprites export failed. " + ex);
    }
    finally
    {
        mobileSpriteApplyImportSettingsAfterExport = previousApplyAfterExport;
        EditorUtility.ClearProgressBar();
    }

    SaveMobileDuration("MobileAll", (DateTime.UtcNow - startUtc).TotalSeconds);
}

private void ExportEnemyMobileSetByIndex(int index)
{
    var enemies = global::DaggerfallWorkshop.Utility.EnemyBasics.Enemies;
    if (enemies == null || enemies.Length == 0)
        throw new InvalidOperationException("EnemyBasics.Enemies was empty.");

    index = Mathf.Clamp(index, 0, enemies.Length - 1);

    var enemy = enemies[index];
    string baseName = GetEnemyMobileBaseName(enemy.ID);

    ExportEnemyMobileSetVariant(
        enemy.ID,
        enemy.MaleTexture,
        baseName,
        false,
        enemy.HasIdle,
        enemy.HasRangedAttack1,
        enemy.HasRangedAttack2);

    if (enemy.FemaleTexture != enemy.MaleTexture)
    {
        ExportEnemyMobileSetVariant(
            enemy.ID,
            enemy.FemaleTexture,
            baseName + "Female",
            true,
            enemy.HasIdle,
            enemy.HasRangedAttack1,
            enemy.HasRangedAttack2);
    }
}

private void ExportEnemyMobileSetVariant(
    int enemyId,
    int archive,
    string entityName,
    bool isFemaleVariant,
    bool hasIdle,
    bool hasRangedAttack1,
    bool hasRangedAttack2)
{
    MobileSpriteAnimSpec[] specs = BuildEnemyAnimSpecs(enemyId, isFemaleVariant, hasIdle, hasRangedAttack1, hasRangedAttack2);

    ExportGenericMobileSpriteSet(
        MobileEnemyFolderAssetRelative,
        "EnemyMobile",
        "Enemy",
        entityName,
        archive,
        enemyId,
        isFemaleVariant,
        specs);
}


private MobileSpriteAnimSpec[] BuildEnemyAnimSpecs(
    int enemyId,
    bool isFemaleVariant,
    bool hasIdle,
    bool hasRangedAttack1,
    bool hasRangedAttack2)
{
    List<MobileSpriteAnimSpec> specs = new List<MobileSpriteAnimSpec>();

    global::DaggerfallWorkshop.MobileEnemy enemyDef;
    bool hasEnemyDef = TryGetEnemyDefinition(enemyId, out enemyDef);

    if (mobileEnemyIncludeMove)
        AddMobileAnimSpecsFromArray(specs, "Move", GetEnemyMoveAnimArray(enemyId));

    if (mobileEnemyIncludeIdle && hasIdle)
        AddMobileAnimSpecsFromArray(specs, "Idle", GetEnemyIdleAnimArray(enemyId, isFemaleVariant));

    if (mobileEnemyIncludePrimaryAttack)
        AddMobileAnimSpecsFromArray(specs, "PrimaryAttack", GetEnemyPrimaryAttackAnimArray(enemyId));

    if (mobileEnemyIncludeHurt)
        AddMobileAnimSpecsFromArray(specs, "Hurt", global::DaggerfallWorkshop.Utility.EnemyBasics.HurtAnims);

    if (mobileEnemyIncludeRangedAttack1 && hasRangedAttack1)
        AddMobileAnimSpecsFromArray(specs, "RangedAttack1", global::DaggerfallWorkshop.Utility.EnemyBasics.RangedAttack1Anims);

    if (mobileEnemyIncludeRangedAttack2 && hasRangedAttack2)
        AddMobileAnimSpecsFromArray(specs, "RangedAttack2", global::DaggerfallWorkshop.Utility.EnemyBasics.RangedAttack2Anims);

    if (hasEnemyDef && EnemyHasSpellStateForExport(enemyDef))
        AddMobileAnimSpecsFromArray(specs, "Spell", GetEnemySpellAnimArray(enemyDef));

    if (hasEnemyDef && enemyDef.HasSeducerTransform1)
        AddMobileAnimSpecsFromArray(specs, "SeducerTransform1", global::DaggerfallWorkshop.Utility.EnemyBasics.SeducerTransform1Anims);

    if (hasEnemyDef && enemyDef.HasSeducerTransform2)
        AddMobileAnimSpecsFromArray(specs, "SeducerTransform2", global::DaggerfallWorkshop.Utility.EnemyBasics.SeducerTransform2Anims);

    return specs.ToArray();
}

private bool TryGetEnemyDefinition(int enemyId, out global::DaggerfallWorkshop.MobileEnemy enemy)
{
    enemy = default(global::DaggerfallWorkshop.MobileEnemy);

    var enemies = global::DaggerfallWorkshop.Utility.EnemyBasics.Enemies;
    if (enemies == null || enemies.Length == 0)
        return false;

    for (int i = 0; i < enemies.Length; i++)
    {
        if (enemies[i].ID == enemyId)
        {
            enemy = enemies[i];
            return true;
        }
    }

    return false;
}

private bool EnemyHasSpellStateForExport(global::DaggerfallWorkshop.MobileEnemy enemy)
{
    if (!HasAnyNonNegativeFrames(enemy.SpellAnimFrames))
        return false;

    if (enemy.ID == (int)global::DaggerfallWorkshop.MobileTypes.DaedraSeducer)
        return true;

    return enemy.CastsMagic || enemy.HasSpellAnimation;
}

private Array GetEnemySpellAnimArray(global::DaggerfallWorkshop.MobileEnemy enemy)
{
    if (enemy.ID == (int)global::DaggerfallWorkshop.MobileTypes.DaedraSeducer)
        return global::DaggerfallWorkshop.Utility.EnemyBasics.SeducerAttackAnims;

    if (enemy.HasSpellAnimation)
        return global::DaggerfallWorkshop.Utility.EnemyBasics.RangedAttack1Anims;

    return global::DaggerfallWorkshop.Utility.EnemyBasics.PrimaryAttackAnims;
}

private bool HasAnyNonNegativeFrames(int[] frames)
{
    if (frames == null || frames.Length == 0)
        return false;

    for (int i = 0; i < frames.Length; i++)
    {
        if (frames[i] >= 0)
            return true;
    }

    return false;
}

private Array GetEnemyMoveAnimArray(int enemyId)
{
    if (enemyId == (int)global::DaggerfallWorkshop.MobileTypes.Slaughterfish)
        return global::DaggerfallWorkshop.Utility.EnemyBasics.SlaughterfishMoveAnims;

    if (enemyId == (int)global::DaggerfallWorkshop.MobileTypes.Ghost ||
        enemyId == (int)global::DaggerfallWorkshop.MobileTypes.Wraith)
        return global::DaggerfallWorkshop.Utility.EnemyBasics.GhostWraithMoveAnims;

    if (enemyId == (int)global::DaggerfallWorkshop.MobileTypes.DaedraSeducer)
        return global::DaggerfallWorkshop.Utility.EnemyBasics.SeducerIdleMoveAnims;

    return global::DaggerfallWorkshop.Utility.EnemyBasics.MoveAnims;
}

private Array GetEnemyIdleAnimArray(int enemyId, bool isFemaleVariant)
{
    if (enemyId == (int)global::DaggerfallWorkshop.MobileTypes.Rat)
        return global::DaggerfallWorkshop.Utility.EnemyBasics.RatIdleAnims;

    if (enemyId == (int)global::DaggerfallWorkshop.MobileTypes.Ghost ||
        enemyId == (int)global::DaggerfallWorkshop.MobileTypes.Wraith)
        return global::DaggerfallWorkshop.Utility.EnemyBasics.GhostWraithMoveAnims;

    if (enemyId == (int)global::DaggerfallWorkshop.MobileTypes.DaedraSeducer)
        return global::DaggerfallWorkshop.Utility.EnemyBasics.SeducerIdleMoveAnims;

    if (isFemaleVariant && enemyId == (int)global::DaggerfallWorkshop.MobileTypes.Thief)
        return global::DaggerfallWorkshop.Utility.EnemyBasics.FemaleThiefIdleAnims;

    return global::DaggerfallWorkshop.Utility.EnemyBasics.IdleAnims;
}

private Array GetEnemyPrimaryAttackAnimArray(int enemyId)
{
    if (enemyId == (int)global::DaggerfallWorkshop.MobileTypes.Ghost ||
        enemyId == (int)global::DaggerfallWorkshop.MobileTypes.Wraith)
        return global::DaggerfallWorkshop.Utility.EnemyBasics.GhostWraithAttackAnims;

    if (enemyId == (int)global::DaggerfallWorkshop.MobileTypes.DaedraSeducer)
        return global::DaggerfallWorkshop.Utility.EnemyBasics.SeducerAttackAnims;

    return global::DaggerfallWorkshop.Utility.EnemyBasics.PrimaryAttackAnims;
}

private void AddMobileAnimSpecsFromArray(
    List<MobileSpriteAnimSpec> specs,
    string animationName,
    Array animArray)
{
    if (specs == null || animArray == null)
        return;

    for (int i = 0; i < animArray.Length; i++)
    {
        object anim = animArray.GetValue(i);
        if (anim == null)
            continue;

        Type animType = anim.GetType();
        FieldInfo recordField = animType.GetField("Record", BindingFlags.Public | BindingFlags.Instance);
        FieldInfo flipField = animType.GetField("FlipLeftRight", BindingFlags.Public | BindingFlags.Instance);

        if (recordField == null || flipField == null)
            continue;

        int record = Convert.ToInt32(recordField.GetValue(anim), CultureInfo.InvariantCulture);
        bool flip = Convert.ToBoolean(flipField.GetValue(anim), CultureInfo.InvariantCulture);

        specs.Add(new MobileSpriteAnimSpec(
            animationName,
            GetOrientationNameFromIndex(i),
            record,
            flip));
    }
}

private string GetEnemyMobilePreviewFolderLabel(int index)
{
    var enemies = global::DaggerfallWorkshop.Utility.EnemyBasics.Enemies;
    if (enemies == null || enemies.Length == 0)
        return "Enemy";

    index = Mathf.Clamp(index, 0, enemies.Length - 1);
    var enemy = enemies[index];

    string baseName = GetEnemyMobileBaseName(enemy.ID);
    if (enemy.FemaleTexture != enemy.MaleTexture)
        return baseName + "Male + " + baseName + "Female";

    return baseName;
}
private List<string> GetEnemyMobileEntityFolderNamesForSelection(int index)
{
    List<string> results = new List<string>();

    var enemies = global::DaggerfallWorkshop.Utility.EnemyBasics.Enemies;
    if (enemies == null || enemies.Length == 0)
        return results;

    index = Mathf.Clamp(index, 0, enemies.Length - 1);
    var enemy = enemies[index];

    string baseName = GetEnemyMobileBaseName(enemy.ID);
    results.Add(baseName);

    if (enemy.FemaleTexture != enemy.MaleTexture)
        results.Add(baseName + "Female");

    return results;
}

private bool DeleteMobileSpriteEntityFolder(string rootAssetRelative, string entityFolderName)
{
    if (string.IsNullOrWhiteSpace(rootAssetRelative) || string.IsNullOrWhiteSpace(entityFolderName))
        return false;

    string folderAssetPath = rootAssetRelative.TrimEnd('/') + "/" + entityFolderName;
    string folderAbs = GetAbsolutePathFromAssetPath(folderAssetPath);

    if (string.IsNullOrWhiteSpace(folderAbs) || !Directory.Exists(folderAbs))
        return false;

    FileUtil.DeleteFileOrDirectory(folderAssetPath);
    FileUtil.DeleteFileOrDirectory(folderAssetPath + ".meta");
    return true;
}

private void DeleteCurrentTownNpcMobileSet()
{
    string entityName = BuildTownNpcEntityName(
        mobileTownNpcRace,
        mobileTownNpcGender,
        mobileTownNpcVariant,
        mobileTownNpcIsGuard);

    try
    {
        bool deleted = DeleteMobileSpriteEntityFolder(MobileTownNpcFolderAssetRelative, entityName);
        AssetDatabase.Refresh();

        if (deleted)
        {
            Debug.Log("[NexusDev][DFU Export] Deleted Town NPC mobile sprite set: " + entityName);
        }
        else
        {
            Debug.LogWarning("[NexusDev][DFU Export] Town NPC mobile sprite folder was not found: " + entityName);
        }
    }
    catch (Exception ex)
    {
        Debug.LogError("[NexusDev][DFU Export] Failed deleting Town NPC mobile sprite set '" + entityName + "'. Exception: " + ex);
    }
}

private void DeleteAllTownNpcMobileSprites()
{
    int deletedCount = 0;

    try
    {
        deletedCount += DeleteAllFilesInAssetFolder(MobileTownNpcFolderAssetRelative);
        AssetDatabase.Refresh();

        EnsureFolderExists(MobileSpritesFolderAssetRelative);
        EnsureFolderExists(MobileTownNpcFolderAssetRelative);
        EnsureFolderExists(MobileEnemyFolderAssetRelative);
        AssetDatabase.Refresh();

        Debug.Log(string.Format(
            CultureInfo.InvariantCulture,
            "[NexusDev][DFU Export] Deleted {0} previously-exported Town NPC MobileSprites asset file(s).",
            deletedCount));
    }
    catch (Exception ex)
    {
        Debug.LogError("[NexusDev][DFU Export] Failed deleting ALL Town NPC MobileSprites. Exception: " + ex);
    }
}

private void DeleteCurrentEnemyMobileSet()
{
    List<string> folderNames = GetEnemyMobileEntityFolderNamesForSelection(mobileEnemySelectionIndex);
    int deletedCount = 0;

    try
    {
        for (int i = 0; i < folderNames.Count; i++)
        {
            if (DeleteMobileSpriteEntityFolder(MobileEnemyFolderAssetRelative, folderNames[i]))
                deletedCount++;
        }

        AssetDatabase.Refresh();

        if (deletedCount > 0)
        {
            Debug.Log("[NexusDev][DFU Export] Deleted current enemy/class mobile sprite set folder count: " +
                deletedCount.ToString(CultureInfo.InvariantCulture));
        }
        else
        {
            Debug.LogWarning("[NexusDev][DFU Export] No enemy/class mobile sprite folders were found for the current selection.");
        }
    }
    catch (Exception ex)
    {
        Debug.LogError("[NexusDev][DFU Export] Failed deleting current enemy/class mobile sprite set. Exception: " + ex);
    }
}
private string GetEnemyMobileBaseName(int enemyId)
{
    string enumName = Enum.GetName(typeof(global::DaggerfallWorkshop.MobileTypes), enemyId);
    if (string.IsNullOrWhiteSpace(enumName))
    {
        return "Enemy" + enemyId.ToString("000", CultureInfo.InvariantCulture);
    }

    return ToReadableExportToken(enumName.Replace("_", " "));
}


private string GetOrientationNameFromIndex(int index)
{
    switch (index)
    {
        case 0: return "Front";
        case 1: return "FrontLeft";
        case 2: return "Left";
        case 3: return "BackLeft";
        case 4: return "Back";
        case 5: return "BackRight";
        case 6: return "Right";
        case 7: return "FrontRight";
        default: return "Dir" + index.ToString("00", CultureInfo.InvariantCulture);
    }
}

private struct TownNpcMobileListEntry
{
    public int OriginalIndex;
    public string SortName;
    public string Label;
    public Races Race;
    public Genders Gender;
    public int Variant;
    public bool IsGuard;
}

private List<TownNpcMobileListEntry> BuildTownNpcMobileListEntriesUncached()
{
    List<TownNpcMobileListEntry> result = new List<TownNpcMobileListEntry>();
    int originalIndex = 0;

    Races[] races = new Races[]
    {
        Races.Breton,
        Races.Redguard,
        Races.Nord,
    };

    Genders[] genders = new Genders[]
    {
        Genders.Male,
        Genders.Female,
    };

    for (int r = 0; r < races.Length; r++)
    {
        for (int g = 0; g < genders.Length; g++)
        {
            for (int variant = 0; variant < 4; variant++)
            {
                string entityName = BuildTownNpcEntityName(races[r], genders[g], variant, false);

                TownNpcMobileListEntry entry = new TownNpcMobileListEntry();
                entry.OriginalIndex = originalIndex++;
                entry.SortName = entityName;
                entry.Label = entityName;
                entry.Race = races[r];
                entry.Gender = genders[g];
                entry.Variant = variant;
                entry.IsGuard = false;
                result.Add(entry);
            }
        }
    }

    TownNpcMobileListEntry guardEntry = new TownNpcMobileListEntry();
    guardEntry.OriginalIndex = originalIndex++;
guardEntry.SortName = "GuardMale00";
guardEntry.Label = "GuardMale00";
    guardEntry.Race = Races.Breton;
    guardEntry.Gender = Genders.Male;
    guardEntry.Variant = 0;
    guardEntry.IsGuard = true;
    result.Add(guardEntry);

    if (mobileSpriteSortMode == 1)
    {
        result.Sort((a, b) =>
        {
            int aId = GetTownNpcSortId(a);
            int bId = GetTownNpcSortId(b);

            int cmp = aId.CompareTo(bId);
            if (cmp != 0)
                return cmp;

            return string.Compare(a.SortName, b.SortName, StringComparison.OrdinalIgnoreCase);
        });
    }
    else
    {
        result.Sort((a, b) =>
        {
            int cmp = string.Compare(a.SortName, b.SortName, StringComparison.OrdinalIgnoreCase);
            if (cmp != 0)
                return cmp;

            return GetTownNpcSortId(a).CompareTo(GetTownNpcSortId(b));
        });
    }

    return result;
}

private int GetTownNpcSortId(TownNpcMobileListEntry entry)
{
    if (entry.IsGuard)
        return 399000;

    int[] archives = GetTownNpcTextureArchives(entry.Race, entry.Gender, false);
    if (archives == null || archives.Length == 0)
        return int.MaxValue;

    int variant = Mathf.Clamp(entry.Variant, 0, archives.Length - 1);
    return archives[variant];
}


private int GetSortedTownNpcPopupIndexFromCurrentSelection(List<TownNpcMobileListEntry> entries)
{
    if (entries == null || entries.Count == 0)
        return 0;

    for (int i = 0; i < entries.Count; i++)
    {
        if (entries[i].IsGuard == mobileTownNpcIsGuard &&
            entries[i].Race == mobileTownNpcRace &&
            entries[i].Gender == mobileTownNpcGender &&
            entries[i].Variant == mobileTownNpcVariant)
        {
            return i;
        }
    }

    return 0;
}

private void ApplySelectedTownNpcEntry(List<TownNpcMobileListEntry> entries, int sortedIndex)
{
    if (entries == null || entries.Count == 0)
        return;

    sortedIndex = Mathf.Clamp(sortedIndex, 0, entries.Count - 1);
    TownNpcMobileListEntry entry = entries[sortedIndex];

    mobileTownNpcSelectionIndex = sortedIndex;
    mobileTownNpcRace = entry.Race;
    mobileTownNpcGender = entry.Gender;
    mobileTownNpcVariant = entry.Variant;
    mobileTownNpcIsGuard = entry.IsGuard;
}

private struct EnemyMobileListEntry
{
    public int OriginalIndex;
    public int EnemyId;
    public string BaseName;
    public string Label;
}

private List<EnemyMobileListEntry> BuildEnemyMobileListEntriesUncached()
{
    List<EnemyMobileListEntry> result = new List<EnemyMobileListEntry>();

    var enemies = global::DaggerfallWorkshop.Utility.EnemyBasics.Enemies;
    if (enemies == null || enemies.Length == 0)
        return result;

    for (int i = 0; i < enemies.Length; i++)
    {
        string baseName = GetEnemyMobileBaseName(enemies[i].ID);
        string label;

        if (enemies[i].FemaleTexture != enemies[i].MaleTexture)
        {
            label = baseName +
                    " [id " + enemies[i].ID.ToString(CultureInfo.InvariantCulture) +
                    " | M:" + enemies[i].MaleTexture.ToString(CultureInfo.InvariantCulture) +
                    " F:" + enemies[i].FemaleTexture.ToString(CultureInfo.InvariantCulture) + "]";
        }
        else
        {
            label = baseName +
                    " [id " + enemies[i].ID.ToString(CultureInfo.InvariantCulture) +
                    " | A:" + enemies[i].MaleTexture.ToString(CultureInfo.InvariantCulture) + "]";
        }

        EnemyMobileListEntry entry = new EnemyMobileListEntry();
        entry.OriginalIndex = i;
        entry.EnemyId = enemies[i].ID;
        entry.BaseName = baseName;
        entry.Label = label;
        result.Add(entry);
    }
if (mobileSpriteSortMode == 1)
{
    result.Sort((a, b) =>
    {
        int cmp = a.EnemyId.CompareTo(b.EnemyId);
        if (cmp != 0)
            return cmp;

        return string.Compare(a.BaseName, b.BaseName, StringComparison.OrdinalIgnoreCase);
    });
}
else
{
    result.Sort((a, b) =>
    {
        int cmp = string.Compare(a.BaseName, b.BaseName, StringComparison.OrdinalIgnoreCase);
        if (cmp != 0)
            return cmp;

        return a.EnemyId.CompareTo(b.EnemyId);
    });
}

    return result;
}

private int GetSortedEnemyPopupIndexFromOriginalIndex(List<EnemyMobileListEntry> entries, int originalIndex)
{
    if (entries == null || entries.Count == 0)
        return 0;

    for (int i = 0; i < entries.Count; i++)
    {
        if (entries[i].OriginalIndex == originalIndex)
            return i;
    }

    return 0;
}

private int GetOriginalEnemyIndexFromSortedPopupIndex(List<EnemyMobileListEntry> entries, int sortedIndex)
{
    if (entries == null || entries.Count == 0)
        return 0;

    sortedIndex = Mathf.Clamp(sortedIndex, 0, entries.Count - 1);
    return entries[sortedIndex].OriginalIndex;
}

private bool ValidateBeforeMobileSpriteExport()
{
    global::DaggerfallWorkshop.DaggerfallUnity dfUnity = global::DaggerfallWorkshop.DaggerfallUnity.Instance;
    if (dfUnity == null)
    {
        Debug.LogError("[NexusDev][DFU Export] DaggerfallUnity.Instance is null.");
        return false;
    }

    if (string.IsNullOrWhiteSpace(dfUnity.Arena2Path) || !Directory.Exists(dfUnity.Arena2Path))
    {
        Debug.LogError("[NexusDev][DFU Export] Arena2Path is missing or invalid: " + dfUnity.Arena2Path);
        return false;
    }

    EnsureFolderExists(ExportFolderAssetRelative);
    EnsureFolderExists(MobileSpritesFolderAssetRelative);
    EnsureFolderExists(MobileTownNpcFolderAssetRelative);
    EnsureFolderExists(MobileEnemyFolderAssetRelative);
    return true;
}

private bool ValidateBeforeTownNpcMobileExport()
{
    if (!ValidateBeforeMobileSpriteExport())
        return false;

    if (!mobileTownNpcIncludeMove && !mobileTownNpcIncludeIdle)
    {
        Debug.LogWarning("[NexusDev][DFU Export] No Town NPC mobile animation toggles are enabled.");
        return false;
    }

    return true;
}

private bool ValidateBeforeEnemyMobileExport()
{
    if (!ValidateBeforeMobileSpriteExport())
        return false;

    if (!mobileEnemyIncludeMove &&
        !mobileEnemyIncludeIdle &&
        !mobileEnemyIncludePrimaryAttack &&
        !mobileEnemyIncludeHurt &&
        !mobileEnemyIncludeRangedAttack1 &&
        !mobileEnemyIncludeRangedAttack2)
    {
        Debug.LogWarning("[NexusDev][DFU Export] No Enemy mobile animation toggles are enabled.");
        return false;
    }

    return true;
}

private bool ValidateBeforeFullMobileSpriteExport()
{
    if (!ValidateBeforeTownNpcMobileExport())
        return false;

    if (!ValidateBeforeEnemyMobileExport())
        return false;

    return true;
}
private void ExportTownNpcMobileSpriteSet(
    Races race,
    Genders gender,
    int personVariant,
    bool isGuard)
{
    global::DaggerfallWorkshop.DaggerfallUnity dfUnity = global::DaggerfallWorkshop.DaggerfallUnity.Instance;
    if (dfUnity == null)
        throw new InvalidOperationException("DaggerfallUnity.Instance is null.");

    int[] archives = GetTownNpcTextureArchives(race, gender, isGuard);
    if (archives == null || archives.Length == 0)
        throw new InvalidOperationException("No town NPC texture archives resolved.");

    personVariant = Mathf.Clamp(personVariant, 0, archives.Length - 1);

    int archive = archives[personVariant];
    string entityName = BuildTownNpcEntityName(race, gender, personVariant, isGuard);

    string townNpcRootAbs = GetAbsolutePathFromAssetPath(MobileTownNpcFolderAssetRelative);
    if (string.IsNullOrWhiteSpace(townNpcRootAbs))
        throw new InvalidOperationException("Could not resolve TownNPC mobile export folder.");

    string entityFolderAbs = Path.Combine(townNpcRootAbs, entityName);
    Directory.CreateDirectory(entityFolderAbs);

    string texturePath = Path.Combine(
        dfUnity.Arena2Path,
        TextureFile.IndexToFileName(archive));

    if (!File.Exists(texturePath))
        throw new FileNotFoundException("Could not find texture archive file.", texturePath);

    TextureFile textureFile = new TextureFile(
        texturePath,
        FileUsage.UseMemory,
        true);

    TownNpcExportManifest manifest = new TownNpcExportManifest();
    manifest.exportType = "TownNPCMobile";
    manifest.entityName = entityName;
    manifest.race = isGuard ? "Guard" : race.ToString();
    manifest.gender = gender.ToString();
    manifest.outfitVariant = personVariant;
    manifest.isGuard = isGuard;
    manifest.archive = archive;

    TownNpcAnimSpec[] specs = GetTownNpcAnimSpecs(isGuard);

    for (int i = 0; i < specs.Length; i++)
    {
        TownNpcAnimSpec spec = specs[i];

        if (spec.AnimationName == "Move" && !mobileTownNpcIncludeMove)
            continue;

        if (spec.AnimationName == "Idle" && !mobileTownNpcIncludeIdle)
            continue;

        int frameCount = textureFile.GetFrameCount(spec.Record);
        if (frameCount < 1)
            frameCount = 1;

        TownNpcExportClip clip = new TownNpcExportClip();
        clip.animationName = spec.AnimationName;
        clip.orientationName = spec.OrientationName;
        clip.record = spec.Record;
        clip.flipLeftRight = spec.FlipLeftRight;
        clip.frameCount = frameCount;

        for (int frame = 0; frame < frameCount; frame++)
        {
                        DFSize frameSize;
            byte[] pngBytes = CreateTownNpcFramePng(
                textureFile,
                spec.Record,
                frame,
                spec.FlipLeftRight,
                out frameSize);

            string fileName = BuildTownNpcFrameFileName(
                entityName,
                spec.AnimationName,
                spec.OrientationName,
                archive,
                spec.Record,
                frame,
                spec.FlipLeftRight);

            string filePath = Path.Combine(entityFolderAbs, fileName);
          File.WriteAllBytes(filePath, pngBytes);
clip.files.Add(fileName);

            TownNpcExportFrame frameInfo = new TownNpcExportFrame();
            frameInfo.fileName = fileName;
            frameInfo.archive = archive;
            frameInfo.record = spec.Record;
            frameInfo.frame = frame;
            frameInfo.flipLeftRight = spec.FlipLeftRight;
            clip.frames.Add(frameInfo);
        }

        manifest.clips.Add(clip);
    }

    string manifestPath = Path.Combine(entityFolderAbs, entityName + "_manifest_df.json");
    File.WriteAllText(manifestPath, JsonUtility.ToJson(manifest, true));

    Debug.Log(string.Format(
        CultureInfo.InvariantCulture,
        "[NexusDev][DFU Export] Exported town mobile sprite set '{0}' to '{1}'",
        entityName,
        entityFolderAbs));
}

private int[] GetTownNpcTextureArchives(
    Races race,
    Genders gender,
    bool isGuard)
{
    if (isGuard)
        return new int[] { 399 };

    switch (race)
    {
        case Races.Redguard:
            return (gender == Genders.Male)
                ? new int[] { 381, 382, 383, 384 }
                : new int[] { 395, 396, 397, 398 };

        case Races.Nord:
            return (gender == Genders.Male)
                ? new int[] { 387, 388, 389, 390 }
                : new int[] { 392, 393, 451, 452 };

        case Races.Breton:
        default:
            return (gender == Genders.Male)
                ? new int[] { 385, 386, 391, 394 }
                : new int[] { 453, 454, 455, 456 };
    }
}

private TownNpcAnimSpec[] GetTownNpcAnimSpecs(bool isGuard)
{
    List<TownNpcAnimSpec> specs = new List<TownNpcAnimSpec>();

    specs.Add(new TownNpcAnimSpec("Move", "Front", 0, false));
    specs.Add(new TownNpcAnimSpec("Move", "FrontLeft", 1, false));
    specs.Add(new TownNpcAnimSpec("Move", "Left", 2, false));
    specs.Add(new TownNpcAnimSpec("Move", "BackLeft", 3, false));
    specs.Add(new TownNpcAnimSpec("Move", "Back", 4, false));
    specs.Add(new TownNpcAnimSpec("Move", "BackRight", 3, true));
    specs.Add(new TownNpcAnimSpec("Move", "Right", 2, true));
    specs.Add(new TownNpcAnimSpec("Move", "FrontRight", 1, true));

    specs.Add(new TownNpcAnimSpec("Idle", "Front", isGuard ? 15 : 5, false));

    return specs.ToArray();
}
private string BuildTownNpcEntityName(
    Races race,
    Genders gender,
    int personVariant,
    bool isGuard)
{
    personVariant = Mathf.Clamp(personVariant, 0, 99);

    if (isGuard)
    {
        return string.Format(
            CultureInfo.InvariantCulture,
            "Guard{0}{1:00}",
            gender == Genders.Female ? "Female" : "Male",
            0);
    }

    string raceName;
    switch (race)
    {
        case Races.Redguard:
            raceName = "Redguard";
            break;
        case Races.Nord:
            raceName = "Nord";
            break;
        case Races.Breton:
        default:
            raceName = "Breton";
            break;
    }

    return string.Format(
        CultureInfo.InvariantCulture,
        "{0}{1}{2:00}",
        raceName,
        gender == Genders.Female ? "Female" : "Male",
        personVariant);
}

private string BuildTownNpcFrameFileName(
    string entityName,
    string animationName,
    string orientationName,
    int archive,
    int record,
    int frame,
    bool flipLeftRight)
{
    return BuildGenericMobileFrameFileName(
        entityName,
        animationName,
        orientationName,
        archive,
        record,
        frame,
        flipLeftRight);
}
private string BuildGenericMobileFrameFileName(
    string entityName,
    string animationName,
    string orientationName,
    int archive,
    int record,
    int frame,
    bool flipLeftRight)
{
    return string.Format(
        CultureInfo.InvariantCulture,
        "{0}_{1}_{2}_id{3}_{4:000}_{5:00}_{6}_df.png",
        entityName,
        animationName,
        orientationName,
        archive,
        record,
        frame,
        flipLeftRight ? 1 : 0);
}

private void ExportGenericMobileSpriteSet(
    string rootAssetRelative,
    string exportType,
    string category,
    string entityName,
    int archive,
    int enemyId,
    bool isFemaleVariant,
    MobileSpriteAnimSpec[] specs)
{
    global::DaggerfallWorkshop.DaggerfallUnity dfUnity = global::DaggerfallWorkshop.DaggerfallUnity.Instance;
    if (dfUnity == null)
        throw new InvalidOperationException("DaggerfallUnity.Instance is null.");

    if (specs == null || specs.Length == 0)
        throw new InvalidOperationException("No mobile animation specs were supplied for '" + entityName + "'.");

    string rootAbs = GetAbsolutePathFromAssetPath(rootAssetRelative);
    if (string.IsNullOrWhiteSpace(rootAbs))
        throw new InvalidOperationException("Could not resolve mobile export root folder: " + rootAssetRelative);

    Directory.CreateDirectory(rootAbs);

    string entityFolderAbs = Path.Combine(rootAbs, entityName);
    Directory.CreateDirectory(entityFolderAbs);

    string texturePath = Path.Combine(
        dfUnity.Arena2Path,
        TextureFile.IndexToFileName(archive));

    if (!File.Exists(texturePath))
        throw new FileNotFoundException("Could not find texture archive file.", texturePath);

    TextureFile textureFile = new TextureFile(
        texturePath,
        FileUsage.UseMemory,
        true);

    MobileSpriteExportManifest manifest = new MobileSpriteExportManifest();
    manifest.exportType = exportType;
    manifest.category = category;
    manifest.entityName = entityName;
    manifest.archive = archive;
    manifest.enemyId = enemyId;
    manifest.isFemaleVariant = isFemaleVariant;

    for (int i = 0; i < specs.Length; i++)
    {
        MobileSpriteAnimSpec spec = specs[i];

        int frameCount = textureFile.GetFrameCount(spec.Record);
        if (frameCount < 1)
            continue;

        TownNpcExportClip clip = new TownNpcExportClip();
        clip.animationName = spec.AnimationName;
        clip.orientationName = spec.OrientationName;
        clip.record = spec.Record;
        clip.flipLeftRight = spec.FlipLeftRight;
        clip.frameCount = frameCount;

        for (int frame = 0; frame < frameCount; frame++)
        {
            DFSize frameSize;
            byte[] pngBytes = CreateTownNpcFramePng(
                textureFile,
                spec.Record,
                frame,
                spec.FlipLeftRight,
                out frameSize);

            string fileName = BuildGenericMobileFrameFileName(
                entityName,
                spec.AnimationName,
                spec.OrientationName,
                archive,
                spec.Record,
                frame,
                spec.FlipLeftRight);

            string filePath = Path.Combine(entityFolderAbs, fileName);
            File.WriteAllBytes(filePath, pngBytes);
            clip.files.Add(fileName);

            TownNpcExportFrame frameInfo = new TownNpcExportFrame();
            frameInfo.fileName = fileName;
            frameInfo.archive = archive;
            frameInfo.record = spec.Record;
            frameInfo.frame = frame;
            frameInfo.flipLeftRight = spec.FlipLeftRight;
            clip.frames.Add(frameInfo);
        }

        manifest.clips.Add(clip);
    }

    for (int i = 0; i < manifest.clips.Count; i++)
    {
        string animationName = manifest.clips[i].animationName;

        if (string.Equals(animationName, "Spell", StringComparison.OrdinalIgnoreCase))
            manifest.hasSpellState = true;
        else if (string.Equals(animationName, "SeducerTransform1", StringComparison.OrdinalIgnoreCase))
            manifest.hasSeducerTransform1 = true;
        else if (string.Equals(animationName, "SeducerTransform2", StringComparison.OrdinalIgnoreCase))
            manifest.hasSeducerTransform2 = true;
    }

    string manifestPath = Path.Combine(entityFolderAbs, entityName + "_manifest_df.json");
    File.WriteAllText(manifestPath, JsonUtility.ToJson(manifest, true));

    Debug.Log(string.Format(
        CultureInfo.InvariantCulture,
        "[NexusDev][DFU Export] Exported mobile sprite set '{0}' to '{1}'",
        entityName,
        entityFolderAbs));
}

private byte[] CreateTownNpcFramePng(
    TextureFile textureFile,
    int record,
    int frame,
    bool flipLeftRight,
    out DFSize size)
{
    Color32[] colors = textureFile.GetColor32(record, frame, 0, 0, out size);

    if (flipLeftRight)
        FlipPixelsHorizontally(colors, size.Width, size.Height);

    Texture2D texture = new Texture2D(size.Width, size.Height, TextureFormat.ARGB32, false);
    texture.filterMode = FilterMode.Point;
    texture.wrapMode = TextureWrapMode.Clamp;
    texture.SetPixels32(colors);
    texture.Apply(false, false);

    byte[] pngBytes = texture.EncodeToPNG();
    UnityEngine.Object.DestroyImmediate(texture);
    return pngBytes;
}

private void FlipPixelsHorizontally(Color32[] colors, int width, int height)
{
    if (colors == null || colors.Length == 0 || width <= 1 || height <= 0)
        return;

    for (int y = 0; y < height; y++)
    {
        int rowStart = y * width;
        int left = 0;
        int right = width - 1;

        while (left < right)
        {
            int leftIndex = rowStart + left;
            int rightIndex = rowStart + right;

            Color32 temp = colors[leftIndex];
            colors[leftIndex] = colors[rightIndex];
            colors[rightIndex] = temp;

            left++;
            right--;
        }
    }
}


private void EnsureAtLeastOneCategoryBatchRow()
{
    if (selectedCategoryBatchSources == null)
        selectedCategoryBatchSources = new List<string>();

    for (int i = selectedCategoryBatchSources.Count - 1; i >= 0; i--)
    {
        if (string.IsNullOrWhiteSpace(selectedCategoryBatchSources[i]) ||
            Array.IndexOf(FixedCategoryBatchSources, selectedCategoryBatchSources[i]) < 0)
        {
            selectedCategoryBatchSources.RemoveAt(i);
        }
    }

    if (selectedCategoryBatchSources.Count == 0 && FixedCategoryBatchSources.Length > 0)
        selectedCategoryBatchSources.Add(FixedCategoryBatchSources[0]);
}

private void EnsureAtLeastOneFlatCategoryBatchRow()
{
    if (selectedFlatCategoryBatchSources == null)
        selectedFlatCategoryBatchSources = new List<string>();

    string[] flatSources = GetAvailableFlatCategoryBatchSources();

    for (int i = selectedFlatCategoryBatchSources.Count - 1; i >= 0; i--)
    {
        if (string.IsNullOrWhiteSpace(selectedFlatCategoryBatchSources[i]) ||
            Array.IndexOf(flatSources, selectedFlatCategoryBatchSources[i]) < 0)
        {
            selectedFlatCategoryBatchSources.RemoveAt(i);
        }
    }

    if (selectedFlatCategoryBatchSources.Count == 0 && flatSources.Length > 0)
        selectedFlatCategoryBatchSources.Add(flatSources[0]);
}

private static string JoinCategoryBatchSelections(List<string> selections)
{
    if (selections == null || selections.Count == 0)
        return string.Empty;

    return string.Join("|", selections.ToArray());
}

private static List<string> SplitCategoryBatchSelections(string saved)
{
    List<string> result = new List<string>();
    if (string.IsNullOrWhiteSpace(saved))
        return result;

    string[] parts = saved.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
    for (int i = 0; i < parts.Length; i++)
    {
        string value = parts[i].Trim();
        if (!string.IsNullOrEmpty(value))
            result.Add(value);
    }

    return result;
}

private string GetCategoryDurationKey(string fieldName)
{
    return fieldName + (exportGroupedFamilyMembers ? "|Grouped" : "|Base");
}

private static string MakeCategoryDurationPrefKey(string fieldName, bool grouped)
{
    return PrefCategoryLastDurationSecondsPrefix + fieldName + (grouped ? "|Grouped" : "|Base");
}

private int GetCategoryBaseModelCount(string fieldName)
{
    if (string.Equals(fieldName, "ships", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(fieldName, "models_structure", StringComparison.OrdinalIgnoreCase))
    {
        List<uint> ids;
        string error;
        if (TryGetModelIdsFromCategorySource(fieldName, out ids, out error) && ids != null)
            return ids.Count;

        return 0;
    }

    HardcodedCategoryCount counts;
    if (HardcodedCategoryCounts.TryGetValue(fieldName, out counts))
        return counts.BaseCount;

    return 0;
}

private int GetCategoryGroupedModelCount(string fieldName)
{
    if (string.Equals(fieldName, "ships", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(fieldName, "models_structure", StringComparison.OrdinalIgnoreCase))
    {
        List<uint> ids;
        string error;
        if (TryGetModelIdsFromCategorySource(fieldName, out ids, out error) && ids != null)
            return ExpandModelIdsByGroupedFamiliesForced(ids).Count;

        return 0;
    }

    HardcodedCategoryCount counts;
    if (HardcodedCategoryCounts.TryGetValue(fieldName, out counts))
        return counts.GroupedCount;

    return 0;
}

private int GetGroupedFamilyCountForModelId(uint modelId)
{
    string groupName;
    string[] groupIds;
    int indexZeroBased;

    if (TryGetModelGroupEntryForModelId(modelId, out groupName, out groupIds, out indexZeroBased) &&
        groupIds != null &&
        groupIds.Length > 0)
    {
        return groupIds.Length;
    }

    return 1;
}

private List<uint> ExpandModelIdsByGroupedFamiliesForced(List<uint> ids)
{
    if (ids == null || ids.Count == 0)
        return ids ?? new List<uint>();

    SortedSet<uint> expanded = new SortedSet<uint>();

    for (int i = 0; i < ids.Count; i++)
    {
        uint modelId = ids[i];

        string groupName;
        string[] groupIds;
        int indexZeroBased;
        if (TryGetModelGroupEntryForModelId(modelId, out groupName, out groupIds, out indexZeroBased) &&
            groupIds != null &&
            groupIds.Length > 0)
        {
            for (int j = 0; j < groupIds.Length; j++)
            {
                uint groupedId;
                if (uint.TryParse(groupIds[j], NumberStyles.Integer, CultureInfo.InvariantCulture, out groupedId))
                    expanded.Add(groupedId);
            }
        }
        else
        {
            expanded.Add(modelId);
        }
    }

    return new List<uint>(expanded);
}

private string GetLastDurationText(string fieldName)
{
    double seconds;
    string durationKey = GetCategoryDurationKey(fieldName);

    if (!categoryLastDurationSeconds.TryGetValue(durationKey, out seconds) || seconds < 0d)
        return "(never)";

    return FormatDurationSeconds(seconds);
}



private static string FormatDurationSeconds(double seconds)
{
    if (seconds <= 0d)
        return "(unknown)";

    TimeSpan span = TimeSpan.FromSeconds(seconds);

    if (span.TotalHours >= 1d)
        return string.Format(CultureInfo.InvariantCulture, "{0:%h}h {0:%m}m {0:%s}s", span);

    if (span.TotalMinutes >= 1d)
        return string.Format(CultureInfo.InvariantCulture, "{0:%m}m {0:%s}s", span);

    return string.Format(CultureInfo.InvariantCulture, "{0:0.0}s", seconds);
}
private static string FormatDurationSecondsWithPlus(double seconds, bool appendPlus)
{
    string text = FormatDurationSeconds(seconds);
    if (!appendPlus)
        return text;

    if (string.Equals(text, "(unknown)", StringComparison.Ordinal))
        return "(unknown+)";

    return text + "+";
}

private bool TryGetSavedDurationSeconds(string durationKey, out double seconds)
{
    seconds = 0d;
    return !string.IsNullOrWhiteSpace(durationKey) &&
           categoryLastDurationSeconds.TryGetValue(durationKey, out seconds) &&
           seconds > 0d;
}

private static bool IsLongEstimatedOperation(double seconds)
{
    return seconds > 300d;
}

private static string[] GetKnownMobileDurationOperationNames()
{
    return new[]
    {
        "TownNpcCurrent",
        "TownNpcAll",
        "EnemyCurrent",
        "EnemyAll",
        "MobileAll",
        "ImportSettingsAll",
    };
}

private string GetMobileDurationKey(string operationName)
{
    return "Mobile|" + operationName;
}

private static string MakeMobileDurationPrefKey(string operationName)
{
    return "NexusDev.ExportDfuModelsWest.MobileLastDurationSeconds." + operationName;
}

private void SaveMobileDuration(string operationName, double seconds)
{
    string key = GetMobileDurationKey(operationName);
    categoryLastDurationSeconds[key] = seconds;
    EditorPrefs.SetFloat(MakeMobileDurationPrefKey(operationName), (float)seconds);
}

private string GetMobileLastDurationText(string operationName)
{
    double seconds;
    if (!TryGetSavedDurationSeconds(GetMobileDurationKey(operationName), out seconds))
        return "(never)";

    return FormatDurationSeconds(seconds);
}

private double GetMobileEstimatedSeconds(string operationName, out bool hasEstimate)
{
    double seconds;
    hasEstimate = TryGetSavedDurationSeconds(GetMobileDurationKey(operationName), out seconds);
    return hasEstimate ? seconds : 0d;
}

private double GetSelectedCategoryEstimatedSeconds(out bool hasMissing)
{
    hasMissing = false;
    double total = 0d;

    EnsureAtLeastOneCategoryBatchRow();

    HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    for (int i = 0; i < selectedCategoryBatchSources.Count; i++)
    {
        string fieldName = selectedCategoryBatchSources[i];
        if (!seen.Add(fieldName))
            continue;

        string durationKey = fieldName + (exportGroupedFamilyMembers ? "|Grouped" : "|Base");
        double seconds;
        if (TryGetSavedDurationSeconds(durationKey, out seconds))
            total += seconds;
        else
            hasMissing = true;
    }

    return total;
}

private double GetAllCategoryEstimatedSeconds(out bool hasMissing)
{
    hasMissing = false;
    double total = 0d;

    for (int i = 0; i < FixedCategoryBatchSources.Length; i++)
    {
        string durationKey = FixedCategoryBatchSources[i] + (exportGroupedFamilyMembers ? "|Grouped" : "|Base");
        double seconds;
        if (TryGetSavedDurationSeconds(durationKey, out seconds))
            total += seconds;
        else
            hasMissing = true;
    }

    return total;
}

private double GetSelectedFlatCategoryEstimatedSeconds(out bool hasMissing)
{
    hasMissing = false;
    double total = 0d;

    EnsureAtLeastOneFlatCategoryBatchRow();

    HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    for (int i = 0; i < selectedFlatCategoryBatchSources.Count; i++)
    {
        string fieldName = selectedFlatCategoryBatchSources[i];
        if (!seen.Add(fieldName))
            continue;

        string durationKey = "FlatCategory|" + fieldName + (exportGroupedFamilyMembers ? "|Grouped" : "|Base");
        double seconds;
        if (TryGetSavedDurationSeconds(durationKey, out seconds))
            total += seconds;
        else
            hasMissing = true;
    }

    return total;
}

private double GetAllFlatCategoryEstimatedSeconds(out bool hasMissing)
{
    hasMissing = false;
    double total = 0d;
    string[] flatSources = GetAvailableFlatCategoryBatchSources();

    for (int i = 0; i < flatSources.Length; i++)
    {
        string durationKey = "FlatCategory|" + flatSources[i] + (exportGroupedFamilyMembers ? "|Grouped" : "|Base");
        double seconds;
        if (TryGetSavedDurationSeconds(durationKey, out seconds))
            total += seconds;
        else
            hasMissing = true;
    }

    return total;
}

private double GetTotalEstimatedMobileExportSeconds(out bool hasMissing)
{
    hasMissing = false;

    double fullSeconds;
    if (TryGetSavedDurationSeconds(GetMobileDurationKey("MobileAll"), out fullSeconds))
        return fullSeconds;

    double total = 0d;
    string[] componentOps = mobileSpriteApplyImportSettingsAfterExport
        ? new[] { "TownNpcAll", "EnemyAll", "ImportSettingsAll" }
        : new[] { "TownNpcAll", "EnemyAll" };

    for (int i = 0; i < componentOps.Length; i++)
    {
        double seconds;
        if (TryGetSavedDurationSeconds(GetMobileDurationKey(componentOps[i]), out seconds))
            total += seconds;
        else
            hasMissing = true;
    }

    return total;
}

private bool ConfirmBulkExportWithEstimate(string dialogTitle, string bodyLine, double estimatedSeconds, bool hasMissingEstimate, bool alwaysConfirm)
{
    if (!alwaysConfirm && !IsLongEstimatedOperation(estimatedSeconds))
        return true;

    string estimateText = FormatDurationSecondsWithPlus(estimatedSeconds, hasMissingEstimate);
    string message;

    if (IsLongEstimatedOperation(estimatedSeconds))
    {
        message =
            "Are you sure, it is estimated to take about " + estimateText + ".\n\n" +
            bodyLine;

        if (hasMissingEstimate)
            message += "\n\n+ means one or more parts have no saved timing yet.";
    }
    else
    {
        message =
            bodyLine + "\n\nEstimated duration: " + estimateText;

        if (hasMissingEstimate)
            message += "\n+ means one or more parts have no saved timing yet.";
    }

    return EditorUtility.DisplayDialog(dialogTitle, message, "Continue", "Cancel");
}

private string BuildStoredDurationDebugReportText()
{
    StringBuilder sb = new StringBuilder();

    sb.AppendLine("DFU Export Duration Estimates");
    sb.AppendLine("Generated: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
    sb.AppendLine();

    sb.AppendLine("[Model Categories]");
    for (int i = 0; i < FixedCategoryBatchSources.Length; i++)
    {
        string fieldName = FixedCategoryBatchSources[i];

        double baseSeconds;
        if (TryGetSavedDurationSeconds(fieldName + "|Base", out baseSeconds))
            sb.AppendLine(fieldName + " | Base = " + baseSeconds.ToString("0.###", CultureInfo.InvariantCulture) + " sec (" + FormatDurationSeconds(baseSeconds) + ")");
        else
            sb.AppendLine(fieldName + " | Base = (never)");

        double groupedSeconds;
        if (TryGetSavedDurationSeconds(fieldName + "|Grouped", out groupedSeconds))
            sb.AppendLine(fieldName + " | Grouped = " + groupedSeconds.ToString("0.###", CultureInfo.InvariantCulture) + " sec (" + FormatDurationSeconds(groupedSeconds) + ")");
        else
            sb.AppendLine(fieldName + " | Grouped = (never)");
    }

    sb.AppendLine();
    sb.AppendLine("[Flat Categories]");

    string[] flatSources = GetAvailableFlatCategoryBatchSources();
    for (int i = 0; i < flatSources.Length; i++)
    {
        string fieldName = flatSources[i];
        string baseKey = "FlatCategory|" + fieldName + "|Base";
        string groupedKey = "FlatCategory|" + fieldName + "|Grouped";

        double baseSeconds;
        if (TryGetSavedDurationSeconds(baseKey, out baseSeconds))
            sb.AppendLine(fieldName + " | Base = " + baseSeconds.ToString("0.###", CultureInfo.InvariantCulture) + " sec (" + FormatDurationSeconds(baseSeconds) + ")");
        else
            sb.AppendLine(fieldName + " | Base = (never)");

        double groupedSeconds;
        if (TryGetSavedDurationSeconds(groupedKey, out groupedSeconds))
            sb.AppendLine(fieldName + " | Grouped = " + groupedSeconds.ToString("0.###", CultureInfo.InvariantCulture) + " sec (" + FormatDurationSeconds(groupedSeconds) + ")");
        else
            sb.AppendLine(fieldName + " | Grouped = (never)");
    }

    sb.AppendLine();
    sb.AppendLine("[Mobile]");
    string[] mobileOps = GetKnownMobileDurationOperationNames();
    for (int i = 0; i < mobileOps.Length; i++)
    {
        string op = mobileOps[i];
        double seconds;
        if (TryGetSavedDurationSeconds(GetMobileDurationKey(op), out seconds))
            sb.AppendLine(op + " = " + seconds.ToString("0.###", CultureInfo.InvariantCulture) + " sec (" + FormatDurationSeconds(seconds) + ")");
        else
            sb.AppendLine(op + " = (never)");
    }

    return sb.ToString();
}

private void ExportStoredDurationDebugReport()
{
    string savePath = EditorUtility.SaveFilePanel(
        "Export DFU duration estimates",
        Directory.GetParent(Application.dataPath).FullName,
        "DFU_Export_Duration_Estimates_" + DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture),
        "txt");

    if (string.IsNullOrWhiteSpace(savePath))
        return;

    File.WriteAllText(savePath, BuildStoredDurationDebugReportText());
    Debug.Log("[NexusDev][DFU Export] Exported duration estimates report to: " + savePath);
    EditorUtility.RevealInFinder(savePath);
}

private static string BuildBatchAtlasPageSuffix(int pageIndex, int totalPages)
{
    if (totalPages <= 1)
        return string.Empty;

    return "_Page_" + (pageIndex + 1).ToString("00", CultureInfo.InvariantCulture);
}
private double GetTotalEstimatedCategoryExportSeconds()
{
    double total = 0d;

    for (int i = 0; i < FixedCategoryBatchSources.Length; i++)
    {
        string durationKey = FixedCategoryBatchSources[i] + (exportGroupedFamilyMembers ? "|Grouped" : "|Base");
        double seconds;
        if (categoryLastDurationSeconds.TryGetValue(durationKey, out seconds) && seconds > 0d)
            total += seconds;
    }

    return total;
}
private void SaveCategoryDuration(string fieldName, double seconds)
{
    string durationKey = GetCategoryDurationKey(fieldName);
    categoryLastDurationSeconds[durationKey] = seconds;
    EditorPrefs.SetFloat(
        MakeCategoryDurationPrefKey(fieldName, exportGroupedFamilyMembers),
        (float)seconds);
}

private void ExportAllFixedCategories()
{
    BeginDeferredAssetEditing();
    try
    {
        for (int i = 0; i < FixedCategoryBatchSources.Length; i++)
            ExportOneFixedCategory(FixedCategoryBatchSources[i]);
    }
    finally
    {
        EndDeferredAssetEditing(true);
    }
}

private void ExportOneFixedCategory(string fieldName)
{
    DateTime startUtc = DateTime.UtcNow;
    int successCount = 0;
    int failCount = 0;

    try
    {
        ExportSelectedCategoryBatchInternal(fieldName, out successCount, out failCount);
    }
    finally
    {
        double seconds = (DateTime.UtcNow - startUtc).TotalSeconds;
        SaveCategoryDuration(fieldName, seconds);

        Debug.Log(string.Format(
            CultureInfo.InvariantCulture,
            "[NexusDev][DFU Export] Category '{0}' finished in {1}. Success: {2}, Failed: {3}",
            fieldName,
            FormatDurationSeconds(seconds),
            successCount,
            failCount));
    }
}
private void LoadPrefs()
{
    txtPath = EditorPrefs.GetString(PrefTxtPath, string.Empty);
    singleModelTxtPath = EditorPrefs.GetString(PrefSingleModelTxtPath, txtPath ?? string.Empty);
    singleFlatTxtPath = EditorPrefs.GetString(PrefSingleFlatTxtPath, txtPath ?? string.Empty);

    deleteTempTextureFilesAfterExport = EditorPrefs.GetBool(PrefDeleteTemps, true);
    overwriteExistingExportAssets = EditorPrefs.GetBool(PrefOverwriteExistingExportAssets, true);
    exportGroupedFamilyMembers = EditorPrefs.GetBool(PrefExportGroupedFamilyMembers, false);
    categoryBatchFbxSubfolders = EditorPrefs.GetBool(PrefCategoryBatchFbxSubfolders, false);
    exportScaleMultiplier = EditorPrefs.GetFloat(PrefExportScaleMultiplier, 1f);
    atlasBatchExportedMats = EditorPrefs.GetBool(PrefAtlasBatchExportedMats, false);
    batchAtlasMaxSize = EditorPrefs.GetInt(PrefBatchAtlasMaxSize, 8192);

    singleModelBatchRangeText = EditorPrefs.GetString(PrefSingleModelBatchRangeText, string.Empty);
    singleFlatBatchRangeText = EditorPrefs.GetString(PrefSingleFlatBatchRangeText, string.Empty);

    categoryBatchExpanded = EditorPrefs.GetBool(PrefCategoryBatchExpanded, false);
    flatCategoryBatchExpanded = EditorPrefs.GetBool(PrefFlatCategoryBatchExpanded, false);

    selectedCategoryBatchSources = SplitCategoryBatchSelections(
        EditorPrefs.GetString(PrefCategoryBatchSelected, string.Empty));

    selectedFlatCategoryBatchSources = SplitCategoryBatchSelections(
        EditorPrefs.GetString(PrefFlatCategoryBatchSelected, string.Empty));

    if (batchAtlasMaxSize < 256) batchAtlasMaxSize = 256;
    if (batchAtlasMaxSize > 8192) batchAtlasMaxSize = 8192;

    string savedId = EditorPrefs.GetString(PrefSingleId, "40001");
    uint parsed;
    singleModelId = uint.TryParse(savedId, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed) ? parsed : 40001u;

    singleFlatIdText = EditorPrefs.GetString(PrefSingleFlatId, "210.2");
    if (string.IsNullOrWhiteSpace(singleFlatIdText))
        singleFlatIdText = "210.2";
mobileSpriteSortMode = EditorPrefs.GetInt(PrefMobileSpriteSortMode, 0);
if (mobileSpriteSortMode < 0 || mobileSpriteSortMode > 1)
    mobileSpriteSortMode = 0;

blenderImportScriptExpanded = EditorPrefs.GetBool("NexusDev.ExportDfuModelsWest.BlenderImportScriptExpanded", false);
    categoryLastDurationSeconds.Clear();

    for (int i = 0; i < FixedCategoryBatchSources.Length; i++)
    {
        string fieldName = FixedCategoryBatchSources[i];

        float baseSeconds = EditorPrefs.GetFloat(MakeCategoryDurationPrefKey(fieldName, false), -1f);
        if (baseSeconds >= 0f)
            categoryLastDurationSeconds[fieldName + "|Base"] = baseSeconds;

        float groupedSeconds = EditorPrefs.GetFloat(MakeCategoryDurationPrefKey(fieldName, true), -1f);
        if (groupedSeconds >= 0f)
            categoryLastDurationSeconds[fieldName + "|Grouped"] = groupedSeconds;
    }

    string[] flatSources = GetAvailableFlatCategoryBatchSources();
    for (int i = 0; i < flatSources.Length; i++)
    {
        string fieldName = flatSources[i];

        float baseSeconds = EditorPrefs.GetFloat(MakeFlatCategoryDurationPrefKey(fieldName, false), -1f);
        if (baseSeconds >= 0f)
            categoryLastDurationSeconds["FlatCategory|" + fieldName + "|Base"] = baseSeconds;

        float groupedSeconds = EditorPrefs.GetFloat(MakeFlatCategoryDurationPrefKey(fieldName, true), -1f);
        if (groupedSeconds >= 0f)
            categoryLastDurationSeconds["FlatCategory|" + fieldName + "|Grouped"] = groupedSeconds;
    }

    string[] mobileOps = GetKnownMobileDurationOperationNames();
    for (int i = 0; i < mobileOps.Length; i++)
    {
        float seconds = EditorPrefs.GetFloat(MakeMobileDurationPrefKey(mobileOps[i]), -1f);
        if (seconds >= 0f)
            categoryLastDurationSeconds[GetMobileDurationKey(mobileOps[i])] = seconds;
    }

    MarkFlatCategorySourcesDirty();
    RefreshFlatCategorySourcesCacheIfNeeded();
    InvalidateMobileListCaches();

    EnsureAtLeastOneCategoryBatchRow();
    EnsureAtLeastOneFlatCategoryBatchRow();
}

private void SavePrefs()
{
    EditorPrefs.SetString(PrefTxtPath, txtPath ?? string.Empty);
    EditorPrefs.SetString(PrefSingleModelTxtPath, singleModelTxtPath ?? string.Empty);
    EditorPrefs.SetString(PrefSingleFlatTxtPath, singleFlatTxtPath ?? string.Empty);

    EditorPrefs.SetBool(PrefDeleteTemps, deleteTempTextureFilesAfterExport);
    EditorPrefs.SetBool(PrefOverwriteExistingExportAssets, overwriteExistingExportAssets);
    EditorPrefs.SetBool(PrefExportGroupedFamilyMembers, exportGroupedFamilyMembers);
    EditorPrefs.SetBool(PrefCategoryBatchFbxSubfolders, categoryBatchFbxSubfolders);
    EditorPrefs.SetString(PrefSingleId, singleModelId.ToString(CultureInfo.InvariantCulture));
    EditorPrefs.SetString(PrefSingleFlatId, singleFlatIdText ?? "210.2");
    EditorPrefs.SetFloat(PrefExportScaleMultiplier, exportScaleMultiplier);
    EditorPrefs.SetBool(PrefAtlasBatchExportedMats, atlasBatchExportedMats);
    EditorPrefs.SetInt(PrefBatchAtlasMaxSize, batchAtlasMaxSize);
    EditorPrefs.SetString(PrefSingleModelBatchRangeText, singleModelBatchRangeText ?? string.Empty);
    EditorPrefs.SetString(PrefSingleFlatBatchRangeText, singleFlatBatchRangeText ?? string.Empty);
EditorPrefs.SetInt(PrefMobileSpriteSortMode, mobileSpriteSortMode);
EditorPrefs.SetBool("NexusDev.ExportDfuModelsWest.BlenderImportScriptExpanded", blenderImportScriptExpanded);
    EditorPrefs.SetBool(PrefCategoryBatchExpanded, categoryBatchExpanded);
    EditorPrefs.SetBool(PrefFlatCategoryBatchExpanded, flatCategoryBatchExpanded);
    EditorPrefs.SetString(PrefCategoryBatchSelected, JoinCategoryBatchSelections(selectedCategoryBatchSources));
    EditorPrefs.SetString(PrefFlatCategoryBatchSelected, JoinCategoryBatchSelections(selectedFlatCategoryBatchSources));

    for (int i = 0; i < FixedCategoryBatchSources.Length; i++)
    {
        string fieldName = FixedCategoryBatchSources[i];
        double seconds;

        if (categoryLastDurationSeconds.TryGetValue(fieldName + "|Base", out seconds))
            EditorPrefs.SetFloat(MakeCategoryDurationPrefKey(fieldName, false), (float)seconds);

        if (categoryLastDurationSeconds.TryGetValue(fieldName + "|Grouped", out seconds))
            EditorPrefs.SetFloat(MakeCategoryDurationPrefKey(fieldName, true), (float)seconds);
    }

    string[] flatSources = GetAvailableFlatCategoryBatchSources();
    for (int i = 0; i < flatSources.Length; i++)
    {
        string fieldName = flatSources[i];
        double seconds;

        if (categoryLastDurationSeconds.TryGetValue("FlatCategory|" + fieldName + "|Base", out seconds))
            EditorPrefs.SetFloat(MakeFlatCategoryDurationPrefKey(fieldName, false), (float)seconds);

        if (categoryLastDurationSeconds.TryGetValue("FlatCategory|" + fieldName + "|Grouped", out seconds))
            EditorPrefs.SetFloat(MakeFlatCategoryDurationPrefKey(fieldName, true), (float)seconds);
    }

    string[] mobileOps = GetKnownMobileDurationOperationNames();
    for (int i = 0; i < mobileOps.Length; i++)
    {
        double seconds;
        if (categoryLastDurationSeconds.TryGetValue(GetMobileDurationKey(mobileOps[i]), out seconds))
            EditorPrefs.SetFloat(MakeMobileDurationPrefKey(mobileOps[i]), (float)seconds);
    }

    MarkFlatCategorySourcesDirty();
    InvalidateMobileListCaches();
}
private void ExportSingleConfiguredModel()
{
    string exporterError;
    if (!HasFbxExporter(out exporterError))
    {
        Debug.LogError("[NexusDev][DFU Export] " + exporterError);
        return;
    }

    if (!ValidateBeforeExport())
        return;

    EnsureExportFolders();

    List<uint> ids = exportGroupedFamilyMembers
        ? ExpandModelIdsByGroupedFamiliesForced(new List<uint> { singleModelId })
        : new List<uint> { singleModelId };

    Debug.Log(string.Format(
        CultureInfo.InvariantCulture,
        "[NexusDev][DFU Export] Exporting single model request {0} resolved to {1} model ID(s).",
        singleModelId,
        ids.Count));

    int successCount = 0;
    int failCount = 0;

    try
    {
        for (int i = 0; i < ids.Count; i++)
        {
            EditorUtility.DisplayProgressBar(
                "DFU Export",
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Exporting model {0} ({1}/{2})",
                    ids[i],
                    i + 1,
                    ids.Count),
                ids.Count > 0 ? (float)i / ids.Count : 0f);

            bool success = ExportOneModel(
                ids[i],
                BuildResolvedExportBaseNameForModelId(ids[i]));

            if (success) successCount++;
            else failCount++;
        }
    }
    finally
    {
        EditorUtility.ClearProgressBar();
    }

       if (ShouldPerformImmediateAssetRefresh())
    {
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    Debug.Log(string.Format(
        CultureInfo.InvariantCulture,
        "[NexusDev][DFU Export] Single export finished. Success: {0}, Failed: {1}",
        successCount,
        failCount));
}

private void ExportSingleConfiguredFlat()
{
    string exporterError;
    if (!HasFbxExporter(out exporterError))
    {
        Debug.LogError("[NexusDev][DFU Export] " + exporterError);
        return;
    }

    if (!ValidateBeforeExport())
        return;

    EnsureExportFolders();
    int archive;
    int record;
    if (!TryParseFlatArchiveRecord(singleFlatIdText, out archive, out record))
    {
        Debug.LogError("[NexusDev][DFU Export] Invalid flat archive.record value: '" + singleFlatIdText + "'");
        return;
    }

    List<ExportRequest> requests = ExpandFlatRequestsByGroupedFamilies(new List<ExportRequest>
    {
        new ExportRequest
        {
            IsFlat = true,
            FlatArchive = archive,
            FlatRecord = record,
        }
    });

    bool success = true;
    try
    {
        for (int i = 0; i < requests.Count; i++)
        {
            ExportRequest request = requests[i];

            EditorUtility.DisplayProgressBar(
                "DFU Export",
                "Exporting flat " +
                request.FlatArchive.ToString(CultureInfo.InvariantCulture) + "." +
                request.FlatRecord.ToString(CultureInfo.InvariantCulture) +
                " (" + (i + 1).ToString(CultureInfo.InvariantCulture) + "/" +
                requests.Count.ToString(CultureInfo.InvariantCulture) + ")",
                requests.Count > 0 ? (float)i / requests.Count : 0f);

            bool oneSuccess = ExportOneFlat(
                request.FlatArchive,
                request.FlatRecord,
                BuildResolvedExportBaseNameForFlat(request.FlatArchive, request.FlatRecord),
                null,
                false);

            if (!oneSuccess)
                success = false;
        }
    }
       finally
    {
        EditorUtility.ClearProgressBar();

        if (ShouldPerformImmediateAssetRefresh())
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    if (success)
    {
        Debug.Log("[NexusDev][DFU Export] Single flat export finished successfully.");
    }
    else
    {
        Debug.LogError("[NexusDev][DFU Export] Single flat export failed.");
    }
}

private void ExportModelIdsFromManualRange()
{
    if (string.IsNullOrWhiteSpace(singleModelBatchRangeText))
    {
        Debug.LogWarning("[NexusDev][DFU Export] Model manual range field is empty.");
        return;
    }

    ExportRequestsFromSourceText(singleModelBatchRangeText, "model manual range field", true, false);
}

private void ExportFlatIdsFromManualRange()
{
    if (string.IsNullOrWhiteSpace(singleFlatBatchRangeText))
    {
        Debug.LogWarning("[NexusDev][DFU Export] Flat manual list field is empty.");
        return;
    }

    ExportRequestsFromSourceText(singleFlatBatchRangeText, "flat manual list field", false, true);
}

private void ExportModelIdsFromTxtOnly()
{
    if (string.IsNullOrEmpty(singleModelTxtPath) || !File.Exists(singleModelTxtPath))
    {
        Debug.LogError("[NexusDev][DFU Export] Model txt file not found: " + singleModelTxtPath);
        return;
    }

    string text = File.ReadAllText(singleModelTxtPath);
    ExportRequestsFromSourceText(text, singleModelTxtPath, true, false);
}

private void ExportFlatIdsFromTxtOnly()
{
    if (string.IsNullOrEmpty(singleFlatTxtPath) || !File.Exists(singleFlatTxtPath))
    {
        Debug.LogError("[NexusDev][DFU Export] Flat txt file not found: " + singleFlatTxtPath);
        return;
    }

    string text = File.ReadAllText(singleFlatTxtPath);
    ExportRequestsFromSourceText(text, singleFlatTxtPath, false, true);
}

private void ExportRequestsFromSourceText(string text, string sourceLabel, bool includeModels, bool includeFlats)
{
    string exporterError;
    if (!HasFbxExporter(out exporterError))
    {
        Debug.LogError("[NexusDev][DFU Export] " + exporterError);
        return;
    }

    if (!ValidateBeforeExport())
        return;

    EnsureExportFolders();

    List<ExportRequest> parsedRequests = ParseExportRequestsAndRanges(text);
    if (parsedRequests == null || parsedRequests.Count == 0)
    {
        Debug.LogWarning("[NexusDev][DFU Export] No valid model IDs or flat archive.record entries found in " + sourceLabel);
        return;
    }

    List<ExportRequest> filteredRequests = new List<ExportRequest>();
    for (int i = 0; i < parsedRequests.Count; i++)
    {
        if (parsedRequests[i].IsFlat && includeFlats)
            filteredRequests.Add(parsedRequests[i]);
        else if (!parsedRequests[i].IsFlat && includeModels)
            filteredRequests.Add(parsedRequests[i]);
    }

    if (filteredRequests.Count == 0)
    {
        Debug.LogWarning("[NexusDev][DFU Export] No matching export entries found in " + sourceLabel);
        return;
    }

    List<ExportRequest> expandedRequests = new List<ExportRequest>();

    for (int i = 0; i < filteredRequests.Count; i++)
    {
        ExportRequest request = filteredRequests[i];

        if (request.IsFlat)
        {
            List<ExportRequest> expandedFlatRequests = ExpandFlatRequestsByGroupedFamilies(
                new List<ExportRequest> { request });

            for (int j = 0; j < expandedFlatRequests.Count; j++)
                expandedRequests.Add(expandedFlatRequests[j]);
        }
        else
        {
            List<uint> expandedModelIds = ExpandModelIdsByGroupedFamilies(new List<uint> { request.ModelId });
            for (int j = 0; j < expandedModelIds.Count; j++)
            {
                expandedRequests.Add(new ExportRequest
                {
                    IsFlat = false,
                    ModelId = expandedModelIds[j],
                });
            }
        }
    }

    if (includeModels && !includeFlats && atlasBatchExportedMats)
    {
        List<uint> modelIds = new List<uint>();
        for (int i = 0; i < expandedRequests.Count; i++)
        {
            if (!expandedRequests[i].IsFlat)
                modelIds.Add(expandedRequests[i].ModelId);
        }

        Debug.Log(string.Format(
            CultureInfo.InvariantCulture,
            "[NexusDev][DFU Export] Parsed {0} model ID(s) from: {1}",
            modelIds.Count,
            sourceLabel));

        ExportIdsFromTxtSharedAtlas(modelIds);
        return;
    }

    int successCount = 0;
    int failCount = 0;

    try
    {
        for (int i = 0; i < expandedRequests.Count; i++)
        {
            ExportRequest request = expandedRequests[i];
            string progressLabel = request.IsFlat
                ? ("flat " + request.FlatArchive.ToString(CultureInfo.InvariantCulture) + "." + request.FlatRecord.ToString(CultureInfo.InvariantCulture))
                : ("model " + request.ModelId.ToString(CultureInfo.InvariantCulture));

            EditorUtility.DisplayProgressBar(
                "DFU Export",
                string.Format("Exporting {0} ({1}/{2})", progressLabel, i + 1, expandedRequests.Count),
                expandedRequests.Count > 0 ? (float)i / expandedRequests.Count : 0f);

            bool success;
            if (request.IsFlat)
            {
                success = ExportOneFlat(
                    request.FlatArchive,
                    request.FlatRecord,
                    BuildResolvedExportBaseNameForFlat(request.FlatArchive, request.FlatRecord),
                    null,
                    false);
            }
            else
            {
                success = ExportOneModel(
                    request.ModelId,
                    BuildResolvedExportBaseNameForModelId(request.ModelId));
            }

            if (success) successCount++;
            else failCount++;
        }
    }
    finally
    {
        EditorUtility.ClearProgressBar();
    }

    if (ShouldPerformImmediateAssetRefresh())
    {
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    Debug.Log(string.Format(
        "[NexusDev][DFU Export] Batch export finished. Success: {0}, Failed: {1}",
        successCount,
        failCount));
}


private void ExportSelectedCategoryBatches()
{
    EnsureAtLeastOneCategoryBatchRow();

    BeginDeferredAssetEditing();
    try
    {
        for (int i = 0; i < selectedCategoryBatchSources.Count; i++)
            ExportOneFixedCategory(selectedCategoryBatchSources[i]);
    }
    finally
    {
        EndDeferredAssetEditing(true);
    }
}

private void ExportSelectedFlatCategoryBatches()
{
    EnsureAtLeastOneFlatCategoryBatchRow();

    BeginDeferredAssetEditing();
    try
    {
        for (int i = 0; i < selectedFlatCategoryBatchSources.Count; i++)
            ExportOneFixedFlatCategory(selectedFlatCategoryBatchSources[i]);
    }
    finally
    {
        EndDeferredAssetEditing(true);
    }
}

private void ExportAllFlatCategories()
{
    string[] flatSources = GetAvailableFlatCategoryBatchSources();

    BeginDeferredAssetEditing();
    try
    {
        for (int i = 0; i < flatSources.Length; i++)
            ExportOneFixedFlatCategory(flatSources[i]);
    }
    finally
    {
        EndDeferredAssetEditing(true);
    }
}

private void ExportOneFixedFlatCategory(string fieldName)
{
    DateTime startUtc = DateTime.UtcNow;
    int successCount = 0;
    int failCount = 0;

    try
    {
        ExportSelectedFlatCategoryBatchInternal(fieldName, out successCount, out failCount);
    }
    finally
    {
        double seconds = (DateTime.UtcNow - startUtc).TotalSeconds;
        SaveFlatCategoryDuration(fieldName, seconds);

        Debug.Log(string.Format(
            CultureInfo.InvariantCulture,
            "[NexusDev][DFU Export] Flat category '{0}' finished in {1}. Success: {2}, Failed: {3}",
            fieldName,
            FormatDurationSeconds(seconds),
            successCount,
            failCount));
    }
}
private void ExportSelectedFlatCategoryBatchInternal(string fieldName, out int totalSuccess, out int totalFail)
{
    totalSuccess = 0;
    totalFail = 0;

    string exporterError;
    if (!HasFbxExporter(out exporterError))
    {
        Debug.LogError("[NexusDev][DFU Export] " + exporterError);
        return;
    }

    if (!ValidateBeforeExport())
        return;

    EnsureExportFolders();

    List<ExportRequest> requests;
    string error;
    if (!TryGetFlatRequestsFromCategorySource(fieldName, out requests, out error))
    {
        Debug.LogWarning("[NexusDev][DFU Export] Skipping flat category batch '" + fieldName + "'. " + error);
        totalFail++;
        return;
    }

       requests = ExpandFlatRequestsByGroupedFamilies(requests);

    Debug.Log(string.Format(
        "[NexusDev][DFU Export] Flat category batch '{0}' resolved to {1} export flat(s).",
        fieldName,
        requests.Count));
    try
    {
                if (atlasBatchExportedMats)
        {
            int successCount;
            int failCount;
            string sharedBatchAssetBaseName = BuildSharedFlatBatchAssetBaseName(fieldName);
            ExportFlatRequestsSharedAtlasWithBaseName(requests, sharedBatchAssetBaseName, fieldName, out successCount, out failCount);
            totalSuccess += successCount;
            totalFail += failCount;
        }
        else
        {
            for (int i = 0; i < requests.Count; i++)
            {
                ExportRequest request = requests[i];

                EditorUtility.DisplayProgressBar(
    "DFU Export",
    BuildFlatBatchProgressLabel(
        "Exporting",
        fieldName,
        request.FlatArchive,
        request.FlatRecord,
        i + 1,
        requests.Count),
    requests.Count > 0 ? (float)i / requests.Count : 0f);

                bool success = ExportOneFlat(
                    request.FlatArchive,
                    request.FlatRecord,
                    BuildResolvedExportBaseNameForFlat(fieldName, request.FlatArchive, request.FlatRecord),
                    fieldName,
                    categoryBatchFbxSubfolders);

                if (success) totalSuccess++;
                else totalFail++;
            }
        }
    }
       finally
    {
        EditorUtility.ClearProgressBar();

        if (ShouldPerformImmediateAssetRefresh())
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}

private void ExportSelectedCategoryBatchInternal(string fieldName, out int totalSuccess, out int totalFail)
{
    totalSuccess = 0;
    totalFail = 0;

    string exporterError;
    if (!HasFbxExporter(out exporterError))
    {
        Debug.LogError("[NexusDev][DFU Export] " + exporterError);
        return;
    }

    if (!ValidateBeforeExport())
        return;

    EnsureExportFolders();

    List<uint> ids;
    string error;
    if (!TryGetModelIdsFromCategorySource(fieldName, out ids, out error))
    {
        Debug.LogWarning("[NexusDev][DFU Export] Skipping category batch '" + fieldName + "'. " + error);
        totalFail++;
        return;
    }

    ids = ExpandModelIdsByGroupedFamilies(ids);

    Debug.Log(string.Format(
        "[NexusDev][DFU Export] Category batch '{0}' resolved to {1} export model ID(s).",
        fieldName,
        ids.Count));

    try
    {
        if (atlasBatchExportedMats)
            {
                int successCount;
                int failCount;
                string sharedBatchAssetBaseName = BuildSharedBatchAssetBaseName(fieldName);
                ExportIdsSharedAtlasWithBaseName(ids, sharedBatchAssetBaseName, fieldName, out successCount, out failCount);
                totalSuccess += successCount;
                totalFail += failCount;
            }
        else
        {
            int successCount = 0;
            int failCount = 0;

            for (int idIndex = 0; idIndex < ids.Count; idIndex++)
            {
                uint modelId = ids[idIndex];

                EditorUtility.DisplayProgressBar(
                    "DFU Export",
                    string.Format("Exporting {0} model {1} ({2}/{3})", fieldName, modelId, idIndex + 1, ids.Count),
                    ids.Count > 0 ? (float)idIndex / ids.Count : 0f);

                string exportBaseName = BuildResolvedExportBaseNameForModelId(fieldName, modelId);
                bool success = ExportOneModel(
                    modelId,
                    exportBaseName,
                    fieldName,
                    categoryBatchFbxSubfolders);

                if (success) successCount++;
                else failCount++;
            }

            totalSuccess += successCount;
            totalFail += failCount;
        }
    }
        finally
    {
        EditorUtility.ClearProgressBar();

        if (ShouldPerformImmediateAssetRefresh())
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
private void ExportIdsIndividually(List<uint> ids, string sourceLabel, out int successCount, out int failCount)
{
    successCount = 0;
    failCount = 0;

    if (ids == null || ids.Count == 0)
        return;

    for (int i = 0; i < ids.Count; i++)
    {
        EditorUtility.DisplayProgressBar(
            "DFU Export",
            string.Format("Exporting {0} model {1} ({2}/{3})", sourceLabel, ids[i], i + 1, ids.Count),
            ids.Count > 0 ? (float)i / ids.Count : 0f);

        bool success = ExportOneModel(ids[i], BuildResolvedExportBaseNameForModelId(ids[i]));
        if (success) successCount++;
        else failCount++;
    }
}

private void ExportIdsSharedAtlasWithBaseName(List<uint> ids, string batchBaseName, string categoryFieldNameOrNull, out int successCount, out int failCount)
{
    successCount = 0;
    failCount = 0;

    if (ShouldSkipSharedBatchBecauseAssetsExist(batchBaseName))
    {
        Debug.LogWarning("[NexusDev][DFU Export] Skipping shared batch '" + batchBaseName + "' because overwrite is off and batch atlas/material assets already exist.");
        return;
    }

    Dictionary<string, Texture2D> uniqueTextures = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);
    Dictionary<string, BatchTexturePlacement> placements = new Dictionary<string, BatchTexturePlacement>(StringComparer.OrdinalIgnoreCase);
    List<BatchTexturePage> pages = new List<BatchTexturePage>();
    List<BatchModelPlan> plans = new List<BatchModelPlan>();

    try
    {
        for (int i = 0; i < ids.Count; i++)
        {
            EditorUtility.DisplayProgressBar(
                "DFU Export",
                string.Format("Analyzing {0} model {1} ({2}/{3})", batchBaseName, ids[i], i + 1, ids.Count),
                ids.Count > 0 ? (float)i / ids.Count : 0f);

            uint modelId = ids[i];
            string exportBaseName = BuildResolvedExportBaseNameForModelId(categoryFieldNameOrNull, modelId);

            BatchModelPlan plan;
            string error;
            if (!TryBuildBatchModelPlan(
                modelId,
                exportBaseName,
                categoryFieldNameOrNull,
                categoryBatchFbxSubfolders,
                out plan,
                out error))
            {
                Debug.LogWarning("[NexusDev][DFU Export] Skipping model " + modelId + ". " + error);
                failCount++;
                continue;
            }

            plans.Add(plan);

            for (int t = 0; t < plan.TextureKeys.Length; t++)
            {
                string key = plan.TextureKeys[t];
                if (string.IsNullOrEmpty(key) || uniqueTextures.ContainsKey(key))
                    continue;

                Texture2D tex;
                if (!TryLoadDfuTexture(key, out tex, out error))
                {
                    Debug.LogError("[NexusDev][DFU Export] Missing batch texture " + key + " for model " + plan.ModelId + ". " + error);
                    continue;
                }

                uniqueTextures.Add(key, tex);
            }
        }

        if (plans.Count == 0)
        {
            Debug.LogWarning("[NexusDev][DFU Export] No valid models were found in batch '" + batchBaseName + "'.");
            return;
        }

        string packError;
        if (!TryBuildSharedBatchAtlasPages(uniqueTextures, batchBaseName, batchAtlasMaxSize, out pages, out placements, out packError))
        {
            Debug.LogError("[NexusDev][DFU Export] Shared atlas build failed for '" + batchBaseName + "'. " + packError);
            failCount += plans.Count;
            return;
        }

        for (int i = 0; i < plans.Count; i++)
        {
            if (!overwriteExistingExportAssets && AssetExistsAtAssetPath(
                BuildFbxAssetRelativePath(
                    plans[i].ExportBaseName,
                    plans[i].CategoryFieldName,
                    plans[i].UseCategorySubfolder)))
            {
                Debug.LogWarning("[NexusDev][DFU Export] Skipping FBX '" + plans[i].ExportBaseName + "' because overwrite is off and it already exists.");
                continue;
            }

            EditorUtility.DisplayProgressBar(
                "DFU Export",
                string.Format("Exporting {0} model {1} ({2}/{3})", batchBaseName, plans[i].ModelId, i + 1, plans.Count),
                plans.Count > 0 ? (float)i / plans.Count : 0f);

            string error;
            if (ExportOneModelSharedAtlas(plans[i], pages, placements, out error))
                successCount++;
            else
            {
                Debug.LogError("[NexusDev][DFU Export] Shared-atlas export failed for model " + plans[i].ModelId + ". " + error);
                failCount++;
            }
        }
    }
    finally
    {
        for (int i = 0; i < plans.Count; i++)
        {
            if (plans[i] != null && plans[i].Mesh != null)
                DestroyImmediate(plans[i].Mesh);
        }

        foreach (Texture2D tex in uniqueTextures.Values)
        {
            if (tex != null)
                DestroyImmediate(tex);
        }
    }

    Debug.Log(string.Format(
        "[NexusDev][DFU Export] Category/shared-atlas batch '{0}' finished. Success: {1}, Failed: {2}, Atlas Pages: {3}",
        batchBaseName,
        successCount,
        failCount,
        pages.Count));
}
private void ExportIdsFromTxtSharedAtlas(List<uint> ids)
{
    int successCount;
    int failCount;
    ExportIdsSharedAtlasWithBaseName(ids, BuildBatchBaseName(ids), null, out successCount, out failCount);

    Debug.Log(string.Format(
        "[NexusDev][DFU Export] Shared-atlas batch export finished. Success: {0}, Failed: {1}",
        successCount,
        failCount));
}

private void ExportFlatRequestsSharedAtlasWithBaseName(
    List<ExportRequest> requests,
    string batchBaseName,
    string categoryFieldNameOrNull,
    out int successCount,
    out int failCount)
{
    successCount = 0;
    failCount = 0;

    if (ShouldSkipSharedBatchBecauseAssetsExist(batchBaseName))
    {
        Debug.LogWarning("[NexusDev][DFU Export] Skipping shared flat batch '" + batchBaseName + "' because overwrite is off and batch atlas/material assets already exist.");
        return;
    }

    Dictionary<string, Texture2D> uniqueTextures = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);
    Dictionary<string, BatchTexturePlacement> placements = new Dictionary<string, BatchTexturePlacement>(StringComparer.OrdinalIgnoreCase);
    List<BatchTexturePage> pages = new List<BatchTexturePage>();
    List<BatchFlatPlan> plans = new List<BatchFlatPlan>();

    try
    {
        for (int i = 0; i < requests.Count; i++)
        {
            ExportRequest request = requests[i];
EditorUtility.DisplayProgressBar(
    "DFU Export",
    BuildFlatBatchProgressLabel(
        "Analyzing",
        batchBaseName,
        request.FlatArchive,
        request.FlatRecord,
        i + 1,
        requests.Count),
    requests.Count > 0 ? (float)i / requests.Count : 0f);

            string exportBaseName = BuildResolvedExportBaseNameForFlat(
                categoryFieldNameOrNull,
                request.FlatArchive,
                request.FlatRecord);

            BatchFlatPlan plan;
            string error;
            if (!TryBuildBatchFlatPlan(
                request.FlatArchive,
                request.FlatRecord,
                exportBaseName,
                categoryFieldNameOrNull,
                categoryBatchFbxSubfolders,
                out plan,
                out error))
            {
                Debug.LogWarning("[NexusDev][DFU Export] Skipping flat " + request.FlatArchive + "." + request.FlatRecord + ". " + error);
                failCount++;
                continue;
            }

            plans.Add(plan);

            if (!string.IsNullOrEmpty(plan.TextureKey) && !uniqueTextures.ContainsKey(plan.TextureKey))
            {
                Texture2D tex;
                if (!TryLoadDfuTexture(plan.TextureKey, out tex, out error))
                {
                    Debug.LogError("[NexusDev][DFU Export] Missing batch flat texture " + plan.TextureKey + " for flat " + plan.FlatArchive + "." + plan.FlatRecord + ". " + error);
                    continue;
                }

                uniqueTextures.Add(plan.TextureKey, tex);
            }
        }

        if (plans.Count == 0)
        {
            Debug.LogWarning("[NexusDev][DFU Export] No valid flats were found in batch '" + batchBaseName + "'.");
            return;
        }

        string packError;
        if (!TryBuildSharedBatchAtlasPages(uniqueTextures, batchBaseName, batchAtlasMaxSize, out pages, out placements, out packError))
        {
            Debug.LogError("[NexusDev][DFU Export] Shared flat atlas build failed for '" + batchBaseName + "'. " + packError);
            failCount += plans.Count;
            return;
        }

        for (int i = 0; i < pages.Count; i++)
            ApplyStandardCutoutMaterialSettings(pages[i].MaterialAsset);

        AssetDatabase.SaveAssets();

        for (int i = 0; i < plans.Count; i++)
        {
            string flatFbxAssetPath = plans[i].UseCategorySubfolder && !string.IsNullOrWhiteSpace(plans[i].CategoryFieldName)
                ? BuildFlatCategoryFbxFolderAssetRelativePath(plans[i].CategoryFieldName) + "/" + SanitizeExportToken(plans[i].ExportBaseName) + ".fbx"
                : BuildFlatFbxAssetRelativePath(plans[i].ExportBaseName);

            if (!overwriteExistingExportAssets && AssetExistsAtAssetPath(flatFbxAssetPath))
            {
                Debug.LogWarning("[NexusDev][DFU Export] Skipping flat FBX '" + plans[i].ExportBaseName + "' because overwrite is off and it already exists.");
                continue;
            }

            EditorUtility.DisplayProgressBar(
    "DFU Export",
    BuildFlatBatchProgressLabel(
        "Exporting",
        batchBaseName,
        plans[i].FlatArchive,
        plans[i].FlatRecord,
        i + 1,
        plans.Count),
    plans.Count > 0 ? (float)i / plans.Count : 0f);

            string error;
            if (ExportOneFlatSharedAtlas(plans[i], pages, placements, out error))
                successCount++;
            else
            {
                Debug.LogError("[NexusDev][DFU Export] Shared-atlas export failed for flat " + plans[i].FlatArchive + "." + plans[i].FlatRecord + ". " + error);
                failCount++;
            }
        }
    }
    finally
    {
        for (int i = 0; i < plans.Count; i++)
        {
            if (plans[i] != null && plans[i].Mesh != null)
                DestroyImmediate(plans[i].Mesh);
        }

        foreach (Texture2D tex in uniqueTextures.Values)
        {
            if (tex != null)
                DestroyImmediate(tex);
        }
    }

    Debug.Log(string.Format(
        "[NexusDev][DFU Export] Flat/shared-atlas batch '{0}' finished. Success: {1}, Failed: {2}, Atlas Pages: {3}",
        batchBaseName,
        successCount,
        failCount,
        pages.Count));
}
private bool TryBuildBatchFlatPlan(
    int archive,
    int record,
    string exportBaseName,
    string categoryFieldNameOrNull,
    bool useCategorySubfolder,
    out BatchFlatPlan plan,
    out string error)
{
    plan = null;
    error = null;

    GameObject sourceGo = null;

    try
    {
        sourceGo = new GameObject(exportBaseName + "_SOURCE");
        MeshFilter mf = sourceGo.AddComponent<MeshFilter>();
        sourceGo.AddComponent<MeshRenderer>();
        global::DaggerfallWorkshop.DaggerfallBillboard billboard = sourceGo.AddComponent<global::DaggerfallWorkshop.DaggerfallBillboard>();

        Material runtimeMaterial;
        try
        {
            runtimeMaterial = billboard.SetMaterial(archive, record, 0);
            billboard.AlignToBase();
        }
        catch (Exception ex)
        {
            error = "DaggerfallBillboard.SetMaterial/AlignToBase failed for flat " +
                    archive.ToString(CultureInfo.InvariantCulture) + "." +
                    record.ToString(CultureInfo.InvariantCulture) + ". Exception: " + ex.Message;
            return false;
        }

        if (mf.sharedMesh == null)
        {
            error = "Flat mesh was null.";
            return false;
        }

        if (runtimeMaterial == null)
        {
            error = "Flat runtime material was null.";
            return false;
        }

        plan = new BatchFlatPlan();
        plan.FlatArchive = archive;
        plan.FlatRecord = record;
        plan.ExportBaseName = string.IsNullOrEmpty(exportBaseName)
            ? BuildDefaultExportBaseNameForFlat(archive, record)
            : exportBaseName;
        plan.CategoryFieldName = categoryFieldNameOrNull;
        plan.UseCategorySubfolder = useCategorySubfolder;
        plan.Mesh = Instantiate(mf.sharedMesh);
        plan.Mesh.name = plan.ExportBaseName + "_BatchMesh";
        RemapMeshUvInPlace(plan.Mesh, Vector2.one, Vector2.zero);
        plan.TextureKey = MakeTextureKey(archive, record);

        WriteTempFlatTextureIfKeepingTemps(plan.ExportBaseName, plan.TextureKey);

        return true;
    }
    finally
    {
        if (sourceGo != null)
            DestroyImmediate(sourceGo);
    }
}

private void WriteTempFlatTextureIfKeepingTemps(string exportBaseName, string textureKey)
{
    if (deleteTempTextureFilesAfterExport)
        return;

    Texture2D readableTexture;
    string textureError;
    if (!TryLoadDfuTexture(textureKey, out readableTexture, out textureError))
    {
        Debug.LogWarning("[NexusDev][DFU Export] Could not write temp flat texture for '" + exportBaseName + "'. " + textureError);
        return;
    }

    try
    {
        string tempTextureAssetRelative = BuildSingleFlatTextureAssetRelativePath(exportBaseName);
        string tempTextureAbsolutePath = GetAbsolutePathFromAssetPath(tempTextureAssetRelative);

        File.WriteAllBytes(tempTextureAbsolutePath, readableTexture.EncodeToPNG());
        AssetDatabase.ImportAsset(tempTextureAssetRelative, ImportAssetOptions.ForceUpdate);
        ConfigureImportedTextureForPointSampling(tempTextureAssetRelative);
    }
    finally
    {
        if (readableTexture != null)
            DestroyImmediate(readableTexture);
    }
}

private bool ExportOneFlatSharedAtlas(
    BatchFlatPlan plan,
    List<BatchTexturePage> pages,
    Dictionary<string, BatchTexturePlacement> placements,
    out string error)
{
    error = null;

    if (plan == null || plan.Mesh == null)
    {
        error = "Plan or mesh was null.";
        return false;
    }

    List<int> localPageIndices;
    Mesh remappedMesh = BuildSharedBatchAtlasedMesh(plan.Mesh, new string[] { plan.TextureKey }, placements, out localPageIndices);
    if (remappedMesh == null)
    {
        error = "Failed building shared batch atlased flat mesh.";
        return false;
    }

    GameObject exportGo = null;
    try
    {
        exportGo = new GameObject(plan.ExportBaseName);
        MeshFilter mf = exportGo.AddComponent<MeshFilter>();
        MeshRenderer mr = exportGo.AddComponent<MeshRenderer>();

        mf.sharedMesh = remappedMesh;
        exportGo.transform.localScale = Vector3.one * exportScaleMultiplier;

        Material[] mats = new Material[localPageIndices.Count];
        for (int i = 0; i < localPageIndices.Count; i++)
            mats[i] = pages[localPageIndices[i]].MaterialAsset;

        mr.sharedMaterials = mats;

        string exportPathAssetRelative = plan.UseCategorySubfolder && !string.IsNullOrWhiteSpace(plan.CategoryFieldName)
            ? BuildFlatCategoryFbxFolderAssetRelativePath(plan.CategoryFieldName) + "/" + SanitizeExportToken(plan.ExportBaseName) + ".fbx"
            : BuildFlatFbxAssetRelativePath(plan.ExportBaseName);

        string exportedAssetPath;
        string exportError;
        bool ok = ExportFbxViaReflection(exportPathAssetRelative, exportGo, out exportedAssetPath, out exportError);

        if (!ok)
        {
            error = exportError;
            return false;
        }

        bool pausedForFbx = PauseDeferredAssetEditingForImmediateImports();
        try
        {
            AssetDatabase.ImportAsset(
                exportedAssetPath,
                ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
        }
        finally
        {
            ResumeDeferredAssetEditingAfterImmediateImports(pausedForFbx);
        }

        string remapError;
        if (!RemapImportedFbxMaterials(exportedAssetPath, mats, out remapError))
        {
            error = "FBX material remap failed. " + remapError;
            return false;
        }

        Debug.Log("[NexusDev][DFU Export] SUCCESS shared-atlas flat " + plan.FlatArchive + "." + plan.FlatRecord + " -> " + exportedAssetPath);
        return true;
    }
    finally
    {
        if (exportGo != null)
            DestroyImmediate(exportGo);

        if (remappedMesh != null)
            DestroyImmediate(remappedMesh);
    }
}
private bool TryBuildBatchModelPlan(
    uint modelId,
    string exportBaseName,
    string categoryFieldNameOrNull,
    bool useCategorySubfolder,
    out BatchModelPlan plan,
    out string error)
{
    plan = null;
    error = null;

    GameObject sourceGo = null;

    try
    {
        sourceGo = global::DaggerfallWorkshop.Utility.GameObjectHelper.CreateDaggerfallMeshGameObject(modelId, null);
        if (sourceGo == null)
        {
            error = "CreateDaggerfallMeshGameObject returned null.";
            return false;
        }

        MeshFilter mf = sourceGo.GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null)
        {
            error = "Model has no mesh.";
            return false;
        }

        string[] textureKeys;
        if (!TryGetTextureKeysForModel(modelId, out textureKeys, out error))
            return false;

        plan = new BatchModelPlan();
        plan.ModelId = modelId;
        plan.ExportBaseName = string.IsNullOrEmpty(exportBaseName) ? BuildDefaultExportBaseName(modelId) : exportBaseName;
        plan.CategoryFieldName = categoryFieldNameOrNull;
        plan.UseCategorySubfolder = useCategorySubfolder;
        plan.Mesh = Instantiate(mf.sharedMesh);
        plan.Mesh.name = plan.ExportBaseName + "_BatchMesh";
        plan.TextureKeys = textureKeys;
        return true;
    }
    finally
    {
        if (sourceGo != null)
            DestroyImmediate(sourceGo);
    }
}
private bool TryBuildSharedBatchAtlasPages(
    Dictionary<string, Texture2D> textures,
    string batchBaseName,
    int maxAtlasSize,
    out List<BatchTexturePage> pages,
    out Dictionary<string, BatchTexturePlacement> placements,
    out string error)
{
    pages = new List<BatchTexturePage>();
    placements = new Dictionary<string, BatchTexturePlacement>(StringComparer.OrdinalIgnoreCase);
    error = null;

    if (textures == null || textures.Count == 0)
    {
        error = "No textures were available for shared atlas build.";
        return false;
    }

    List<KeyValuePair<string, Texture2D>> remaining = new List<KeyValuePair<string, Texture2D>>(textures);
    remaining.Sort((a, b) =>
    {
        int aMax = Mathf.Max(a.Value.width, a.Value.height);
        int bMax = Mathf.Max(b.Value.width, b.Value.height);
        int cmp = bMax.CompareTo(aMax);
        if (cmp != 0) return cmp;

        int aArea = a.Value.width * a.Value.height;
        int bArea = b.Value.width * b.Value.height;
        return bArea.CompareTo(aArea);
    });

    List<BatchPackCandidate> candidates = new List<BatchPackCandidate>();

    while (remaining.Count > 0)
    {
        BatchPackCandidate candidate = ChooseBestPackCandidate(remaining, maxAtlasSize);
        if (candidate == null || candidate.PackedKeys.Count == 0)
        {
            error = "Could not pack remaining textures into any atlas size up to " + maxAtlasSize + ".";
            return false;
        }

        candidates.Add(candidate);

        HashSet<string> packedSet = new HashSet<string>(candidate.PackedKeys, StringComparer.OrdinalIgnoreCase);
        remaining.RemoveAll(kvp => packedSet.Contains(kvp.Key));
    }

    int totalPages = candidates.Count;

    for (int pageIndex = 0; pageIndex < candidates.Count; pageIndex++)
    {
        BatchPackCandidate candidate = candidates[pageIndex];
        BatchTexturePage page = CreateBatchTexturePage(pageIndex, candidate.Width, candidate.Height);
        page.AtlasTexture = new Texture2D(candidate.Width, candidate.Height, TextureFormat.RGBA32, false);
        page.AtlasTexture.filterMode = FilterMode.Point;
        page.AtlasTexture.wrapMode = TextureWrapMode.Clamp;

        Color[] clear = new Color[candidate.Width * candidate.Height];
        for (int i = 0; i < clear.Length; i++)
            clear[i] = new Color(0f, 0f, 0f, 0f);
        page.AtlasTexture.SetPixels(clear);

        for (int i = 0; i < candidate.PackedKeys.Count; i++)
        {
            string key = candidate.PackedKeys[i];
            Texture2D tex = textures[key];
            BatchTexturePlacement placement = candidate.Placements[key];
            placement.PageIndex = page.PageIndex;

            page.AtlasTexture.SetPixels(
                placement.PixelX,
                placement.PixelY,
                placement.Width,
                placement.Height,
                tex.GetPixels());

            placements[key] = placement;
            page.TextureKeys.Add(key);
        }

        page.AtlasTexture.Apply(false, false);

        string pageSuffix = BuildBatchAtlasPageSuffix(page.PageIndex, totalPages);
        string atlasBaseName = batchBaseName + "_Atlas_df" + pageSuffix;
        string atlasAssetRelative = AtlasFolderAssetRelative + "/" + atlasBaseName + ".png";
        string materialAssetRelative = MaterialFolderAssetRelative + "/" + atlasBaseName + ".mat";
        string atlasAbs = GetAbsolutePathFromAssetPath(atlasAssetRelative);

        File.WriteAllBytes(atlasAbs, page.AtlasTexture.EncodeToPNG());

        bool pausedEditing = PauseDeferredAssetEditingForImmediateImports();
        try
        {
            AssetDatabase.ImportAsset(atlasAssetRelative, ImportAssetOptions.ForceUpdate);
            ConfigureImportedTextureForPointSampling(atlasAssetRelative);

            Texture2D importedAtlas = AssetDatabase.LoadAssetAtPath<Texture2D>(atlasAssetRelative);
            if (importedAtlas == null)
            {
                error = "Failed loading imported shared atlas page " + page.PageIndex + " at path '" + atlasAssetRelative + "'";
                return false;
            }

            Material mat = new Material(Shader.Find("Standard"));
            mat.name = atlasBaseName;
            mat.mainTexture = importedAtlas;
            ApplyStandardCutoutMaterialSettings(mat);

            AssetDatabase.DeleteAsset(materialAssetRelative);
            AssetDatabase.CreateAsset(mat, materialAssetRelative);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(materialAssetRelative, ImportAssetOptions.ForceUpdate);

            page.AtlasAssetPath = atlasAssetRelative;
            page.MaterialAssetPath = materialAssetRelative;
            page.MaterialAsset = AssetDatabase.LoadAssetAtPath<Material>(materialAssetRelative);

            if (page.MaterialAsset == null)
            {
                error = "Failed loading created shared atlas material page " + page.PageIndex + " at path '" + materialAssetRelative + "'";
                return false;
            }
        }
        finally
        {
            ResumeDeferredAssetEditingAfterImmediateImports(pausedEditing);
        }

        pages.Add(page);
    }

    return true;
}

private static BatchTexturePage CreateBatchTexturePage(int pageIndex, int width, int height)
{
    BatchTexturePage page = new BatchTexturePage();
    page.PageIndex = pageIndex;
    page.FinalWidth = width;
    page.FinalHeight = height;
    page.AtlasTexture = null;
    return page;
}
private bool ExportOneModelSharedAtlas(
    BatchModelPlan plan,
    List<BatchTexturePage> pages,
    Dictionary<string, BatchTexturePlacement> placements,
    out string error)
{
    error = null;

    if (plan == null || plan.Mesh == null)
    {
        error = "Plan or mesh was null.";
        return false;
    }

    List<int> localPageIndices;
    Mesh remappedMesh = BuildSharedBatchAtlasedMesh(plan.Mesh, plan.TextureKeys, placements, out localPageIndices);
    if (remappedMesh == null)
    {
        error = "Failed building shared batch atlased mesh.";
        return false;
    }

    GameObject exportGo = null;
    try
    {
        string exportBaseName = string.IsNullOrEmpty(plan.ExportBaseName)
            ? BuildDefaultExportBaseName(plan.ModelId)
            : plan.ExportBaseName;

        exportGo = new GameObject(exportBaseName);
        MeshFilter mf = exportGo.AddComponent<MeshFilter>();
        MeshRenderer mr = exportGo.AddComponent<MeshRenderer>();

        mf.sharedMesh = remappedMesh;
        exportGo.transform.localScale = Vector3.one * exportScaleMultiplier;

        Material[] mats = new Material[localPageIndices.Count];
        for (int i = 0; i < localPageIndices.Count; i++)
            mats[i] = pages[localPageIndices[i]].MaterialAsset;

        mr.sharedMaterials = mats;

        string categoryLabelForLog = string.IsNullOrWhiteSpace(plan.CategoryFieldName)
            ? "(none)"
            : plan.CategoryFieldName;

        string targetFolderAssetRelative = plan.UseCategorySubfolder && !string.IsNullOrWhiteSpace(plan.CategoryFieldName)
            ? BuildCategoryFbxFolderAssetRelativePath(plan.CategoryFieldName)
            : FbxFolderAssetRelative;

        string exportPathAssetRelative = BuildFbxAssetRelativePath(
            exportBaseName,
            plan.CategoryFieldName,
            plan.UseCategorySubfolder);

        Debug.Log("[NexusDev][DFU Export] Shared-atlas export for category '" + categoryLabelForLog + "'.");
        Debug.Log("[NexusDev][DFU Export] Shared-atlas target directory will be: " + targetFolderAssetRelative);
        Debug.Log("[NexusDev][DFU Export] Shared-atlas target FBX path will be: " + exportPathAssetRelative);

        string exportedAssetPath;
        string exportError;
        bool ok = ExportFbxViaReflection(exportPathAssetRelative, exportGo, out exportedAssetPath, out exportError);

        if (!ok)
        {
            error = exportError;
            return false;
        }

        bool pausedForFbx = PauseDeferredAssetEditingForImmediateImports();
        try
        {
            AssetDatabase.ImportAsset(
                exportedAssetPath,
                ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
        }
        finally
        {
            ResumeDeferredAssetEditingAfterImmediateImports(pausedForFbx);
        }

        string remapError;
        if (!RemapImportedFbxMaterials(exportedAssetPath, mats, out remapError))
        {
            error = "FBX material remap failed. " + remapError;
            return false;
        }

        Debug.Log("[NexusDev][DFU Export] We placed shared-atlas model " + plan.ModelId + " in directory: " + targetFolderAssetRelative);
        Debug.Log("[NexusDev][DFU Export] SUCCESS shared-atlas model " + plan.ModelId + " -> " + exportedAssetPath);
        return true;
    }
    finally
    {
        if (exportGo != null)
            DestroyImmediate(exportGo);

        if (remappedMesh != null)
            DestroyImmediate(remappedMesh);
    }
}
private bool ValidateBeforeExport()
{
    if (!IsDaggerfallUnityReady())
    {
        Debug.LogError("[NexusDev][DFU Export] DaggerfallUnity is not ready.");
        return false;
    }

    string arena2Path = global::DaggerfallWorkshop.DaggerfallUnity.Instance.Arena2Path;
    if (string.IsNullOrWhiteSpace(arena2Path) || !Directory.Exists(arena2Path))
    {
        Debug.LogError("[NexusDev][DFU Export] DaggerfallUnity Arena2Path is invalid: " + arena2Path);
        return false;
    }

    return true;
}
private static bool IsDaggerfallUnityReady()
{
    if (global::DaggerfallWorkshop.DaggerfallUnity.Instance == null)
        return false;

    return global::DaggerfallWorkshop.DaggerfallUnity.Instance.IsReady;
}


private static bool HasFbxExporter(out string error)
{
    if (cachedExporterType == null)
    {
        cachedExporterType = Type.GetType(
            "UnityEditor.Formats.Fbx.Exporter.ModelExporter, Unity.Formats.Fbx.Editor");
    }

    if (cachedIExportOptionsType == null)
    {
        cachedIExportOptionsType = Type.GetType(
            "UnityEditor.Formats.Fbx.Exporter.IExportOptions, Unity.Formats.Fbx.Editor");
    }

    if (cachedExportModelSettingsSerializeType == null)
    {
        cachedExportModelSettingsSerializeType = Type.GetType(
            "UnityEditor.Formats.Fbx.Exporter.ExportModelSettingsSerialize, Unity.Formats.Fbx.Editor");
    }

    if (cachedExportSettingsType == null)
    {
        cachedExportSettingsType = Type.GetType(
            "UnityEditor.Formats.Fbx.Exporter.ExportSettings, Unity.Formats.Fbx.Editor");
    }

    if (cachedExporterType != null && cachedIExportOptionsType != null && cachedExportMethod == null)
    {
        cachedExportMethod = cachedExporterType.GetMethod(
            "ExportObject",
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
            null,
            new Type[] { typeof(string), typeof(UnityEngine.Object), cachedIExportOptionsType },
            null);
    }

    if (cachedExportSettingsType != null && cachedExportFormatProperty == null)
    {
        cachedExportFormatProperty = cachedExportSettingsType.GetProperty(
            "ExportFormat",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    }

    if (cachedExportModelSettingsSerializeType != null && cachedSetExportFormatMethod == null && cachedExportSettingsType != null)
    {
        Type exportFormatEnumType = cachedExportSettingsType.GetNestedType(
            "ExportFormat",
            BindingFlags.Public | BindingFlags.NonPublic);

        if (exportFormatEnumType != null)
        {
            cachedSetExportFormatMethod = cachedExportModelSettingsSerializeType.GetMethod(
                "SetExportFormat",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[] { exportFormatEnumType },
                null);

            cachedBinaryExportFormatValue = Enum.ToObject(exportFormatEnumType, 1);
        }
    }

    if (cachedExporterType == null)
    {
        error = "Unity FBX Exporter package is not installed or not available to this project.";
        return false;
    }

    if (cachedIExportOptionsType == null)
    {
        error = "Could not find UnityEditor.Formats.Fbx.Exporter.IExportOptions.";
        return false;
    }

    if (cachedExportModelSettingsSerializeType == null)
    {
        error = "Could not find ExportModelSettingsSerialize in the FBX Exporter package.";
        return false;
    }

    if (cachedExportMethod == null)
    {
        error = "Could not find ModelExporter.ExportObject(string, Object, IExportOptions).";
        return false;
    }

    if (cachedSetExportFormatMethod == null || cachedBinaryExportFormatValue == null)
    {
        error = "Could not find ExportModelSettingsSerialize.SetExportFormat(Binary).";
        return false;
    }

    error = null;
    return true;
}
private bool ExportOneModel(uint modelId, string exportBaseName = null)
{
    return ExportOneModel(modelId, exportBaseName, null, false);
}

private bool ExportOneModel(uint modelId, string exportBaseName, string categoryFieldNameOrNull, bool useCategorySubfolder)
{
    GameObject sourceGo = null;
    GameObject exportGo = null;
    string tempModelTextureFolderAssetRelative = null;

    exportBaseName = string.IsNullOrEmpty(exportBaseName)
        ? BuildDefaultExportBaseName(modelId)
        : exportBaseName;

    try
    {
        if (ShouldSkipSingleExportBecauseAssetsExist(exportBaseName))
        {
            Debug.LogWarning("[NexusDev][DFU Export] Skipping '" + exportBaseName + "' because overwrite is off and one or more matching assets already exist.");
            return true;
        }

        sourceGo = global::DaggerfallWorkshop.Utility.GameObjectHelper.CreateDaggerfallMeshGameObject(modelId, null);

        if (sourceGo == null)
        {
            Debug.LogError("[NexusDev][DFU Export] Source object is null for model " + modelId);
            return false;
        }

        sourceGo.name = exportBaseName + "_SOURCE";

        MeshFilter sourceMf = sourceGo.GetComponent<MeshFilter>();
        MeshRenderer sourceMr = sourceGo.GetComponent<MeshRenderer>();

        if (sourceMf == null || sourceMf.sharedMesh == null)
        {
            Debug.LogError("[NexusDev][DFU Export] Model " + modelId + " has no mesh.");
            return false;
        }

        if (sourceMr == null || sourceMr.sharedMaterials == null || sourceMr.sharedMaterials.Length == 0)
        {
            Debug.LogError("[NexusDev][DFU Export] Model " + modelId + " has no materials.");
            return false;
        }

        string bakeError;
        bool baked = TryCreateAtlasedExportObject(
            modelId,
            exportBaseName,
            sourceGo,
            out exportGo,
            out tempModelTextureFolderAssetRelative,
            out bakeError);

        if (!baked)
        {
            Debug.LogError("[NexusDev][DFU Export] Bake failed for model " + modelId + ". " + bakeError);
            return false;
        }

                     string categoryLabelForLog = string.IsNullOrWhiteSpace(categoryFieldNameOrNull)
            ? "(none)"
            : categoryFieldNameOrNull;

        string targetFolderAssetRelative = useCategorySubfolder && !string.IsNullOrWhiteSpace(categoryFieldNameOrNull)
            ? BuildCategoryFbxFolderAssetRelativePath(categoryFieldNameOrNull)
            : FbxFolderAssetRelative;

        string exportPathAssetRelative = BuildFbxAssetRelativePath(
            exportBaseName,
            categoryFieldNameOrNull,
            useCategorySubfolder);

        Debug.Log("[NexusDev][DFU Export] Export for category '" + categoryLabelForLog + "'.");
        Debug.Log("[NexusDev][DFU Export] Target directory will be: " + targetFolderAssetRelative);
        Debug.Log("[NexusDev][DFU Export] Target FBX path will be: " + exportPathAssetRelative);

        string exportedAssetPath;
        string exportError;
        bool ok = ExportFbxViaReflection(exportPathAssetRelative, exportGo, out exportedAssetPath, out exportError);

        if (!ok)
        {
            Debug.LogError("[NexusDev][DFU Export] FAILED model " + modelId + ". " + exportError);
            return false;
        }

                Debug.Log("[NexusDev][DFU Export] We placed model " + modelId + " in directory: " + targetFolderAssetRelative);
        Debug.Log("[NexusDev][DFU Export] SUCCESS model " + modelId + " -> " + exportedAssetPath);
        return true;
    }
    catch (Exception ex)
    {
        Debug.LogError("[NexusDev][DFU Export] FAILED model " + modelId + ". Exception: " + ex);
        return false;
    }
    finally
    {
        if (exportGo != null)
            DestroyImmediate(exportGo);

        if (sourceGo != null)
            DestroyImmediate(sourceGo);

                if (deleteTempTextureFilesAfterExport && !string.IsNullOrEmpty(tempModelTextureFolderAssetRelative))
            DeleteAssetFolder(tempModelTextureFolderAssetRelative);

        if (ShouldPerformImmediateAssetRefresh())
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}

private bool ExportOneFlat(int archive, int record, string exportBaseName, string flatGroupNameOrNull, bool useFlatGroupSubfolder)
{
    GameObject sourceGo = null;
    GameObject exportGo = null;
    string tempFlatTextureAssetRelative = null;

    exportBaseName = string.IsNullOrEmpty(exportBaseName)
        ? BuildDefaultExportBaseNameForFlat(archive, record)
        : exportBaseName;

    try
    {
        string exportPathAssetRelative = useFlatGroupSubfolder && !string.IsNullOrWhiteSpace(flatGroupNameOrNull)
            ? BuildFlatCategoryFbxFolderAssetRelativePath(flatGroupNameOrNull) + "/" + SanitizeExportToken(exportBaseName) + ".fbx"
            : BuildFlatFbxAssetRelativePath(exportBaseName);

        if (!overwriteExistingExportAssets && AssetExistsAtAssetPath(exportPathAssetRelative))
        {
            Debug.LogWarning("[NexusDev][DFU Export] Skipping flat '" + exportBaseName + "' because overwrite is off and it already exists.");
            return true;
        }

        sourceGo = new GameObject(exportBaseName + "_SOURCE");
        MeshFilter mf = sourceGo.AddComponent<MeshFilter>();
        MeshRenderer mr = sourceGo.AddComponent<MeshRenderer>();
        global::DaggerfallWorkshop.DaggerfallBillboard billboard = sourceGo.AddComponent<global::DaggerfallWorkshop.DaggerfallBillboard>();

        Material runtimeMaterial;
        try
        {
            runtimeMaterial = billboard.SetMaterial(archive, record, 0);
            billboard.AlignToBase();
        }
        catch (Exception ex)
        {
            Debug.LogError(
                "[NexusDev][DFU Export] Flat " +
                archive.ToString(CultureInfo.InvariantCulture) + "." +
                record.ToString(CultureInfo.InvariantCulture) +
                " failed while building billboard mesh/material. Exception: " + ex);
            return false;
        }

        if (mf.sharedMesh == null)
        {
            Debug.LogError("[NexusDev][DFU Export] Flat " + archive + "." + record + " did not generate a mesh.");
            return false;
        }

        if (runtimeMaterial == null)
        {
            Debug.LogError("[NexusDev][DFU Export] Flat " + archive + "." + record + " did not generate a runtime material.");
            return false;
        }

        Texture2D readableTexture;
        string textureError;
        if (!TryLoadDfuTexture(MakeTextureKey(archive, record), out readableTexture, out textureError))
        {
            Debug.LogError("[NexusDev][DFU Export] Could not load direct flat texture for " + archive + "." + record + ". " + textureError);
            return false;
        }

        try
        {
            tempFlatTextureAssetRelative = BuildSingleFlatTextureAssetRelativePath(exportBaseName);
            string textureAbs = GetAbsolutePathFromAssetPath(tempFlatTextureAssetRelative);
            File.WriteAllBytes(textureAbs, readableTexture.EncodeToPNG());

            AssetDatabase.ImportAsset(tempFlatTextureAssetRelative, ImportAssetOptions.ForceUpdate);
            ConfigureImportedTextureForPointSampling(tempFlatTextureAssetRelative);

            Texture2D importedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(tempFlatTextureAssetRelative);
            if (importedTexture == null)
            {
                Debug.LogError("[NexusDev][DFU Export] Imported flat texture could not be loaded: " + tempFlatTextureAssetRelative);
                return false;
            }

            string materialAssetRelative = BuildSingleFlatMaterialAssetRelativePath(exportBaseName);
            Material flatMaterial = new Material(Shader.Find("Standard"));
            flatMaterial.name = exportBaseName + "_Flat";
            flatMaterial.mainTexture = importedTexture;
            ApplyStandardCutoutMaterialSettings(flatMaterial);

            AssetDatabase.DeleteAsset(materialAssetRelative);
            AssetDatabase.CreateAsset(flatMaterial, materialAssetRelative);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(materialAssetRelative, ImportAssetOptions.ForceUpdate);

            Material savedFlatMaterial = AssetDatabase.LoadAssetAtPath<Material>(materialAssetRelative);
            if (savedFlatMaterial == null)
            {
                Debug.LogError("[NexusDev][DFU Export] Saved flat material could not be loaded: " + materialAssetRelative);
                return false;
            }

            exportGo = new GameObject(exportBaseName);
            MeshFilter outMf = exportGo.AddComponent<MeshFilter>();
            MeshRenderer outMr = exportGo.AddComponent<MeshRenderer>();

            Mesh exportMesh = Instantiate(mf.sharedMesh);
            exportMesh.name = exportBaseName + "_Mesh";

            RemapMeshUvInPlace(exportMesh, Vector2.one, Vector2.zero);

            outMf.sharedMesh = exportMesh;
            outMr.sharedMaterials = new Material[] { savedFlatMaterial };
            exportGo.transform.position = sourceGo.transform.position;
            exportGo.transform.rotation = sourceGo.transform.rotation;
            exportGo.transform.localScale = Vector3.one * exportScaleMultiplier;

            string targetFolderAssetRelative = useFlatGroupSubfolder && !string.IsNullOrWhiteSpace(flatGroupNameOrNull)
                ? BuildFlatCategoryFbxFolderAssetRelativePath(flatGroupNameOrNull)
                : FbxFlatFolderAssetRelative;

            if (useFlatGroupSubfolder && !string.IsNullOrWhiteSpace(flatGroupNameOrNull))
                EnsureFolderExists(targetFolderAssetRelative);

            Debug.Log("[NexusDev][DFU Export] Flat target directory will be: " + targetFolderAssetRelative);
            Debug.Log("[NexusDev][DFU Export] Flat target FBX path will be: " + exportPathAssetRelative);

            string exportedAssetPath;
            string exportError;
            bool ok = ExportFbxViaReflection(exportPathAssetRelative, exportGo, out exportedAssetPath, out exportError);

            if (!ok)
            {
                Debug.LogError("[NexusDev][DFU Export] FAILED flat " + archive + "." + record + ". " + exportError);
                return false;
            }

            string remapError;
            if (!RemapImportedFbxMaterials(exportedAssetPath, new Material[] { savedFlatMaterial }, out remapError))
            {
                Debug.LogError("[NexusDev][DFU Export] Flat FBX material remap failed for " + archive + "." + record + ". " + remapError);
                return false;
            }

            Debug.Log("[NexusDev][DFU Export] SUCCESS flat " + archive + "." + record + " -> " + exportedAssetPath);
            return true;
        }
        finally
        {
            DestroyImmediate(readableTexture);
        }
    }
    catch (Exception ex)
    {
        Debug.LogError("[NexusDev][DFU Export] FAILED flat " + archive + "." + record + ". Exception: " + ex);
        return false;
    }
    finally
    {
        if (exportGo != null)
            DestroyImmediate(exportGo);

        if (sourceGo != null)
            DestroyImmediate(sourceGo);

                if (deleteTempTextureFilesAfterExport && !string.IsNullOrEmpty(tempFlatTextureAssetRelative))
            AssetDatabase.DeleteAsset(tempFlatTextureAssetRelative);

        if (ShouldPerformImmediateAssetRefresh())
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
    private bool TryCreateAtlasedExportObject(
    uint modelId,
    string exportBaseName,
    GameObject sourceGo,
    out GameObject exportGo,
    out string tempModelTextureFolderAssetRelative,
    out string error)
{
    exportGo = null;
    tempModelTextureFolderAssetRelative = null;
    error = null;

    MeshFilter sourceMf = sourceGo.GetComponent<MeshFilter>();
    MeshRenderer sourceMr = sourceGo.GetComponent<MeshRenderer>();
    Mesh sourceMesh = sourceMf.sharedMesh;
    Material[] sourceMaterials = sourceMr.sharedMaterials;

    if (sourceMaterials == null || sourceMaterials.Length == 0)
    {
        error = "Source renderer had zero materials.";
        return false;
    }

    EnsureFolderExists(AtlasFolderAssetRelative);
    EnsureFolderExists(MaterialFolderAssetRelative);

    List<Texture2D> atlasSourceTextures = new List<Texture2D>();
    List<Rect> packedRects = null;

    try
    {
        string[] textureKeys;
        if (!TryGetTextureKeysForModel(modelId, out textureKeys, out error))
            return false;

        for (int i = 0; i < textureKeys.Length; i++)
        {
            Texture2D tex;
            if (!TryLoadDfuTexture(textureKeys[i], out tex, out error))
            {
                error = "Failed loading DFU texture for key '" + textureKeys[i] + "'. " + error;
                return false;
            }

            atlasSourceTextures.Add(tex);
        }

        if (atlasSourceTextures.Count == 0)
        {
            error = "No textures gathered for atlas.";
            return false;
        }

        Texture2D atlasTex = BuildAtlas(atlasSourceTextures, out packedRects);
        if (atlasTex == null || packedRects == null || packedRects.Count != atlasSourceTextures.Count)
        {
            error = "Atlas creation failed.";
            return false;
        }

        try
        {
            string atlasAssetRelative = BuildSingleAtlasAssetRelativePath(exportBaseName);
            string atlasAbs = GetAbsolutePathFromAssetPath(atlasAssetRelative);
            File.WriteAllBytes(atlasAbs, atlasTex.EncodeToPNG());

            AssetDatabase.ImportAsset(atlasAssetRelative, ImportAssetOptions.ForceUpdate);
            ConfigureImportedTextureForPointSampling(atlasAssetRelative);

            Texture2D importedAtlas = AssetDatabase.LoadAssetAtPath<Texture2D>(atlasAssetRelative);
            if (importedAtlas == null)
            {
                error = "Imported atlas could not be loaded: " + atlasAssetRelative;
                return false;
            }

            string materialAssetRelative = BuildSingleMaterialAssetRelativePath(exportBaseName);
            Material atlasMaterial = new Material(Shader.Find("Standard"));
            atlasMaterial.name = exportBaseName;
            atlasMaterial.mainTexture = importedAtlas;

            if (atlasMaterial.HasProperty("_Glossiness"))
                atlasMaterial.SetFloat("_Glossiness", 0f);

            if (atlasMaterial.HasProperty("_Mode"))
                atlasMaterial.SetFloat("_Mode", 0f);

            AssetDatabase.DeleteAsset(materialAssetRelative);
            AssetDatabase.CreateAsset(atlasMaterial, materialAssetRelative);
            AssetDatabase.SaveAssets();

            Mesh atlasedMesh = BuildSingleMaterialAtlasedMesh(sourceMesh, packedRects);
            if (atlasedMesh == null)
            {
                error = "Failed building atlased mesh.";
                return false;
            }

            exportGo = new GameObject(exportBaseName);
            MeshFilter outMf = exportGo.AddComponent<MeshFilter>();
            MeshRenderer outMr = exportGo.AddComponent<MeshRenderer>();

            outMf.sharedMesh = atlasedMesh;
            outMr.sharedMaterials = new Material[] { atlasMaterial };
            exportGo.transform.localScale = Vector3.one * exportScaleMultiplier;

            return true;
        }
        finally
        {
            DestroyImmediate(atlasTex);
        }
    }
    finally
    {
        for (int i = 0; i < atlasSourceTextures.Count; i++)
        {
            if (atlasSourceTextures[i] != null)
                DestroyImmediate(atlasSourceTextures[i]);
        }

        atlasSourceTextures.Clear();
    }
}

        private static Texture2D BuildAtlas(List<Texture2D> textures, out List<Rect> rectsOut)
        {
            rectsOut = new List<Rect>();

            Texture2D atlas = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            atlas.filterMode = FilterMode.Point;
            atlas.wrapMode = TextureWrapMode.Clamp;

            Texture2D[] texArray = textures.ToArray();
            Rect[] rects = atlas.PackTextures(texArray, 0, 8192, false);

            for (int i = 0; i < rects.Length; i++)
                rectsOut.Add(rects[i]);

            return atlas;
        }
        private static Mesh BuildSingleMaterialAtlasedMesh(Mesh sourceMesh, List<Rect> packedRects)
{
    if (sourceMesh == null)
        return null;

    int usableSubmeshCount = Mathf.Min(sourceMesh.subMeshCount, packedRects.Count);
    if (usableSubmeshCount <= 0)
        return null;

    Vector3[] srcVertices = sourceMesh.vertices;
    Vector3[] srcNormals = sourceMesh.normals;
    Vector4[] srcTangents = sourceMesh.tangents;
    Vector2[] srcUv = sourceMesh.uv;
    Color[] srcColors = sourceMesh.colors;

    bool hasNormals = srcNormals != null && srcNormals.Length == srcVertices.Length;
    bool hasTangents = srcTangents != null && srcTangents.Length == srcVertices.Length;
    bool hasUv = srcUv != null && srcUv.Length == srcVertices.Length;
    bool hasColors = srcColors != null && srcColors.Length == srcVertices.Length;

    List<Vector3> dstVertices = new List<Vector3>();
    List<Vector3> dstNormals = new List<Vector3>();
    List<Vector4> dstTangents = new List<Vector4>();
    List<Vector2> dstUv = new List<Vector2>();
    List<Color> dstColors = new List<Color>();
    List<int> dstTriangles = new List<int>();

    Dictionary<VertexKey, int> vertexMap = new Dictionary<VertexKey, int>();

    for (int submeshIndex = 0; submeshIndex < usableSubmeshCount; submeshIndex++)
    {
        int[] tris = sourceMesh.GetTriangles(submeshIndex);
        Rect rect = packedRects[submeshIndex];

        for (int triIndex = 0; triIndex + 2 < tris.Length; triIndex += 3)
        {
            int i0 = tris[triIndex + 0];
            int i1 = tris[triIndex + 1];
            int i2 = tris[triIndex + 2];

            Vector2 uv0 = hasUv ? srcUv[i0] : Vector2.zero;
            Vector2 uv1 = hasUv ? srcUv[i1] : Vector2.zero;
            Vector2 uv2 = hasUv ? srcUv[i2] : Vector2.zero;

            float minU = Mathf.Min(uv0.x, Mathf.Min(uv1.x, uv2.x));
            float maxU = Mathf.Max(uv0.x, Mathf.Max(uv1.x, uv2.x));
            float minV = Mathf.Min(uv0.y, Mathf.Min(uv1.y, uv2.y));
            float maxV = Mathf.Max(uv0.y, Mathf.Max(uv1.y, uv2.y));

            int minUCell = Mathf.FloorToInt(minU);
            int maxUCell = Mathf.FloorToInt(maxU);
            int minVCell = Mathf.FloorToInt(minV);
            int maxVCell = Mathf.FloorToInt(maxV);

            bool needsSubdivision = hasUv && (minUCell != maxUCell || minVCell != maxVCell);

            if (!needsSubdivision)
            {
                int d0 = GetOrCreateNonSubdividedVertex(
                    i0, submeshIndex, rect,
                    srcVertices, srcNormals, srcTangents, srcUv, srcColors,
                    hasNormals, hasTangents, hasUv, hasColors,
                    vertexMap,
                    dstVertices, dstNormals, dstTangents, dstUv, dstColors);

                int d1 = GetOrCreateNonSubdividedVertex(
                    i1, submeshIndex, rect,
                    srcVertices, srcNormals, srcTangents, srcUv, srcColors,
                    hasNormals, hasTangents, hasUv, hasColors,
                    vertexMap,
                    dstVertices, dstNormals, dstTangents, dstUv, dstColors);

                int d2 = GetOrCreateNonSubdividedVertex(
                    i2, submeshIndex, rect,
                    srcVertices, srcNormals, srcTangents, srcUv, srcColors,
                    hasNormals, hasTangents, hasUv, hasColors,
                    vertexMap,
                    dstVertices, dstNormals, dstTangents, dstUv, dstColors);

                dstTriangles.Add(d0);
                dstTriangles.Add(d1);
                dstTriangles.Add(d2);
                continue;
            }

            AtlasedClipVertex a = CreateClipVertex(
                i0, srcVertices, srcNormals, srcTangents, srcUv, srcColors,
                hasNormals, hasTangents, hasUv, hasColors);

            AtlasedClipVertex b = CreateClipVertex(
                i1, srcVertices, srcNormals, srcTangents, srcUv, srcColors,
                hasNormals, hasTangents, hasUv, hasColors);

            AtlasedClipVertex c = CreateClipVertex(
                i2, srcVertices, srcNormals, srcTangents, srcUv, srcColors,
                hasNormals, hasTangents, hasUv, hasColors);

            for (int uCell = minUCell; uCell <= maxUCell; uCell++)
            {
                for (int vCell = minVCell; vCell <= maxVCell; vCell++)
                {
                    List<AtlasedClipVertex> poly = ClipTriangleToCell(
                        a, b, c,
                        uCell, vCell,
                        hasNormals, hasTangents, hasColors);

                    if (poly == null || poly.Count < 3)
                        continue;

                    AddClippedPolygonToMesh(
                        poly, uCell, vCell, rect,
                        hasNormals, hasTangents, hasColors,
                        dstVertices, dstNormals, dstTangents, dstUv, dstColors, dstTriangles);
                }
            }
        }
    }

    Mesh outMesh = new Mesh();
    outMesh.name = sourceMesh.name + "_Atlased";

    if (dstVertices.Count > 65535)
        outMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

    outMesh.SetVertices(dstVertices);
    outMesh.SetTriangles(dstTriangles, 0);

    if (hasNormals) outMesh.SetNormals(dstNormals);
    else outMesh.RecalculateNormals();

    if (hasTangents && dstTangents.Count == dstVertices.Count)
        outMesh.SetTangents(dstTangents);

    if (dstUv.Count == dstVertices.Count)
        outMesh.SetUVs(0, dstUv);

    if (hasColors && dstColors.Count == dstVertices.Count)
        outMesh.SetColors(dstColors);

    outMesh.RecalculateBounds();
    return outMesh;
}

private static Mesh BuildSharedBatchAtlasedMesh(
    Mesh sourceMesh,
    string[] textureKeys,
    Dictionary<string, BatchTexturePlacement> placements,
    out List<int> localPageIndices)
{
    localPageIndices = new List<int>();

    if (sourceMesh == null || textureKeys == null || placements == null)
        return null;

    Vector3[] srcVertices = sourceMesh.vertices;
    Vector3[] srcNormals = sourceMesh.normals;
    Vector4[] srcTangents = sourceMesh.tangents;
    Vector2[] srcUv = sourceMesh.uv;
    Color[] srcColors = sourceMesh.colors;

    bool hasNormals = srcNormals != null && srcNormals.Length == srcVertices.Length;
    bool hasTangents = srcTangents != null && srcTangents.Length == srcVertices.Length;
    bool hasUv = srcUv != null && srcUv.Length == srcVertices.Length;
    bool hasColors = srcColors != null && srcColors.Length == srcVertices.Length;

    List<Vector3> dstVertices = new List<Vector3>();
    List<Vector3> dstNormals = new List<Vector3>();
    List<Vector4> dstTangents = new List<Vector4>();
    List<Vector2> dstUv = new List<Vector2>();
    List<Color> dstColors = new List<Color>();

    Dictionary<int, int> pageToLocalSlot = new Dictionary<int, int>();
    List<List<int>> trianglesByLocalSlot = new List<List<int>>();
    Dictionary<BatchVertexKey, int> vertexMap = new Dictionary<BatchVertexKey, int>();

    int usableSubmeshCount = Mathf.Min(sourceMesh.subMeshCount, textureKeys.Length);

    for (int submeshIndex = 0; submeshIndex < usableSubmeshCount; submeshIndex++)
    {
        string textureKey = textureKeys[submeshIndex];
        BatchTexturePlacement placement;
        if (string.IsNullOrEmpty(textureKey) || !placements.TryGetValue(textureKey, out placement))
            continue;

        int localSlot;
        if (!pageToLocalSlot.TryGetValue(placement.PageIndex, out localSlot))
        {
            localSlot = trianglesByLocalSlot.Count;
            pageToLocalSlot.Add(placement.PageIndex, localSlot);
            localPageIndices.Add(placement.PageIndex);
            trianglesByLocalSlot.Add(new List<int>());
        }

        int[] tris = sourceMesh.GetTriangles(submeshIndex);

        for (int triIndex = 0; triIndex + 2 < tris.Length; triIndex += 3)
        {
            int i0 = tris[triIndex + 0];
            int i1 = tris[triIndex + 1];
            int i2 = tris[triIndex + 2];

            Vector2 uv0 = hasUv ? srcUv[i0] : Vector2.zero;
            Vector2 uv1 = hasUv ? srcUv[i1] : Vector2.zero;
            Vector2 uv2 = hasUv ? srcUv[i2] : Vector2.zero;

            float minU = Mathf.Min(uv0.x, Mathf.Min(uv1.x, uv2.x));
            float maxU = Mathf.Max(uv0.x, Mathf.Max(uv1.x, uv2.x));
            float minV = Mathf.Min(uv0.y, Mathf.Min(uv1.y, uv2.y));
            float maxV = Mathf.Max(uv0.y, Mathf.Max(uv1.y, uv2.y));

            int minUCell = Mathf.FloorToInt(minU);
            int maxUCell = Mathf.FloorToInt(maxU);
            int minVCell = Mathf.FloorToInt(minV);
            int maxVCell = Mathf.FloorToInt(maxV);

            bool needsSubdivision = hasUv && (minUCell != maxUCell || minVCell != maxVCell);

            if (!needsSubdivision)
            {
                int d0 = GetOrCreateSharedBatchVertex(
                    i0, submeshIndex, localSlot, placement.Rect,
                    srcVertices, srcNormals, srcTangents, srcUv, srcColors,
                    hasNormals, hasTangents, hasUv, hasColors,
                    vertexMap,
                    dstVertices, dstNormals, dstTangents, dstUv, dstColors);

                int d1 = GetOrCreateSharedBatchVertex(
                    i1, submeshIndex, localSlot, placement.Rect,
                    srcVertices, srcNormals, srcTangents, srcUv, srcColors,
                    hasNormals, hasTangents, hasUv, hasColors,
                    vertexMap,
                    dstVertices, dstNormals, dstTangents, dstUv, dstColors);

                int d2 = GetOrCreateSharedBatchVertex(
                    i2, submeshIndex, localSlot, placement.Rect,
                    srcVertices, srcNormals, srcTangents, srcUv, srcColors,
                    hasNormals, hasTangents, hasUv, hasColors,
                    vertexMap,
                    dstVertices, dstNormals, dstTangents, dstUv, dstColors);

                trianglesByLocalSlot[localSlot].Add(d0);
                trianglesByLocalSlot[localSlot].Add(d1);
                trianglesByLocalSlot[localSlot].Add(d2);
                continue;
            }

            AtlasedClipVertex a = CreateClipVertex(i0, srcVertices, srcNormals, srcTangents, srcUv, srcColors, hasNormals, hasTangents, hasUv, hasColors);
            AtlasedClipVertex b = CreateClipVertex(i1, srcVertices, srcNormals, srcTangents, srcUv, srcColors, hasNormals, hasTangents, hasUv, hasColors);
            AtlasedClipVertex c = CreateClipVertex(i2, srcVertices, srcNormals, srcTangents, srcUv, srcColors, hasNormals, hasTangents, hasUv, hasColors);

            for (int uCell = minUCell; uCell <= maxUCell; uCell++)
            {
                for (int vCell = minVCell; vCell <= maxVCell; vCell++)
                {
                    List<AtlasedClipVertex> poly = ClipTriangleToCell(a, b, c, uCell, vCell, hasNormals, hasTangents, hasColors);
                    if (poly == null || poly.Count < 3)
                        continue;

                    AddClippedPolygonToMesh(
                        poly, uCell, vCell, placement.Rect,
                        hasNormals, hasTangents, hasColors,
                        dstVertices, dstNormals, dstTangents, dstUv, dstColors,
                        trianglesByLocalSlot[localSlot]);
                }
            }
        }
    }

    Mesh outMesh = new Mesh();
    outMesh.name = sourceMesh.name + "_SharedBatchAtlased";

    if (dstVertices.Count > 65535)
        outMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

    outMesh.SetVertices(dstVertices);
    outMesh.subMeshCount = trianglesByLocalSlot.Count;

    for (int i = 0; i < trianglesByLocalSlot.Count; i++)
        outMesh.SetTriangles(trianglesByLocalSlot[i], i);

    if (hasNormals) outMesh.SetNormals(dstNormals);
    else outMesh.RecalculateNormals();

    if (hasTangents && dstTangents.Count == dstVertices.Count)
        outMesh.SetTangents(dstTangents);

    if (dstUv.Count == dstVertices.Count)
        outMesh.SetUVs(0, dstUv);

    if (hasColors && dstColors.Count == dstVertices.Count)
        outMesh.SetColors(dstColors);

    outMesh.RecalculateBounds();
    return outMesh;
}

private static int GetOrCreateSharedBatchVertex(
    int srcIndex,
    int submeshIndex,
    int localMaterialSlot,
    Rect rect,
    Vector3[] srcVertices,
    Vector3[] srcNormals,
    Vector4[] srcTangents,
    Vector2[] srcUv,
    Color[] srcColors,
    bool hasNormals,
    bool hasTangents,
    bool hasUv,
    bool hasColors,
    Dictionary<BatchVertexKey, int> vertexMap,
    List<Vector3> dstVertices,
    List<Vector3> dstNormals,
    List<Vector4> dstTangents,
    List<Vector2> dstUv,
    List<Color> dstColors)
{
    BatchVertexKey key = new BatchVertexKey(srcIndex, submeshIndex, localMaterialSlot);

    int dstIndex;
    if (vertexMap.TryGetValue(key, out dstIndex))
        return dstIndex;

    dstIndex = dstVertices.Count;
    vertexMap.Add(key, dstIndex);

    dstVertices.Add(srcVertices[srcIndex]);

    if (hasNormals) dstNormals.Add(srcNormals[srcIndex]);
    if (hasTangents) dstTangents.Add(srcTangents[srcIndex]);
    if (hasColors) dstColors.Add(srcColors[srcIndex]);

    if (hasUv)
    {
        float localU = Mathf.Repeat(srcUv[srcIndex].x, 1.0f);
        float localV = Mathf.Repeat(srcUv[srcIndex].y, 1.0f);

        dstUv.Add(new Vector2(
            rect.x + (localU * rect.width),
            rect.y + (localV * rect.height)));
    }
    else
    {
        dstUv.Add(Vector2.zero);
    }

    return dstIndex;
}

private struct AtlasedClipVertex
{
    public Vector3 Position;
    public Vector3 Normal;
    public Vector4 Tangent;
    public Vector2 UV;
    public Color Color;
}

private static int GetOrCreateNonSubdividedVertex(
    int srcIndex,
    int submeshIndex,
    Rect rect,
    Vector3[] srcVertices,
    Vector3[] srcNormals,
    Vector4[] srcTangents,
    Vector2[] srcUv,
    Color[] srcColors,
    bool hasNormals,
    bool hasTangents,
    bool hasUv,
    bool hasColors,
    Dictionary<VertexKey, int> vertexMap,
    List<Vector3> dstVertices,
    List<Vector3> dstNormals,
    List<Vector4> dstTangents,
    List<Vector2> dstUv,
    List<Color> dstColors)
{
    VertexKey key = new VertexKey(srcIndex, submeshIndex);

    int dstIndex;
    if (vertexMap.TryGetValue(key, out dstIndex))
        return dstIndex;

    dstIndex = dstVertices.Count;
    vertexMap.Add(key, dstIndex);

    dstVertices.Add(srcVertices[srcIndex]);

    if (hasNormals)
        dstNormals.Add(srcNormals[srcIndex]);

    if (hasTangents)
        dstTangents.Add(srcTangents[srcIndex]);

    if (hasColors)
        dstColors.Add(srcColors[srcIndex]);

    if (hasUv)
    {
        float localU = Mathf.Repeat(srcUv[srcIndex].x, 1.0f);
        float localV = Mathf.Repeat(srcUv[srcIndex].y, 1.0f);

        dstUv.Add(new Vector2(
            rect.x + (localU * rect.width),
            rect.y + (localV * rect.height)));
    }
    else
    {
        dstUv.Add(Vector2.zero);
    }

    return dstIndex;
}

private static AtlasedClipVertex CreateClipVertex(
    int srcIndex,
    Vector3[] srcVertices,
    Vector3[] srcNormals,
    Vector4[] srcTangents,
    Vector2[] srcUv,
    Color[] srcColors,
    bool hasNormals,
    bool hasTangents,
    bool hasUv,
    bool hasColors)
{
    AtlasedClipVertex v = new AtlasedClipVertex();
    v.Position = srcVertices[srcIndex];
    v.Normal = hasNormals ? srcNormals[srcIndex] : Vector3.up;
    v.Tangent = hasTangents ? srcTangents[srcIndex] : new Vector4(1f, 0f, 0f, 1f);
    v.UV = hasUv ? srcUv[srcIndex] : Vector2.zero;
    v.Color = hasColors ? srcColors[srcIndex] : Color.white;
    return v;
}

private static AtlasedClipVertex LerpClipVertex(
    AtlasedClipVertex a,
    AtlasedClipVertex b,
    float t,
    bool hasNormals,
    bool hasTangents,
    bool hasColors)
{
    AtlasedClipVertex v = new AtlasedClipVertex();
    v.Position = Vector3.Lerp(a.Position, b.Position, t);
    v.UV = Vector2.Lerp(a.UV, b.UV, t);

    if (hasNormals)
        v.Normal = Vector3.Normalize(Vector3.Lerp(a.Normal, b.Normal, t));
    else
        v.Normal = Vector3.up;

    if (hasTangents)
    {
        Vector4 tan = Vector4.Lerp(a.Tangent, b.Tangent, t);
        Vector3 tan3 = new Vector3(tan.x, tan.y, tan.z).normalized;
        v.Tangent = new Vector4(tan3.x, tan3.y, tan3.z, tan.w);
    }
    else
    {
        v.Tangent = new Vector4(1f, 0f, 0f, 1f);
    }

    v.Color = hasColors ? Color.Lerp(a.Color, b.Color, t) : Color.white;
    return v;
}

private static bool IsInsideBoundary(AtlasedClipVertex v, int axis, float boundary, bool keepGreater)
{
    float value = (axis == 0) ? v.UV.x : v.UV.y;
    return keepGreater ? value >= boundary : value <= boundary;
}

private static AtlasedClipVertex IntersectBoundary(
    AtlasedClipVertex a,
    AtlasedClipVertex b,
    int axis,
    float boundary,
    bool hasNormals,
    bool hasTangents,
    bool hasColors)
{
    float av = (axis == 0) ? a.UV.x : a.UV.y;
    float bv = (axis == 0) ? b.UV.x : b.UV.y;
    float denom = bv - av;

    float t = 0f;
    if (Mathf.Abs(denom) > 0.000001f)
        t = (boundary - av) / denom;

    t = Mathf.Clamp01(t);
    return LerpClipVertex(a, b, t, hasNormals, hasTangents, hasColors);
}

private static List<AtlasedClipVertex> ClipPolygonAgainstBoundary(
    List<AtlasedClipVertex> input,
    int axis,
    float boundary,
    bool keepGreater,
    bool hasNormals,
    bool hasTangents,
    bool hasColors)
{
    List<AtlasedClipVertex> output = new List<AtlasedClipVertex>();
    if (input == null || input.Count == 0)
        return output;

    AtlasedClipVertex prev = input[input.Count - 1];
    bool prevInside = IsInsideBoundary(prev, axis, boundary, keepGreater);

    for (int i = 0; i < input.Count; i++)
    {
        AtlasedClipVertex cur = input[i];
        bool curInside = IsInsideBoundary(cur, axis, boundary, keepGreater);

        if (curInside != prevInside)
        {
            output.Add(IntersectBoundary(prev, cur, axis, boundary, hasNormals, hasTangents, hasColors));
        }

        if (curInside)
            output.Add(cur);

        prev = cur;
        prevInside = curInside;
    }

    return output;
}

private static List<AtlasedClipVertex> ClipTriangleToCell(
    AtlasedClipVertex a,
    AtlasedClipVertex b,
    AtlasedClipVertex c,
    int uCell,
    int vCell,
    bool hasNormals,
    bool hasTangents,
    bool hasColors)
{
    List<AtlasedClipVertex> poly = new List<AtlasedClipVertex>(3);
    poly.Add(a);
    poly.Add(b);
    poly.Add(c);

    poly = ClipPolygonAgainstBoundary(poly, 0, uCell, true, hasNormals, hasTangents, hasColors);
    if (poly.Count < 3) return poly;

    poly = ClipPolygonAgainstBoundary(poly, 0, uCell + 1, false, hasNormals, hasTangents, hasColors);
    if (poly.Count < 3) return poly;

    poly = ClipPolygonAgainstBoundary(poly, 1, vCell, true, hasNormals, hasTangents, hasColors);
    if (poly.Count < 3) return poly;

    poly = ClipPolygonAgainstBoundary(poly, 1, vCell + 1, false, hasNormals, hasTangents, hasColors);
    return poly;
}

private static bool IsDegenerateTriangleUV(AtlasedClipVertex a, AtlasedClipVertex b, AtlasedClipVertex c)
{
    Vector2 ab = b.UV - a.UV;
    Vector2 ac = c.UV - a.UV;
    float area2 = Mathf.Abs((ab.x * ac.y) - (ab.y * ac.x));
    return area2 < 0.000001f;
}

private static int AddSubdividedVertex(
    AtlasedClipVertex v,
    int uCell,
    int vCell,
    Rect rect,
    bool hasNormals,
    bool hasTangents,
    bool hasColors,
    List<Vector3> dstVertices,
    List<Vector3> dstNormals,
    List<Vector4> dstTangents,
    List<Vector2> dstUv,
    List<Color> dstColors)
{
    int index = dstVertices.Count;

    dstVertices.Add(v.Position);

    if (hasNormals)
        dstNormals.Add(v.Normal);

    if (hasTangents)
        dstTangents.Add(v.Tangent);

    if (hasColors)
        dstColors.Add(v.Color);

    float localU = Mathf.Clamp01(v.UV.x - uCell);
    float localV = Mathf.Clamp01(v.UV.y - vCell);

    dstUv.Add(new Vector2(
        rect.x + (localU * rect.width),
        rect.y + (localV * rect.height)));

    return index;
}

private static void AddClippedPolygonToMesh(
    List<AtlasedClipVertex> poly,
    int uCell,
    int vCell,
    Rect rect,
    bool hasNormals,
    bool hasTangents,
    bool hasColors,
    List<Vector3> dstVertices,
    List<Vector3> dstNormals,
    List<Vector4> dstTangents,
    List<Vector2> dstUv,
    List<Color> dstColors,
    List<int> dstTriangles)
{
    if (poly == null || poly.Count < 3)
        return;

    AtlasedClipVertex root = poly[0];

    for (int i = 1; i < poly.Count - 1; i++)
    {
        AtlasedClipVertex b = poly[i];
        AtlasedClipVertex c = poly[i + 1];

        if (IsDegenerateTriangleUV(root, b, c))
            continue;

        int i0 = AddSubdividedVertex(
            root, uCell, vCell, rect,
            hasNormals, hasTangents, hasColors,
            dstVertices, dstNormals, dstTangents, dstUv, dstColors);

        int i1 = AddSubdividedVertex(
            b, uCell, vCell, rect,
            hasNormals, hasTangents, hasColors,
            dstVertices, dstNormals, dstTangents, dstUv, dstColors);

        int i2 = AddSubdividedVertex(
            c, uCell, vCell, rect,
            hasNormals, hasTangents, hasColors,
            dstVertices, dstNormals, dstTangents, dstUv, dstColors);

        dstTriangles.Add(i0);
        dstTriangles.Add(i1);
        dstTriangles.Add(i2);
    }
}

private static bool TryParseArchiveRecordFromMaterialName(string materialName, out int archive, out int record)
{
    archive = -1;
    record = -1;

    if (string.IsNullOrEmpty(materialName))
        return false;

    Match match = TextureNameRegex.Match(materialName);
    if (!match.Success || match.Groups.Count < 3)
    {
        match = TextureNameBracketRegex.Match(materialName);
        if (!match.Success || match.Groups.Count < 3)
            return false;
    }

    if (!int.TryParse(match.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out archive))
        return false;

    if (!int.TryParse(match.Groups[2].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out record))
        return false;

    return true;
}

private static string MakeTextureKey(int archive, int record)
{
    return archive.ToString("000", CultureInfo.InvariantCulture) + ":" +
           record.ToString(CultureInfo.InvariantCulture);
}

private static bool TryParseTextureKey(string textureKey, out int archive, out int record)
{
    archive = -1;
    record = -1;

    if (string.IsNullOrWhiteSpace(textureKey))
        return false;

    string[] parts = textureKey.Split(':');
    if (parts.Length != 2)
        return false;

    if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out archive))
        return false;

    if (!int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out record))
        return false;

    return true;
}

private static bool TryGetTextureKeysForModel(uint modelId, out string[] textureKeys, out string error)
{
    textureKeys = null;
    error = null;

    global::DaggerfallWorkshop.DaggerfallUnity dfUnity = global::DaggerfallWorkshop.DaggerfallUnity.Instance;
    if (dfUnity == null || !dfUnity.IsReady)
    {
        error = "DaggerfallUnity is not ready.";
        return false;
    }

    global::DaggerfallWorkshop.ModelData modelData;
    if (!dfUnity.MeshReader.GetModelData(modelId, out modelData))
    {
        error = "MeshReader.GetModelData failed for model " + modelId;
        return false;
    }

    if (modelData.DFMesh.SubMeshes == null || modelData.DFMesh.SubMeshes.Length == 0)
    {
        error = "Model has no DFMesh submeshes.";
        return false;
    }

    textureKeys = new string[modelData.DFMesh.SubMeshes.Length];
    for (int i = 0; i < modelData.DFMesh.SubMeshes.Length; i++)
    {
        int archive = modelData.DFMesh.SubMeshes[i].TextureArchive;
        int record = modelData.DFMesh.SubMeshes[i].TextureRecord;
        textureKeys[i] = MakeTextureKey(archive, record);
    }

    return true;
}

private static Texture2D MakeReadableTextureCopy(Texture source)
{
    if (source == null)
        return null;

    Texture2D source2D = source as Texture2D;
    int width = source2D != null ? source2D.width : 2;
    int height = source2D != null ? source2D.height : 2;

    RenderTexture rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
    RenderTexture previous = RenderTexture.active;

    try
    {
        Graphics.Blit(source, rt);
        RenderTexture.active = rt;

        Texture2D copy = new Texture2D(width, height, TextureFormat.RGBA32, false);
        copy.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        copy.Apply(false, false);
        copy.filterMode = FilterMode.Point;
        copy.wrapMode = TextureWrapMode.Clamp;
        return copy;
    }
    finally
    {
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);
    }
}
private static void RemapMeshUvInPlace(Mesh mesh, Vector2 scale, Vector2 offset)
{
    if (mesh == null)
        return;

    Vector2[] uv = mesh.uv;
    if (uv == null || uv.Length == 0)
        return;

    float minU = float.PositiveInfinity;
    float minV = float.PositiveInfinity;
    float maxU = float.NegativeInfinity;
    float maxV = float.NegativeInfinity;

    for (int i = 0; i < uv.Length; i++)
    {
        if (uv[i].x < minU) minU = uv[i].x;
        if (uv[i].y < minV) minV = uv[i].y;
        if (uv[i].x > maxU) maxU = uv[i].x;
        if (uv[i].y > maxV) maxV = uv[i].y;
    }

    float width = maxU - minU;
    float height = maxV - minV;

    if (width <= 0.000001f || height <= 0.000001f)
        return;

    for (int i = 0; i < uv.Length; i++)
    {
        float normalizedU = (uv[i].x - minU) / width;
        float normalizedV = (uv[i].y - minV) / height;

        uv[i] = new Vector2(normalizedU, normalizedV);
    }

    mesh.uv = uv;
}
private static bool TryLoadDfuTexture(string textureKey, out Texture2D texture, out string error)
{
    texture = null;
    error = null;

    int archive;
    int record;
    if (!TryParseTextureKey(textureKey, out archive, out record))
    {
        error = "Invalid texture key: " + textureKey;
        return false;
    }

    global::DaggerfallWorkshop.DaggerfallUnity dfUnity = global::DaggerfallWorkshop.DaggerfallUnity.Instance;
    if (dfUnity == null || !dfUnity.IsReady)
    {
        error = "DaggerfallUnity is not ready.";
        return false;
    }

    global::DaggerfallWorkshop.Utility.TextureReader reader =
        new global::DaggerfallWorkshop.Utility.TextureReader(dfUnity.Arena2Path);

    Texture2D dfuTexture = reader.GetTexture2D(archive, record, 0, 0);
    if (dfuTexture == null)
    {
        error = string.Format(
            CultureInfo.InvariantCulture,
            "DFU TextureReader returned null for archive {0:000}, record {1}.",
            archive,
            record);
        return false;
    }

    texture = MakeReadableTextureCopy(dfuTexture);
    if (texture == null)
    {
        error = string.Format(
            CultureInfo.InvariantCulture,
            "Failed making readable copy for archive {0:000}, record {1}.",
            archive,
            record);
        return false;
    }

    texture.name = string.Format(
        CultureInfo.InvariantCulture,
        "TEXTURE_{0:000}_Index_{1}_0",
        archive,
        record);

    return true;
}

private static Type GetWorldDataEditorObjectDataType()
{
    const string fullName = "DaggerfallWorkshop.Game.Utility.WorldDataEditor.WorldDataEditorObjectData";

    Type type = Type.GetType(fullName);
    if (type != null)
        return type;

    Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
    for (int i = 0; i < assemblies.Length; i++)
    {
        Assembly asm = assemblies[i];
        if (asm == null)
            continue;

        type = asm.GetType(fullName, false);
        if (type != null)
            return type;
    }

    return null;
}
private static bool TryParseCategoryEntryToModelId(string raw, out uint modelId)
{
    modelId = 0;

    if (string.IsNullOrWhiteSpace(raw))
        return false;

    raw = raw.Trim();

    if (raw.Contains("."))
        return false;

    return uint.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out modelId);
}



private static bool TryGetModelGroupsDictionary(out Dictionary<string, string[]> modelGroups)
{
    modelGroups = null;

    Type type = GetWorldDataEditorObjectDataType();
    if (type == null)
        return false;

    FieldInfo field = type.GetField("modelGroups", BindingFlags.Public | BindingFlags.Static);
    if (field == null)
        return false;

    object rawValue = field.GetValue(null);
    modelGroups = rawValue as Dictionary<string, string[]>;
    return modelGroups != null;
}

private static bool TryGetFlatGroupsDictionary(out Dictionary<string, string[]> flatGroups)
{
    flatGroups = null;

    Type type = GetWorldDataEditorObjectDataType();
    if (type == null)
        return false;

    FieldInfo field = type.GetField("flatGroups", BindingFlags.Public | BindingFlags.Static);
    if (field == null)
        return false;

    object rawValue = field.GetValue(null);
    flatGroups = rawValue as Dictionary<string, string[]>;
    return flatGroups != null;
}

private static bool TryGetFlatGroupEntryForArchiveRecord(
    int archive,
    int record,
    out string groupName,
    out string[] groupIds,
    out int indexZeroBased)
{
    groupName = null;
    groupIds = null;
    indexZeroBased = -1;

    Dictionary<string, string[]> flatGroups;
    if (!TryGetFlatGroupsDictionary(out flatGroups) || flatGroups == null)
        return false;

    string target = MakeFlatArchiveRecordKey(archive, record);

    foreach (KeyValuePair<string, string[]> kvp in flatGroups)
    {
        string[] ids = kvp.Value;
        if (ids == null || ids.Length == 0)
            continue;

        for (int i = 0; i < ids.Length; i++)
        {
            if (string.Equals(ids[i], target, StringComparison.OrdinalIgnoreCase))
            {
                groupName = kvp.Key;
                groupIds = ids;
                indexZeroBased = i;
                return true;
            }
        }
    }

    return false;
}
private List<ExportRequest> ExpandFlatRequestsByGroupedFamilies(List<ExportRequest> requests)
{
    if (!exportGroupedFamilyMembers || requests == null || requests.Count == 0)
        return requests ?? new List<ExportRequest>();

    return ExpandFlatRequestsByGroupedFamiliesForced(requests);
}

private List<ExportRequest> ExpandFlatRequestsByGroupedFamiliesForced(List<ExportRequest> requests)
{
    if (requests == null || requests.Count == 0)
        return requests ?? new List<ExportRequest>();

    List<ExportRequest> expanded = new List<ExportRequest>();
    HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    for (int i = 0; i < requests.Count; i++)
    {
        ExportRequest request = requests[i];
        if (request == null || !request.IsFlat)
            continue;

        string groupName;
        string[] groupIds;
        int indexZeroBased;

        if (TryGetFlatGroupEntryForArchiveRecord(
            request.FlatArchive,
            request.FlatRecord,
            out groupName,
            out groupIds,
            out indexZeroBased) &&
            groupIds != null &&
            groupIds.Length > 0)
        {
            for (int j = 0; j < groupIds.Length; j++)
            {
                int groupedArchive;
                int groupedRecord;
                if (!TryParseFlatArchiveRecord(groupIds[j], out groupedArchive, out groupedRecord))
                    continue;

                string dedupeKey = MakeFlatArchiveRecordKey(groupedArchive, groupedRecord);
                if (!seen.Add(dedupeKey))
                    continue;

                expanded.Add(new ExportRequest
                {
                    IsFlat = true,
                    FlatArchive = groupedArchive,
                    FlatRecord = groupedRecord,
                });
            }
        }
        else
        {
            string dedupeKey = MakeFlatArchiveRecordKey(request.FlatArchive, request.FlatRecord);
            if (!seen.Add(dedupeKey))
                continue;

            expanded.Add(request);
        }
    }

    return expanded;
}


private static bool TryGetModelGroupEntryForModelId(uint modelId, out string groupName, out string[] groupIds, out int indexZeroBased)
{
    groupName = null;
    groupIds = null;
    indexZeroBased = -1;

    Dictionary<string, string[]> modelGroups;
    if (!TryGetModelGroupsDictionary(out modelGroups) || modelGroups == null)
        return false;

    string target = modelId.ToString(CultureInfo.InvariantCulture);

    foreach (KeyValuePair<string, string[]> kvp in modelGroups)
    {
        string[] ids = kvp.Value;
        if (ids == null || ids.Length == 0)
            continue;

        for (int i = 0; i < ids.Length; i++)
        {
            if (string.Equals(ids[i], target, StringComparison.Ordinal))
            {
                groupName = kvp.Key;
                groupIds = ids;
                indexZeroBased = i;
                return true;
            }
        }
    }

    return false;
}
private bool TryGetResolvedDisplayNameForModelId(
    string preferredFieldName,
    uint modelId,
    out string resolvedFieldName,
    out string displayName)
{
    resolvedFieldName = null;
    displayName = null;

    if (!string.IsNullOrWhiteSpace(preferredFieldName) &&
        TryGetCategoryDisplayNameForModelId(preferredFieldName, modelId, out displayName) &&
        !string.IsNullOrWhiteSpace(displayName))
    {
        resolvedFieldName = preferredFieldName;
        return true;
    }

    if (TryGetAnyCategoryDisplayNameForModelId(modelId, out resolvedFieldName, out displayName) &&
        !string.IsNullOrWhiteSpace(displayName))
    {
        return true;
    }

    string groupName;
    string[] groupIds;
    int groupIndex;
    if (TryGetModelGroupEntryForModelId(modelId, out groupName, out groupIds, out groupIndex) &&
        !string.IsNullOrWhiteSpace(groupName))
    {
        resolvedFieldName = string.IsNullOrWhiteSpace(preferredFieldName)
            ? TryGetAnyCategoryFieldNameForModelId(modelId)
            : preferredFieldName;
        displayName = groupName;
        return true;
    }

    return false;
}


private string TryGetAnyCategoryFieldNameForModelId(uint modelId)
{
    string resolvedFieldName;
    string displayName;
    if (TryGetAnyCategoryDisplayNameForModelId(modelId, out resolvedFieldName, out displayName))
        return resolvedFieldName;
    return null;
}

private List<uint> ExpandModelIdsByGroupedFamilies(List<uint> ids)
{
    if (!exportGroupedFamilyMembers || ids == null || ids.Count == 0)
        return ids ?? new List<uint>();

    SortedSet<uint> expanded = new SortedSet<uint>();

    for (int i = 0; i < ids.Count; i++)
    {
        uint modelId = ids[i];

        string groupName;
        string[] groupIds;
        int indexZeroBased;
        if (TryGetModelGroupEntryForModelId(modelId, out groupName, out groupIds, out indexZeroBased) &&
            groupIds != null &&
            groupIds.Length > 0)
        {
            for (int j = 0; j < groupIds.Length; j++)
            {
                uint groupedId;
                if (uint.TryParse(groupIds[j], NumberStyles.Integer, CultureInfo.InvariantCulture, out groupedId))
                    expanded.Add(groupedId);
            }
        }
        else
        {
            expanded.Add(modelId);
        }
    }

    return new List<uint>(expanded);
}

private bool TryGetModelIdsFromCategorySource(string fieldName, out List<uint> ids, out string error)
{
    ids = new List<uint>();
    error = null;

    if (string.IsNullOrWhiteSpace(fieldName))
    {
        error = "Field name was empty.";
        return false;
    }

    Type type = GetWorldDataEditorObjectDataType();
    if (type == null)
    {
        error = "Could not find WorldDataEditorObjectData type.";
        return false;
    }

    bool isShipsCategory = string.Equals(fieldName, "ships", StringComparison.OrdinalIgnoreCase);
    bool isStructureCategory = string.Equals(fieldName, "models_structure", StringComparison.OrdinalIgnoreCase);

    string sourceFieldName = isShipsCategory ? "models_structure" : fieldName;

    FieldInfo field = type.GetField(sourceFieldName, BindingFlags.Public | BindingFlags.Static);
    if (field == null)
    {
        error = "Could not find field '" + sourceFieldName + "'.";
        return false;
    }

    object rawValue = field.GetValue(null);
    if (rawValue == null)
    {
        error = "Field '" + sourceFieldName + "' was null.";
        return false;
    }

    SortedSet<uint> set = new SortedSet<uint>();

    Dictionary<string, string[]> stringArrayDict = rawValue as Dictionary<string, string[]>;
    if (stringArrayDict != null)
    {
        foreach (KeyValuePair<string, string[]> kvp in stringArrayDict)
        {
            bool isShipLike = IsShipOrBoatCategoryLabel(kvp.Key);

            if (isShipsCategory && !isShipLike)
                continue;

            if (isStructureCategory && isShipLike)
                continue;

            if (kvp.Value == null)
                continue;

            for (int i = 0; i < kvp.Value.Length; i++)
            {
                uint id;
                if (TryParseCategoryEntryToModelId(kvp.Value[i], out id))
                    set.Add(id);
            }
        }

        ids = new List<uint>(set);
        if (ids.Count == 0)
            error = "Field '" + fieldName + "' did not contain any exportable model IDs.";

        return ids.Count > 0;
    }

    Dictionary<string, string> stringDict = rawValue as Dictionary<string, string>;
    if (stringDict != null)
    {
        foreach (KeyValuePair<string, string> kvp in stringDict)
        {
            bool isShipLike = IsShipOrBoatCategoryLabel(kvp.Value);

            if (isShipsCategory && !isShipLike)
                continue;

            if (isStructureCategory && isShipLike)
                continue;

            uint id;
            if (TryParseCategoryEntryToModelId(kvp.Key, out id))
                set.Add(id);
        }

        ids = new List<uint>(set);
        if (ids.Count == 0)
            error = "Field '" + fieldName + "' did not contain any exportable model IDs.";

        return ids.Count > 0;
    }

    if (rawValue is System.Collections.IDictionary genericDict)
    {
        foreach (System.Collections.DictionaryEntry entry in genericDict)
        {
            string keyString = entry.Key as string;
            string valueString = entry.Value as string;
            string[] valueArray = entry.Value as string[];

            string labelForFilter = null;

            if (!string.IsNullOrWhiteSpace(valueString))
                labelForFilter = valueString;
            else if (!string.IsNullOrWhiteSpace(keyString))
                labelForFilter = keyString;

            bool isShipLike = IsShipOrBoatCategoryLabel(labelForFilter);

            if (isShipsCategory && !isShipLike)
                continue;

            if (isStructureCategory && isShipLike)
                continue;

            if (!string.IsNullOrWhiteSpace(keyString))
            {
                uint id;
                if (TryParseCategoryEntryToModelId(keyString, out id))
                    set.Add(id);
            }

            if (valueArray != null)
            {
                for (int i = 0; i < valueArray.Length; i++)
                {
                    uint id;
                    if (TryParseCategoryEntryToModelId(valueArray[i], out id))
                        set.Add(id);
                }
            }
        }

        ids = new List<uint>(set);
        if (ids.Count == 0)
            error = "Field '" + fieldName + "' did not contain any exportable model IDs.";

        return ids.Count > 0;
    }

    error = "Field '" + sourceFieldName + "' was not a supported dictionary type. Actual type: " + rawValue.GetType().FullName;
    return false;
}

private static bool IsShipOrBoatCategoryLabel(string label)
{
    if (string.IsNullOrWhiteSpace(label))
        return false;

    string cleaned = StripBracketCountSuffix(label);

    if (cleaned.IndexOf("Shipwreck", StringComparison.OrdinalIgnoreCase) >= 0)
        return true;

    return Regex.IsMatch(
        cleaned,
        @"\b(Ship|Ships|Boat|Boats)\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
}

private static string StripBracketCountSuffix(string label)
{
    if (string.IsNullOrEmpty(label))
        return string.Empty;

    return Regex.Replace(label, @"\s*\[\d+\]\s*$", string.Empty).Trim();
}

private static bool LabelHasBracketCountSuffix(string label)
{
    if (string.IsNullOrEmpty(label))
        return false;

    return Regex.IsMatch(label, @"\[\d+\]\s*$");
}

private static readonly Dictionary<string, string> ExactExportWordReplacements =
    new Dictionary<string, string>(StringComparer.Ordinal)
{
    { "Altars", "Altar" },
    { "Beds", "Bed" },
    { "Chairs", "Chair" },
    { "Benches", "Bench" },
    { "Lecterns", "Lectern" },
    { "Tables", "Table" },
    { "Thrones", "Throne" },
    { "Grates", "Grate" },
    { "Crates", "Crate" },
    { "Chests", "Chest" },
    { "Drawers", "Drawer" },
    { "Shelves", "Shelf" },
    { "Ships", "Ship" },
    { "Banners", "Banner" },
    { "Carpets", "Carpet" },
    { "Rocks", "Rock" },
    { "Swords", "Sword" },
    { "Tents", "Tent" },
    { "Coffins", "Coffin" },
    { "Statues", "Statue" },
    { "Chutes", "Chute" },
    { "Paintings", "Painting" },
    { "Hexagon", "Hex" },
    { ",", "-" },
    { "Hexagonal", "Hex" },
    { "SwitchLevers", "SwitchLever" },
    { "Switchlevers", "SwitchLever" },
     //NPC FLATS
    { "Beggars", "Beggar" },
    { "Children", "Child" },
    { "Commonersmen", "CommonerMan" },
    { "Commonerswomen", "CommonerWoman" },
    { "CommonersMen", "CommonerMan" },
    { "CommonersWomen", "CommonerWoman" },
    { "Daedricprinces", "DaedricPrince" },
    { "Darkbrotherhood", "DarkBrotherhood" },
    { "Elders", "Elder" },
    { "Jesters", "Jester" },
    { "Knights", "Knight" },
    { "Mages", "Mage" },
    { "Musicians", "Musician" },
    { "Necromancers", "Necromancer" },
    { "Noblemen", "Nobleman" },
    { "Noblewomen", "Noblewoman" },
    { "Prostitutes", "Prostitute" },
    { "Smiths", "Smith" },
    { "Vampires", "Vampire" },
    { "Witchcovens", "WitchCoven" },
    { "WitchCovens", "WitchCoven" }

   


};

private static readonly Dictionary<string, string> PrefixExportWordReplacements =
    new Dictionary<string, string>(StringComparer.Ordinal)
{
    { "Shelf", "Shelf" },
    { "Corrarched", "CorrArched" }
};

private static readonly Dictionary<string, string> ExactSanitizedTokenOverrides =
    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
{
    { "Bannersen", "BannerSen" },
    { "Bannersml", "BannerSml" },
    { "Rockslarge", "RockLarge" },
    { "Rocksmedium", "RockMedium" },
    { "Rockssmall", "RockSmall" },
    { "CorrArchedcornerporticullis", "CorrArchedCornerPortcullis" },
    { "CorrHexTransarched", "CorrHexTransArched" },
    { "SecretDoorHexagonal", "SecretDoorHex" },
    { "CircularStaircasemid", "CircularStaircaseMid" },
    { "SecretDoorHexagonalLarge", "SecretDoorHexLarge" },
        { "Statuelarg", "StatueLarge" }
};

private static readonly Dictionary<string, Dictionary<string, string>> CategoryContainsTypoFixes =
    new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
{
    {
        "dungeonParts_corridors",
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "porticullis", "Portcullis" },
            { "cornerporticullis", "CornerPortcullis" }
        }
    }
};
private static readonly Dictionary<string, string[]> CategoryContainsCapitalizationTweaks =

new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
{
    {
        "dungeonParts_corridors",
        new[]
        {
            "Hex",
            "Way",
            "Chute",
            "Ceiling",
            "Trans",
            "Cave",
            "Diag",
            "Floor",
            "Door",
            "Window",
            "Square",
            "Corner",
            "DeadEnd",
            "Ramp",
            "Slope",
            "Beams",
            "Stairs",
            "Junction",
            "Corridor",
            "Niches",
            "Narrow",
            "Deep"
        }
    },

    { "dungeonParts_caves", new[]
        {
            "DonkSkillet",
            "Doberman"
        }
    },
    { "dungeonParts_doors", new[]
        {
            "DoorHexagonal",
            "DoorHexagonalLarge",
            "DoorLarge",
            "DoorSmall",
            "DoorStandard",
        }
    },
    { "dungeonParts_misc", new[]
        {
            "Staircase",
            "RoomMid",
            "Landing",
            "Bottom",
            "Bridge",
            "BridgeEnd",
            "BridgeMid",
            "PlatformMid",
            "RampSmall",
            "StairsCorner",
            "Gap",
           
        }
    },
    { "dungeonParts_rooms", new[]
        {
            "DonkSkillet",
            "Doberman"
        }
    },
    { "houseParts", new[]
        {
            "DonkSkillet",
            "Doberman"
        }
    },
    { "models_clutter", new[]
        {
            "BannerDag",
            "BannerDir",
            "BannerDwy",
            "BannerLrg",
            "BannerOfl",
            "BannerOla",
            "BannerSen",//ends up Banneren due to Banners to Banner happening first.
            "BannerSml",//ends up Bannerml due to Banners to Banner happening first.
            "BannerWay",  
            "RockLarge",
            "RockMedium",
            "RockSmall",
            "POTATICUS",
        }
    },
    { "models_dungeon", new[]
        {
            "Stone",
            "Wood"
        }
    },
    { "models_furniture", new[]
        {
            "DonkSkillet",
            "Doberman"
        }
    },
    { "models_graveyard", new[]
        {
            "DonkSkillet",
            "Doberman"
        }
    },
    { "ships", new[]
        {
            "DonkSkillet",
            "Doberman"
        }
    },
    { "models_structure", new[]
        {
            "DonkSkillet",
            "Doberman"
        }
    }
};

private static string SanitizeExportToken(string raw)
{
    if (string.IsNullOrEmpty(raw))
        return string.Empty;

    raw = raw.Replace(" ", string.Empty);
    raw = raw.Replace("-", string.Empty);
    raw = raw.Replace("(", string.Empty);
    raw = raw.Replace(")", string.Empty);
    raw = raw.Replace("[", string.Empty);
    raw = raw.Replace("]", string.Empty);

    raw = raw.Replace("/", string.Empty);
    raw = raw.Replace("\\", string.Empty);
    raw = raw.Replace(":", string.Empty);
    raw = raw.Replace("*", string.Empty);
    raw = raw.Replace("?", string.Empty);
    raw = raw.Replace("\"", string.Empty);
    raw = raw.Replace("<", string.Empty);
    raw = raw.Replace(">", string.Empty);
    raw = raw.Replace("|", string.Empty);

    raw = raw.Replace(".", string.Empty);

    return raw.Trim();
}
private static string ApplyExportWordReplacements(string token)
{
    if (string.IsNullOrEmpty(token))
        return string.Empty;

    string replaced;

    if (ExactSanitizedTokenOverrides.TryGetValue(token, out replaced) &&
        !string.IsNullOrEmpty(replaced))
    {
        return replaced;
    }

    if (ExactExportWordReplacements.TryGetValue(token, out replaced) &&
        !string.IsNullOrEmpty(replaced))
    {
        return replaced;
    }

    foreach (KeyValuePair<string, string> kvp in ExactExportWordReplacements)
    {
        if (token.StartsWith(kvp.Key, StringComparison.Ordinal) &&
            token.Length > kvp.Key.Length &&
            IsSafeWordBoundaryForLeadingReplacement(token, kvp.Key.Length))
        {
            return kvp.Value + token.Substring(kvp.Key.Length);
        }
    }

    foreach (KeyValuePair<string, string> kvp in PrefixExportWordReplacements)
    {
        if (token.StartsWith(kvp.Key, StringComparison.Ordinal) &&
            token.Length > kvp.Key.Length)
        {
            return kvp.Value + token.Substring(kvp.Key.Length);
        }
    }

    return token;
}

private static bool IsSafeWordBoundaryForLeadingReplacement(string token, int prefixLength)
{
    if (string.IsNullOrEmpty(token))
        return false;

    if (prefixLength < 0 || prefixLength >= token.Length)
        return false;

    char nextChar = token[prefixLength];

    if (char.IsLower(nextChar))
        return false;

    return true;
}
private static string ToReadableExportToken(string raw)
{
    return ToReadableExportToken(raw, null);
}
private static string ToReadableExportToken(string raw, string categoryFieldNameOrNull)
{
    string sanitized = SanitizeExportToken(raw);
    if (string.IsNullOrEmpty(sanitized))
        return string.Empty;

    if (sanitized.IndexOfAny(new[] { '_', ' ' }) >= 0)
    {
        string[] parts = sanitized.Split(new[] { '_', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i].Length > 0)
                parts[i] = char.ToUpperInvariant(parts[i][0]) + parts[i].Substring(1);
        }

        sanitized = string.Join(string.Empty, parts);
    }
    else if (char.IsLower(sanitized[0]))
    {
        sanitized = char.ToUpperInvariant(sanitized[0]) + sanitized.Substring(1);
    }

    // Capitalize each embedded camelCase word boundary so that e.g.
    // "CorrArchedcornerPortcullis" → "CorrArchedCornerPortcullis"
    sanitized = CapitalizeEmbeddedCamelCaseWords(sanitized);

    sanitized = ApplyExportWordReplacements(sanitized);
    sanitized = ApplyCategoryContainsCapitalizationTweaks(sanitized, categoryFieldNameOrNull);
    return sanitized;
}

private static string CapitalizeEmbeddedCamelCaseWords(string token)
{
    if (string.IsNullOrEmpty(token))
        return token;

    // Walk the string; whenever we see a lowercase→uppercase transition,
    // that uppercase letter is already correct. But a lowercase letter
    // that follows an uppercase run (e.g. "Arched[c]orner") needs to be
    // checked: if it starts a new run of lowercase letters that follows an
    // uppercase letter, capitalize it only when the preceding uppercase
    // letter was NOT itself preceded by another uppercase letter
    // (i.e., it is the start of a new PascalCase word, not mid-acronym).
    //
    // Simpler reliable approach: split on camelCase boundaries then re-join
    // with each segment's first char uppercased.
    //
    // Boundary = position where char[i] is uppercase AND char[i-1] is lowercase.
    // Also treat char[i] uppercase AND char[i-1] uppercase AND char[i+1] lowercase
    // as a boundary (acronym end, e.g. "XMLParser" → "XML"+"Parser").

    var sb = new System.Text.StringBuilder(token.Length);
    int start = 0;
    for (int i = 1; i <= token.Length; i++)
    {
        bool boundary = (i == token.Length);
        if (!boundary && i > 0)
        {
            char prev = token[i - 1];
            char cur  = token[i];
            // lowercase → uppercase
            if (char.IsLower(prev) && char.IsUpper(cur))
                boundary = true;
            // uppercase run → uppercase+lowercase  (e.g. "ABCdef": split before 'C')
            else if (i >= 2 && char.IsUpper(prev) && char.IsUpper(token[i - 2]) && char.IsLower(cur))
                boundary = true;
        }

        if (boundary)
        {
            string segment = token.Substring(start, i - start);
            if (segment.Length > 0)
                sb.Append(char.ToUpperInvariant(segment[0]));
            if (segment.Length > 1)
                sb.Append(segment, 1, segment.Length - 1);
            start = i;
        }
    }

    return sb.ToString();
}
private static string BuildDefaultExportBaseName(uint modelId)
{
    return "Model_id" +
           modelId.ToString(CultureInfo.InvariantCulture) +
           "_df";
}
private static string BuildCategoryExportBaseName(string dictionaryLabel, uint modelId)
{
    return BuildCategoryExportBaseName(dictionaryLabel, modelId, null);
}

private static string BuildCategoryExportBaseName(string dictionaryLabel, uint modelId, string categoryFieldNameOrNull)
{
    string cleanedLabel = StripBracketCountSuffix(dictionaryLabel);

    if (Regex.IsMatch(cleanedLabel ?? string.Empty, @"\bBoats\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        cleanedLabel = Regex.Replace(cleanedLabel, @"\bBoats\b", "Boat", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    if (Regex.IsMatch(cleanedLabel ?? string.Empty, @"\bShips\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        cleanedLabel = Regex.Replace(cleanedLabel, @"\bShips\b", "Ship", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    string baseName = ToReadableExportToken(cleanedLabel, categoryFieldNameOrNull);
    if (string.IsNullOrEmpty(baseName))
        baseName = "Model";

    string modelIdText = modelId.ToString(CultureInfo.InvariantCulture);

    return baseName +
           "_id" + modelIdText +
           "_df";
}
private static string ApplyCategoryContainsCapitalizationTweaks(string token, string categoryFieldNameOrNull)
{
    if (string.IsNullOrEmpty(token) || string.IsNullOrWhiteSpace(categoryFieldNameOrNull))
        return token;

    string result = token;

    Dictionary<string, string> typoFixes;
    if (CategoryContainsTypoFixes.TryGetValue(categoryFieldNameOrNull, out typoFixes) &&
        typoFixes != null)
    {
        foreach (KeyValuePair<string, string> kvp in typoFixes)
        {
            if (string.IsNullOrWhiteSpace(kvp.Key) || string.IsNullOrWhiteSpace(kvp.Value))
                continue;

            result = ReplaceAllCaseInsensitiveLiteral(result, kvp.Key, kvp.Value);
        }
    }

    if (result.EndsWith("Sml", StringComparison.Ordinal))
        result = result.Substring(0, result.Length - 3) + "Small";

    if (result.EndsWith("Lrg", StringComparison.Ordinal))
        result = result.Substring(0, result.Length - 3) + "Large";

    string[] tweakParts;
    if (!CategoryContainsCapitalizationTweaks.TryGetValue(categoryFieldNameOrNull, out tweakParts) ||
        tweakParts == null ||
        tweakParts.Length == 0)
    {
        return result;
    }

    for (int i = 0; i < tweakParts.Length; i++)
    {
        string desired = tweakParts[i];
        if (string.IsNullOrWhiteSpace(desired))
            continue;

        result = ReplaceAllCaseInsensitiveLiteral(result, desired.ToLowerInvariant(), desired);
    }

    return result;
}

private static string ReplaceAllCaseInsensitiveLiteral(string input, string lowerNeedle, string replacement)
{
    if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(lowerNeedle))
        return input;

    string workingLower = input.ToLowerInvariant();
    int searchStart = 0;

    while (true)
    {
        int foundIndex = workingLower.IndexOf(lowerNeedle, searchStart, StringComparison.Ordinal);
        if (foundIndex < 0)
            break;

        input = input.Substring(0, foundIndex) +
                replacement +
                input.Substring(foundIndex + lowerNeedle.Length);

        workingLower = input.ToLowerInvariant();
        searchStart = foundIndex + replacement.Length;
    }

    return input;
}
private static string BuildFbxAssetRelativePath(string exportBaseName)
{
    string safeExportBaseName = SanitizeExportToken(exportBaseName);
    if (string.IsNullOrEmpty(safeExportBaseName))
        safeExportBaseName = "Model";

   return FbxModelFolderAssetRelative + "/" + safeExportBaseName + ".fbx";
}

private static string BuildCategoryFbxFolderAssetRelativePath(string categoryFieldName)
{
    string folderName = BuildSharedBatchAssetBaseName(categoryFieldName);
    if (string.IsNullOrEmpty(folderName))
        folderName = "Category";

   return FbxModelFolderAssetRelative + "/" + folderName;
}

private static string BuildFlatFbxAssetRelativePath(string exportBaseName)
{
    string safeExportBaseName = SanitizeExportToken(exportBaseName);
    if (string.IsNullOrEmpty(safeExportBaseName))
        safeExportBaseName = "Flat";

    return FbxFlatFolderAssetRelative + "/" + safeExportBaseName + ".fbx";
}

private static string BuildFlatCategoryFbxFolderAssetRelativePath(string flatGroupName)
{
    string folderName = BuildSharedFlatBatchAssetBaseName(flatGroupName);
    if (string.IsNullOrEmpty(folderName))
        folderName = "FlatGroup";

    return FbxFlatFolderAssetRelative + "/" + folderName;
}

private static string BuildSingleFlatTextureAssetRelativePath(string exportBaseName)
{
    string safeExportBaseName = SanitizeExportToken(exportBaseName);
    if (string.IsNullOrEmpty(safeExportBaseName))
        safeExportBaseName = "Flat";

    return TempTextureFolderAssetRelative + "/" + safeExportBaseName + ".png";
}

private static string BuildSingleFlatMaterialAssetRelativePath(string exportBaseName)
{
    return MaterialFolderAssetRelative + "/" + exportBaseName + "_Flat.mat";
}

private static string BuildFbxAssetRelativePath(string exportBaseName, string categoryFieldNameOrNull, bool useCategorySubfolder)
{
    string safeExportBaseName = SanitizeExportToken(exportBaseName);
    if (string.IsNullOrEmpty(safeExportBaseName))
        safeExportBaseName = "Model";

    if (!useCategorySubfolder || string.IsNullOrWhiteSpace(categoryFieldNameOrNull))
        return BuildFbxAssetRelativePath(safeExportBaseName);

        string folder = BuildCategoryFbxFolderAssetRelativePath(categoryFieldNameOrNull);
    EnsureFolderExists(folder);

    return folder + "/" + safeExportBaseName + ".fbx";
}

private static string BuildSingleAtlasAssetRelativePath(string exportBaseName)
{
    return AtlasFolderAssetRelative + "/" + exportBaseName + "_Atlas.png";
}

private static string BuildSingleMaterialAssetRelativePath(string exportBaseName)
{
    return MaterialFolderAssetRelative + "/" + exportBaseName + ".mat";
}

private static bool AssetExistsAtAssetPath(string assetPath)
{
    if (string.IsNullOrEmpty(assetPath))
        return false;

    string abs = GetAbsolutePathFromAssetPath(assetPath);
    return !string.IsNullOrEmpty(abs) && File.Exists(abs);
}

private bool ShouldSkipSingleExportBecauseAssetsExist(string exportBaseName)
{
    if (overwriteExistingExportAssets)
        return false;

    return AssetExistsAtAssetPath(BuildFbxAssetRelativePath(exportBaseName)) ||
           AssetExistsAtAssetPath(BuildSingleAtlasAssetRelativePath(exportBaseName)) ||
           AssetExistsAtAssetPath(BuildSingleMaterialAssetRelativePath(exportBaseName));
}

private bool ShouldSkipSharedBatchBecauseAssetsExist(string batchBaseName)
{
    if (overwriteExistingExportAssets)
        return false;

    string atlasFolderAbs = GetAbsolutePathFromAssetPath(AtlasFolderAssetRelative);
    if (!string.IsNullOrEmpty(atlasFolderAbs) && Directory.Exists(atlasFolderAbs))
    {
        string[] atlasFiles = Directory.GetFiles(atlasFolderAbs, batchBaseName + "_Atlas_df*.png", SearchOption.TopDirectoryOnly);
        if (atlasFiles != null && atlasFiles.Length > 0)
            return true;
    }

    string materialFolderAbs = GetAbsolutePathFromAssetPath(MaterialFolderAssetRelative);
    if (!string.IsNullOrEmpty(materialFolderAbs) && Directory.Exists(materialFolderAbs))
    {
        string[] materialFiles = Directory.GetFiles(materialFolderAbs, batchBaseName + "_Atlas_df*.mat", SearchOption.TopDirectoryOnly);
        if (materialFiles != null && materialFiles.Length > 0)
            return true;
    }

    return false;
}
private bool TryGetAnyCategoryDisplayNameForModelId(uint modelId, out string categoryFieldName, out string displayName)
{
    categoryFieldName = null;
    displayName = null;

    for (int i = 0; i < FixedCategoryBatchSources.Length; i++)
    {
        string candidate;
        if (TryGetCategoryDisplayNameForModelId(FixedCategoryBatchSources[i], modelId, out candidate) &&
            !string.IsNullOrWhiteSpace(candidate))
        {
            categoryFieldName = FixedCategoryBatchSources[i];
            displayName = candidate;
            return true;
        }
    }

    return false;
}

private string BuildResolvedExportBaseNameForModelId(uint modelId)
{
    string resolvedFieldName;
    string label;
    if (TryGetResolvedDisplayNameForModelId(null, modelId, out resolvedFieldName, out label) &&
        !string.IsNullOrWhiteSpace(label))
    {
        return BuildCategoryExportBaseName(label, modelId, resolvedFieldName);
    }

    return BuildDefaultExportBaseName(modelId);
}

private string BuildResolvedExportBaseNameForModelId(string preferredFieldName, uint modelId)
{
    string resolvedFieldName;
    string label;
    if (TryGetResolvedDisplayNameForModelId(preferredFieldName, modelId, out resolvedFieldName, out label) &&
        !string.IsNullOrWhiteSpace(label))
    {
        return BuildCategoryExportBaseName(label, modelId, resolvedFieldName);
    }

    return BuildDefaultExportBaseName(modelId);
}

private static bool TryParseFlatArchiveRecord(string text, out int archive, out int record)
{
    archive = -1;
    record = -1;

    if (string.IsNullOrWhiteSpace(text))
        return false;

    string trimmed = text.Trim();
    string[] parts = trimmed.Split('.');

    if (parts.Length != 2)
        return false;

    if (!int.TryParse(parts[0].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out archive))
        return false;

    if (!int.TryParse(parts[1].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out record))
        return false;

    if (archive < 0 || record < 0)
        return false;

    return true;
}

private string BuildDefaultExportBaseNameForFlat(int archive, int record)
{
    return "Flat_" +
           archive.ToString("000", CultureInfo.InvariantCulture) +
           "_" +
           record.ToString(CultureInfo.InvariantCulture) +
           "_df";
}

private static bool TryParseCategoryEntryToFlatArchiveRecord(string raw, out int archive, out int record)
{
    archive = -1;
    record = -1;

    if (string.IsNullOrWhiteSpace(raw))
        return false;

    raw = raw.Trim();

    if (!raw.Contains("."))
        return false;

    return TryParseFlatArchiveRecord(raw, out archive, out record);
}

private static string MakeFlatArchiveRecordKey(int archive, int record)
{
    return archive.ToString(CultureInfo.InvariantCulture) + "." +
           record.ToString(CultureInfo.InvariantCulture);
}

private static bool IsExcludedFlatCategoryFieldName(string fieldName)
{
    return string.Equals(fieldName, "flatGroups", StringComparison.OrdinalIgnoreCase);
}
private static string BuildCategoryExportBaseNameForFlat(string dictionaryLabel, int archive, int record)
{
    return BuildCategoryExportBaseNameForFlat(dictionaryLabel, archive, record, null);
}

private static string BuildCategoryExportBaseNameForFlat(string dictionaryLabel, int archive, int record, string categoryFieldNameOrNull)
{
    string cleanedLabel = StripBracketCountSuffix(dictionaryLabel);
    string baseName = ToReadableExportToken(cleanedLabel, categoryFieldNameOrNull);

    if (string.IsNullOrEmpty(baseName))
        baseName = "Flat";

    return baseName +
           "_id" +
           archive.ToString(CultureInfo.InvariantCulture) +
           "_" +
           record.ToString(CultureInfo.InvariantCulture) +
           "_df";
}
private string BuildResolvedExportBaseNameForFlat(int archive, int record)
{
    return BuildResolvedExportBaseNameForFlat(null, archive, record);
}

private string BuildResolvedExportBaseNameForFlat(string preferredFieldName, int archive, int record)
{
    string resolvedFieldName;
    string label;
    if (TryGetResolvedDisplayNameForFlat(preferredFieldName, archive, record, out resolvedFieldName, out label) &&
        !string.IsNullOrWhiteSpace(label))
    {
        return BuildCategoryExportBaseNameForFlat(label, archive, record, resolvedFieldName);
    }

    return BuildDefaultExportBaseNameForFlat(archive, record);
}
private bool TryGetResolvedDisplayNameForFlat(
    string preferredFieldName,
    int archive,
    int record,
    out string resolvedFieldName,
    out string displayName)
{
    resolvedFieldName = null;
    displayName = null;

    // 1. Try the preferred field directly (for category batch exports)
    if (!string.IsNullOrWhiteSpace(preferredFieldName))
    {
        Type type = GetWorldDataEditorObjectDataType();
        if (type != null)
        {
            FieldInfo field = type.GetField(preferredFieldName, BindingFlags.Public | BindingFlags.Static);
            if (field != null)
            {
                string candidateDisplayName;
                if (TryGetFlatDisplayNameForArchiveRecord(field, archive, record, out candidateDisplayName) &&
                    !string.IsNullOrWhiteSpace(candidateDisplayName))
                {
                    resolvedFieldName = preferredFieldName;
                    displayName = candidateDisplayName;
                    return true;
                }
            }
        }
    }

    // 2. Try flatGroups — the primary name source for all flats
    string groupName;
    string[] groupIds;
    int groupIndex;
    if (TryGetFlatGroupEntryForArchiveRecord(archive, record, out groupName, out groupIds, out groupIndex) &&
        !string.IsNullOrWhiteSpace(groupName))
    {
        resolvedFieldName = preferredFieldName ?? "flatGroups";
        displayName = groupName;
        return true;
    }

    // 3. Fall back to scanning all flat-capable fields
    return TryGetAnyFlatDisplayNameForArchiveRecord(
        archive,
        record,
        out resolvedFieldName,
        out displayName);
}

private bool TryGetAnyFlatDisplayNameForArchiveRecord(
    int archive,
    int record,
    out string categoryFieldName,
    out string displayName)
{
    categoryFieldName = null;
    displayName = null;

    Type type = GetWorldDataEditorObjectDataType();
    if (type == null)
        return false;

    FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
    if (fields == null || fields.Length == 0)
        return false;

    for (int i = 0; i < fields.Length; i++)
    {
        if (IsExcludedFlatCategoryFieldName(fields[i].Name))
            continue;

        // Skip fields that don't contain flat archive.record entries at all
        if (!DoesFieldContainAnyFlatArchiveRecords(fields[i]))
            continue;

        string candidateDisplayName;
        if (TryGetFlatDisplayNameForArchiveRecord(fields[i], archive, record, out candidateDisplayName) &&
            !string.IsNullOrWhiteSpace(candidateDisplayName))
        {
            categoryFieldName = fields[i].Name;
            displayName = candidateDisplayName;
            return true;
        }
    }

    return false;
}




private bool TryGetFlatDisplayNameForArchiveRecord(FieldInfo field, int archive, int record, out string displayName)
{
    displayName = null;

    if (field == null)
        return false;

    object rawValue = field.GetValue(null);
    if (rawValue == null)
        return false;

    string target = MakeFlatArchiveRecordKey(archive, record);

    Dictionary<string, string> stringDict = rawValue as Dictionary<string, string>;
    if (stringDict != null)
    {
        foreach (KeyValuePair<string, string> kvp in stringDict)
        {
            int entryArchive;
            int entryRecord;

            if (TryParseCategoryEntryToFlatArchiveRecord(kvp.Key, out entryArchive, out entryRecord) &&
                entryArchive == archive &&
                entryRecord == record &&
                !string.IsNullOrWhiteSpace(kvp.Value))
            {
                displayName = kvp.Value;
                return true;
            }

            if (TryParseCategoryEntryToFlatArchiveRecord(kvp.Value, out entryArchive, out entryRecord) &&
                entryArchive == archive &&
                entryRecord == record &&
                !string.IsNullOrWhiteSpace(kvp.Key))
            {
                displayName = kvp.Key;
                return true;
            }
        }

        return false;
    }

    Dictionary<string, string[]> stringArrayDict = rawValue as Dictionary<string, string[]>;
    if (stringArrayDict != null)
    {
        foreach (KeyValuePair<string, string[]> kvp in stringArrayDict)
        {
            if (kvp.Value == null)
                continue;

            for (int i = 0; i < kvp.Value.Length; i++)
            {
                int entryArchive;
                int entryRecord;
                if (TryParseCategoryEntryToFlatArchiveRecord(kvp.Value[i], out entryArchive, out entryRecord) &&
                    entryArchive == archive &&
                    entryRecord == record &&
                    !string.IsNullOrWhiteSpace(kvp.Key))
                {
                    displayName = kvp.Key;
                    return true;
                }
            }
        }

        return false;
    }

    if (rawValue is System.Collections.IDictionary genericDict)
    {
        foreach (System.Collections.DictionaryEntry entry in genericDict)
        {
            string keyString = entry.Key != null ? entry.Key.ToString() : null;
            string valueString = entry.Value as string;
            string[] valueArray = entry.Value as string[];

            int entryArchive;
            int entryRecord;

            if (!string.IsNullOrWhiteSpace(keyString) &&
                TryParseCategoryEntryToFlatArchiveRecord(keyString, out entryArchive, out entryRecord) &&
                entryArchive == archive &&
                entryRecord == record &&
                !string.IsNullOrWhiteSpace(valueString))
            {
                displayName = valueString;
                return true;
            }

            if (!string.IsNullOrWhiteSpace(valueString) &&
                TryParseCategoryEntryToFlatArchiveRecord(valueString, out entryArchive, out entryRecord) &&
                entryArchive == archive &&
                entryRecord == record &&
                !string.IsNullOrWhiteSpace(keyString))
            {
                displayName = keyString;
                return true;
            }

            if (valueArray != null && !string.IsNullOrWhiteSpace(keyString))
            {
                for (int i = 0; i < valueArray.Length; i++)
                {
                    if (TryParseCategoryEntryToFlatArchiveRecord(valueArray[i], out entryArchive, out entryRecord) &&
                        entryArchive == archive &&
                        entryRecord == record)
                    {
                        displayName = keyString;
                        return true;
                    }
                }
            }
        }
    }

    return false;
}



private bool TryGetFlatDisplayNameFromRawValue(object rawValue, int archive, int record, out string displayName)
{
    displayName = null;

    System.Collections.IDictionary dict = rawValue as System.Collections.IDictionary;
    if (dict == null)
        return false;

    foreach (System.Collections.DictionaryEntry entry in dict)
    {
        if (TryGetFlatDisplayNameFromDictionaryEntry(entry, archive, record, out displayName))
            return true;
    }

    return false;
}

private bool TryGetFlatDisplayNameFromDictionaryEntry(
    System.Collections.DictionaryEntry entry,
    int archive,
    int record,
    out string displayName)
{
    displayName = null;

    string keyString = entry.Key != null ? entry.Key.ToString() : null;
    string valueString = entry.Value as string;

    int entryArchive;
    int entryRecord;

    if (!string.IsNullOrWhiteSpace(keyString) &&
        TryParseCategoryEntryToFlatArchiveRecord(keyString, out entryArchive, out entryRecord) &&
        entryArchive == archive &&
        entryRecord == record &&
        !string.IsNullOrWhiteSpace(valueString))
    {
        displayName = valueString;
        return true;
    }

    if (!string.IsNullOrWhiteSpace(valueString) &&
        TryParseCategoryEntryToFlatArchiveRecord(valueString, out entryArchive, out entryRecord) &&
        entryArchive == archive &&
        entryRecord == record &&
        !string.IsNullOrWhiteSpace(keyString))
    {
        displayName = keyString;
        return true;
    }

    if (!string.IsNullOrWhiteSpace(keyString) &&
        ValueContainsFlatArchiveRecord(entry.Value, archive, record))
    {
        displayName = keyString;
        return true;
    }

    return false;
}

private bool ValueContainsFlatArchiveRecord(object value, int archive, int record)
{
    if (value == null)
        return false;

    string valueString = value as string;
    if (!string.IsNullOrWhiteSpace(valueString))
    {
        int entryArchive;
        int entryRecord;
        return TryParseCategoryEntryToFlatArchiveRecord(valueString, out entryArchive, out entryRecord) &&
               entryArchive == archive &&
               entryRecord == record;
    }

    System.Collections.IEnumerable enumerable = value as System.Collections.IEnumerable;
    if (enumerable == null || value is string)
        return false;

    foreach (object item in enumerable)
    {
        string itemString = item != null ? item.ToString() : null;
        if (string.IsNullOrWhiteSpace(itemString))
            continue;

        int entryArchive;
        int entryRecord;
        if (TryParseCategoryEntryToFlatArchiveRecord(itemString, out entryArchive, out entryRecord) &&
            entryArchive == archive &&
            entryRecord == record)
        {
            return true;
        }
    }

    return false;
}

private void AddFlatRequestsFromRawValue(object rawValue, List<ExportRequest> requests, HashSet<string> seen)
{
    if (rawValue == null)
        return;

    System.Collections.IDictionary dict = rawValue as System.Collections.IDictionary;
    if (dict == null)
        return;

    foreach (System.Collections.DictionaryEntry entry in dict)
    {
        AddFlatRequestIfPresent(entry.Key != null ? entry.Key.ToString() : null, requests, seen);
        AddFlatRequestIfPresent(entry.Value as string, requests, seen);

        System.Collections.IEnumerable enumerable = entry.Value as System.Collections.IEnumerable;
        if (enumerable == null || entry.Value is string)
            continue;

        foreach (object item in enumerable)
            AddFlatRequestIfPresent(item != null ? item.ToString() : null, requests, seen);
    }
}

private string[] CollectAvailableFlatCategoryBatchSources()
{
    Type type = GetWorldDataEditorObjectDataType();
    if (type == null)
        return new string[0];

    List<string> result = new List<string>();
    FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);

    for (int i = 0; i < fields.Length; i++)
    {
        if (IsExcludedFlatCategoryFieldName(fields[i].Name))
            continue;

        if (DoesFieldContainAnyFlatArchiveRecords(fields[i]))
            result.Add(fields[i].Name);
    }

    result.Sort(StringComparer.OrdinalIgnoreCase);
    return result.ToArray();
}

private bool DoesFieldContainAnyFlatArchiveRecords(FieldInfo field)
{
    if (field == null)
        return false;

    if (IsExcludedFlatCategoryFieldName(field.Name))
        return false;

    object rawValue = field.GetValue(null);
    if (rawValue == null)
        return false;

    Dictionary<string, string> stringDict = rawValue as Dictionary<string, string>;
    if (stringDict != null)
    {
        foreach (KeyValuePair<string, string> kvp in stringDict)
        {
            int archive;
            int record;

            if (TryParseCategoryEntryToFlatArchiveRecord(kvp.Key, out archive, out record))
                return true;

            if (TryParseCategoryEntryToFlatArchiveRecord(kvp.Value, out archive, out record))
                return true;
        }

        return false;
    }

    Dictionary<string, string[]> stringArrayDict = rawValue as Dictionary<string, string[]>;
    if (stringArrayDict != null)
    {
        foreach (KeyValuePair<string, string[]> kvp in stringArrayDict)
        {
            if (kvp.Value == null)
                continue;

            for (int i = 0; i < kvp.Value.Length; i++)
            {
                int archive;
                int record;
                if (TryParseCategoryEntryToFlatArchiveRecord(kvp.Value[i], out archive, out record))
                    return true;
            }
        }

        return false;
    }

    if (rawValue is System.Collections.IDictionary genericDict)
    {
        foreach (System.Collections.DictionaryEntry entry in genericDict)
        {
            int archive;
            int record;

            string keyString = entry.Key != null ? entry.Key.ToString() : null;
            string valueString = entry.Value as string;
            string[] valueArray = entry.Value as string[];

            if (TryParseCategoryEntryToFlatArchiveRecord(keyString, out archive, out record))
                return true;

            if (TryParseCategoryEntryToFlatArchiveRecord(valueString, out archive, out record))
                return true;

            if (valueArray != null)
            {
                for (int i = 0; i < valueArray.Length; i++)
                {
                    if (TryParseCategoryEntryToFlatArchiveRecord(valueArray[i], out archive, out record))
                        return true;
                }
            }
        }
    }

    return false;
}

private bool TryGetFlatRequestsFromCategorySource(string fieldName, out List<ExportRequest> requests, out string error)
{
    requests = new List<ExportRequest>();
    error = null;

    if (string.IsNullOrWhiteSpace(fieldName))
    {
        error = "Field name was empty.";
        return false;
    }

    if (IsExcludedFlatCategoryFieldName(fieldName))
    {
        error = "Field '" + fieldName + "' is excluded from flat category batch export.";
        return false;
    }

    Type type = GetWorldDataEditorObjectDataType();
    if (type == null)
    {
        error = "Could not find WorldDataEditorObjectData type.";
        return false;
    }

    FieldInfo field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.Static);
    if (field == null)
    {
        error = "Could not find field '" + fieldName + "'.";
        return false;
    }

    object rawValue = field.GetValue(null);
    if (rawValue == null)
    {
        error = "Field '" + fieldName + "' was null.";
        return false;
    }

    if (!(rawValue is System.Collections.IDictionary))
    {
        error = "Field '" + fieldName + "' was not a supported dictionary type. Actual type: " + rawValue.GetType().FullName;
        return false;
    }

    HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    AddFlatRequestsFromRawValue(rawValue, requests, seen);

    if (requests.Count == 0)
    {
        error = "Field '" + fieldName + "' did not contain any exportable flat archive.record entries.";
        return false;
    }

    return true;
}
private void AddFlatRequestIfPresent(string raw, List<ExportRequest> requests, HashSet<string> seen)
{
    int archive;
    int record;
    if (!TryParseCategoryEntryToFlatArchiveRecord(raw, out archive, out record))
        return;

    string key = MakeFlatArchiveRecordKey(archive, record);
    if (!seen.Add(key))
        return;

    requests.Add(new ExportRequest
    {
        IsFlat = true,
        FlatArchive = archive,
        FlatRecord = record,
    });
}

private int GetFlatCategoryBaseCount(string fieldName)
{
    List<ExportRequest> requests;
    string error;
    if (TryGetFlatRequestsFromCategorySource(fieldName, out requests, out error) && requests != null)
        return requests.Count;

    return 0;
}

private int GetFlatCategoryGroupedCount(string fieldName)
{
    List<ExportRequest> requests;
    string error;
    if (!TryGetFlatRequestsFromCategorySource(fieldName, out requests, out error) || requests == null)
        return 0;

    return ExpandFlatRequestsByGroupedFamiliesForced(requests).Count;
}

private static string BuildFlatBatchProgressLabel(
    string verb,
    string fieldName,
    int archive,
    int record,
    int indexOneBased,
    int totalCount)
{
    return string.Format(
        CultureInfo.InvariantCulture,
        "{0} {1} flat {2}.{3} ({4}/{5})",
        verb,
        fieldName,
        archive,
        record,
        indexOneBased,
        totalCount);
}

private string GetFlatCategoryDurationKey(string fieldName)
{
    return "FlatCategory|" + fieldName + (exportGroupedFamilyMembers ? "|Grouped" : "|Base");
}
private static string MakeFlatCategoryDurationPrefKey(string fieldName, bool grouped)
{
    return PrefFlatCategoryLastDurationSecondsPrefix + fieldName + (grouped ? "|Grouped" : "|Base");
}

private string GetFlatLastDurationText(string fieldName)
{
    double seconds;
    string durationKey = GetFlatCategoryDurationKey(fieldName);

    if (!categoryLastDurationSeconds.TryGetValue(durationKey, out seconds) || seconds < 0d)
        return "(never)";

    return FormatDurationSeconds(seconds);
}

private void SaveFlatCategoryDuration(string fieldName, double seconds)
{
    string durationKey = GetFlatCategoryDurationKey(fieldName);
    categoryLastDurationSeconds[durationKey] = seconds;
    EditorPrefs.SetFloat(
        MakeFlatCategoryDurationPrefKey(fieldName, exportGroupedFamilyMembers),
        (float)seconds);
}

private double GetTotalEstimatedFlatCategoryExportSeconds()
{
    double total = 0d;
    string[] flatSources = GetAvailableFlatCategoryBatchSources();

    for (int i = 0; i < flatSources.Length; i++)
    {
        string durationKey = "FlatCategory|" + flatSources[i] + (exportGroupedFamilyMembers ? "|Grouped" : "|Base");
        double seconds;
        if (categoryLastDurationSeconds.TryGetValue(durationKey, out seconds) && seconds > 0d)
            total += seconds;
    }

    return total;
}
private bool TryGetCategoryDisplayNameForModelId(string fieldName, uint modelId, out string displayName)
{
    displayName = null;

    Type type = GetWorldDataEditorObjectDataType();
    if (type == null)
        return false;

    FieldInfo field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.Static);
    if (field == null)
        return false;

    object rawValue = field.GetValue(null);
    if (rawValue == null)
        return false;

    string target = modelId.ToString(CultureInfo.InvariantCulture);

    Dictionary<string, string> stringDict = rawValue as Dictionary<string, string>;
    if (stringDict != null)
    {
        if (stringDict.TryGetValue(target, out displayName) &&
            !string.IsNullOrWhiteSpace(displayName))
        {
            return true;
        }

        foreach (KeyValuePair<string, string> kvp in stringDict)
        {
            if (string.Equals(kvp.Key, target, StringComparison.Ordinal) &&
                !string.IsNullOrWhiteSpace(kvp.Value))
            {
                displayName = kvp.Value;
                return true;
            }

            if (string.Equals(kvp.Value, target, StringComparison.Ordinal) &&
                !string.IsNullOrWhiteSpace(kvp.Key))
            {
                displayName = kvp.Key;
                return true;
            }
        }

        return false;
    }

    Dictionary<string, string[]> stringArrayDict = rawValue as Dictionary<string, string[]>;
    if (stringArrayDict != null)
    {
        foreach (KeyValuePair<string, string[]> kvp in stringArrayDict)
        {
            if (kvp.Value == null)
                continue;

            for (int i = 0; i < kvp.Value.Length; i++)
            {
                if (string.Equals(kvp.Value[i], target, StringComparison.Ordinal) &&
                    !string.IsNullOrWhiteSpace(kvp.Key))
                {
                    displayName = kvp.Key;
                    return true;
                }
            }
        }

        return false;
    }

    if (rawValue is System.Collections.IDictionary genericDict)
    {
        foreach (System.Collections.DictionaryEntry entry in genericDict)
        {
            string keyString = entry.Key != null
                ? entry.Key.ToString()
                : null;

            string valueString = entry.Value as string;
            if (!string.IsNullOrWhiteSpace(keyString) &&
                string.Equals(keyString, target, StringComparison.Ordinal) &&
                !string.IsNullOrWhiteSpace(valueString))
            {
                displayName = valueString;
                return true;
            }

            if (!string.IsNullOrWhiteSpace(keyString) &&
                !string.IsNullOrWhiteSpace(valueString) &&
                string.Equals(valueString, target, StringComparison.Ordinal))
            {
                displayName = keyString;
                return true;
            }

            string[] arr = entry.Value as string[];
            if (arr != null && !string.IsNullOrWhiteSpace(keyString))
            {
                for (int i = 0; i < arr.Length; i++)
                {
                    if (string.Equals(arr[i], target, StringComparison.Ordinal))
                    {
                        displayName = keyString;
                        return true;
                    }
                }
            }
        }
    }

    return false;
}
private class BatchPackCandidate
{
    public int Width;
    public int Height;
    public Dictionary<string, BatchTexturePlacement> Placements =
        new Dictionary<string, BatchTexturePlacement>(StringComparer.OrdinalIgnoreCase);
    public List<string> PackedKeys = new List<string>();
}

private static BatchPackCandidate TryPackCandidate(
    List<KeyValuePair<string, Texture2D>> textures,
    int atlasWidth,
    int atlasHeight)
{
    BatchPackCandidate candidate = new BatchPackCandidate();
    candidate.Width = atlasWidth;
    candidate.Height = atlasHeight;

    int cursorX = 0;
    int cursorYTop = 0;
    int rowHeight = 0;

    for (int i = 0; i < textures.Count; i++)
    {
        string key = textures[i].Key;
        Texture2D tex = textures[i].Value;

        if (tex == null)
            continue;

        if (tex.width > atlasWidth || tex.height > atlasHeight)
            continue;

        if (cursorX + tex.width > atlasWidth)
        {
            cursorX = 0;
            cursorYTop += rowHeight;
            rowHeight = 0;
        }

        if (cursorYTop + tex.height > atlasHeight)
            continue;

        int pixelYBottom = atlasHeight - cursorYTop - tex.height;

        BatchTexturePlacement placement = new BatchTexturePlacement();
        placement.PageIndex = -1;
        placement.PixelX = cursorX;
        placement.PixelY = pixelYBottom;
        placement.Width = tex.width;
        placement.Height = tex.height;
        placement.Rect = new Rect(
            (float)cursorX / atlasWidth,
            (float)pixelYBottom / atlasHeight,
            (float)tex.width / atlasWidth,
            (float)tex.height / atlasHeight);

        candidate.Placements.Add(key, placement);
        candidate.PackedKeys.Add(key);

        cursorX += tex.width;
        if (tex.height > rowHeight)
            rowHeight = tex.height;
    }

    return candidate;
}

private static BatchPackCandidate ChooseBestPackCandidate(
    List<KeyValuePair<string, Texture2D>> remaining,
    int maxAtlasSize)
{
    BatchPackCandidate best = null;

    for (int width = 256; width <= maxAtlasSize; width <<= 1)
    {
        for (int height = 256; height <= maxAtlasSize; height <<= 1)
        {
            BatchPackCandidate candidate = TryPackCandidate(remaining, width, height);
            if (candidate.PackedKeys.Count == 0)
                continue;

            if (best == null)
            {
                best = candidate;
                continue;
            }

            if (candidate.PackedKeys.Count > best.PackedKeys.Count)
            {
                best = candidate;
                continue;
            }

            if (candidate.PackedKeys.Count < best.PackedKeys.Count)
                continue;

            int candidateArea = candidate.Width * candidate.Height;
            int bestArea = best.Width * best.Height;

            if (candidateArea < bestArea)
            {
                best = candidate;
                continue;
            }

            if (candidateArea > bestArea)
                continue;

            int candidateSquareness = Mathf.Abs(candidate.Width - candidate.Height);
            int bestSquareness = Mathf.Abs(best.Width - best.Height);

            if (candidateSquareness < bestSquareness)
                best = candidate;
        }
    }

    return best;
}
private static string BuildBatchBaseName(List<uint> ids)
{
    if (ids == null || ids.Count == 0)
        return "DFU_Model_Batch";

    uint minId = ids[0];
    uint maxId = ids[0];

    for (int i = 1; i < ids.Count; i++)
    {
        if (ids[i] < minId) minId = ids[i];
        if (ids[i] > maxId) maxId = ids[i];
    }

    if (minId == maxId)
        return "DFU_Model_id" + minId.ToString(CultureInfo.InvariantCulture);

    return "DFU_Model_id" +
       minId.ToString(CultureInfo.InvariantCulture) +
       "_to_id" +
       maxId.ToString(CultureInfo.InvariantCulture);
}
private static string BuildSharedBatchAssetBaseName(string rawBatchName)
{
    if (string.IsNullOrWhiteSpace(rawBatchName))
        return "Batch";

    if (string.Equals(rawBatchName, "ships", StringComparison.OrdinalIgnoreCase))
        return "Ships";

    string working = rawBatchName.Trim();

    if (working.StartsWith("models_", StringComparison.OrdinalIgnoreCase))
        working = working.Substring("models_".Length);

    working = ToReadableExportToken(working);

    if (string.IsNullOrEmpty(working))
        working = "Batch";

    return working;
}

private static string BuildSharedFlatBatchAssetBaseName(string rawBatchName)
{
    if (string.IsNullOrWhiteSpace(rawBatchName))
        return "FlatBatch";

    string working = rawBatchName.Trim();

    if (working.StartsWith("billboards_", StringComparison.OrdinalIgnoreCase))
        working = working.Substring("billboards_".Length);
    else if (working.StartsWith("billboard_", StringComparison.OrdinalIgnoreCase))
        working = working.Substring("billboard_".Length);
    else if (working.StartsWith("flats_", StringComparison.OrdinalIgnoreCase))
        working = working.Substring("flats_".Length);

    working = ToReadableExportToken(working);

    if (string.IsNullOrEmpty(working))
        working = "FlatBatch";

    return working;
}

private void DeleteExportedFilesForGroup(string fieldName)
{
    if (string.IsNullOrWhiteSpace(fieldName))
        return;

    string batchBaseName = BuildSharedBatchAssetBaseName(fieldName);
    string categoryFolderAssetPath = BuildCategoryFbxFolderAssetRelativePath(fieldName);

    int deletedCount = 0;

    try
    {
        string atlasFolderAbs = GetAbsolutePathFromAssetPath(AtlasFolderAssetRelative);
        string materialFolderAbs = GetAbsolutePathFromAssetPath(MaterialFolderAssetRelative);
        string categoryFolderAbs = GetAbsolutePathFromAssetPath(categoryFolderAssetPath);

        if (!string.IsNullOrEmpty(atlasFolderAbs) && Directory.Exists(atlasFolderAbs))
        {
            string[] atlasFiles = Directory.GetFiles(atlasFolderAbs, batchBaseName + "*", SearchOption.TopDirectoryOnly);
            for (int i = 0; i < atlasFiles.Length; i++)
            {
                FileUtil.DeleteFileOrDirectory(atlasFiles[i]);
                FileUtil.DeleteFileOrDirectory(atlasFiles[i] + ".meta");
                deletedCount++;
            }
        }

        if (!string.IsNullOrEmpty(materialFolderAbs) && Directory.Exists(materialFolderAbs))
        {
            string[] matFiles = Directory.GetFiles(materialFolderAbs, batchBaseName + "*", SearchOption.TopDirectoryOnly);
            for (int i = 0; i < matFiles.Length; i++)
            {
                FileUtil.DeleteFileOrDirectory(matFiles[i]);
                FileUtil.DeleteFileOrDirectory(matFiles[i] + ".meta");
                deletedCount++;
            }
        }

        if (!string.IsNullOrEmpty(categoryFolderAbs) && Directory.Exists(categoryFolderAbs))
        {
            string[] fbxFiles = Directory.GetFiles(categoryFolderAbs, "*.fbx", SearchOption.TopDirectoryOnly);
            for (int i = 0; i < fbxFiles.Length; i++)
            {
                FileUtil.DeleteFileOrDirectory(fbxFiles[i]);
                FileUtil.DeleteFileOrDirectory(fbxFiles[i] + ".meta");
                deletedCount++;
            }
        }

        AssetDatabase.Refresh();

        Debug.Log(string.Format(
            CultureInfo.InvariantCulture,
            "[NexusDev][DFU Export] Deleted {0} exported file(s) for model group '{1}'.",
            deletedCount,
            fieldName));
    }
    catch (Exception ex)
    {
        Debug.LogError("[NexusDev][DFU Export] Failed deleting exported files for model group '" + fieldName + "'. Exception: " + ex);
    }
}

private void DeleteExportedFilesForFlatGroup(string fieldName)
{
    if (string.IsNullOrWhiteSpace(fieldName))
        return;

        string batchBaseName = BuildSharedFlatBatchAssetBaseName(fieldName);
    string categoryFolderAssetPath = BuildFlatCategoryFbxFolderAssetRelativePath(fieldName);

    int deletedCount = 0;

    try
    {
        string atlasFolderAbs = GetAbsolutePathFromAssetPath(AtlasFolderAssetRelative);
        string materialFolderAbs = GetAbsolutePathFromAssetPath(MaterialFolderAssetRelative);
        string categoryFolderAbs = GetAbsolutePathFromAssetPath(categoryFolderAssetPath);

        if (!string.IsNullOrEmpty(atlasFolderAbs) && Directory.Exists(atlasFolderAbs))
        {
            string[] atlasFiles = Directory.GetFiles(atlasFolderAbs, batchBaseName + "*", SearchOption.TopDirectoryOnly);
            for (int i = 0; i < atlasFiles.Length; i++)
            {
                FileUtil.DeleteFileOrDirectory(atlasFiles[i]);
                FileUtil.DeleteFileOrDirectory(atlasFiles[i] + ".meta");
                deletedCount++;
            }
        }

        if (!string.IsNullOrEmpty(materialFolderAbs) && Directory.Exists(materialFolderAbs))
        {
            string[] matFiles = Directory.GetFiles(materialFolderAbs, batchBaseName + "*", SearchOption.TopDirectoryOnly);
            for (int i = 0; i < matFiles.Length; i++)
            {
                FileUtil.DeleteFileOrDirectory(matFiles[i]);
                FileUtil.DeleteFileOrDirectory(matFiles[i] + ".meta");
                deletedCount++;
            }
        }

        if (!string.IsNullOrEmpty(categoryFolderAbs) && Directory.Exists(categoryFolderAbs))
        {
            string[] fbxFiles = Directory.GetFiles(categoryFolderAbs, "*.fbx", SearchOption.TopDirectoryOnly);
            for (int i = 0; i < fbxFiles.Length; i++)
            {
                FileUtil.DeleteFileOrDirectory(fbxFiles[i]);
                FileUtil.DeleteFileOrDirectory(fbxFiles[i] + ".meta");
                deletedCount++;
            }
        }

        AssetDatabase.Refresh();

        Debug.Log(string.Format(
            CultureInfo.InvariantCulture,
            "[NexusDev][DFU Export] Deleted {0} exported file(s) for flat group '{1}'.",
            deletedCount,
            fieldName));
    }
    catch (Exception ex)
    {
        Debug.LogError("[NexusDev][DFU Export] Failed deleting exported files for flat group '" + fieldName + "'. Exception: " + ex);
    }
}

private void DeleteAllExportedMobileSprites()
{
    int deletedCount = 0;

    try
    {
        deletedCount += DeleteAllFilesInAssetFolder(MobileSpritesFolderAssetRelative);
        AssetDatabase.Refresh();

        EnsureFolderExists(MobileSpritesFolderAssetRelative);
        EnsureFolderExists(MobileTownNpcFolderAssetRelative);
        EnsureFolderExists(MobileEnemyFolderAssetRelative);
        AssetDatabase.Refresh();

        Debug.Log(string.Format(
            CultureInfo.InvariantCulture,
            "[NexusDev][DFU Export] Deleted {0} previously-exported MobileSprites asset file(s).",
            deletedCount));
    }
    catch (Exception ex)
    {
        Debug.LogError("[NexusDev][DFU Export] Failed deleting MobileSprites exports. Exception: " + ex);
    }
}
private void DeleteAllExportedAssets()
{
    int deletedCount = 0;

    try
    {
        deletedCount += DeleteAllFilesInAssetFolder(AtlasFolderAssetRelative);
        deletedCount += DeleteAllFilesInAssetFolder(MaterialFolderAssetRelative);
        deletedCount += DeleteAllFilesInAssetFolder(FbxFolderAssetRelative);
        deletedCount += DeleteAssetFolderIfExists(TempTextureFolderAssetRelative);

        AssetDatabase.Refresh();

        Debug.Log(string.Format(
            CultureInfo.InvariantCulture,
            "[NexusDev][DFU Export] Deleted {0} exported asset file(s)/folder entries from all export locations.",
            deletedCount));
    }
    catch (Exception ex)
    {
        Debug.LogError("[NexusDev][DFU Export] Failed deleting all exported assets. Exception: " + ex);
    }
}

private static int DeleteAllFilesInAssetFolder(string assetFolderPath)
{
    int deletedCount = 0;

    string absFolder = GetAbsolutePathFromAssetPath(assetFolderPath);
    if (string.IsNullOrEmpty(absFolder) || !Directory.Exists(absFolder))
        return 0;

    string[] files = Directory.GetFiles(absFolder, "*", SearchOption.AllDirectories);
    for (int i = 0; i < files.Length; i++)
    {
        string file = files[i];
        if (file.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
            continue;

        FileUtil.DeleteFileOrDirectory(file);
        FileUtil.DeleteFileOrDirectory(file + ".meta");
        deletedCount++;
    }

    string[] subdirs = Directory.GetDirectories(absFolder, "*", SearchOption.AllDirectories);
    Array.Sort(subdirs, (a, b) => b.Length.CompareTo(a.Length));

    for (int i = 0; i < subdirs.Length; i++)
    {
        if (Directory.Exists(subdirs[i]))
        {
            FileUtil.DeleteFileOrDirectory(subdirs[i]);
            FileUtil.DeleteFileOrDirectory(subdirs[i] + ".meta");
        }
    }

    return deletedCount;
}

private static int DeleteAssetFolderIfExists(string assetFolderPath)
{
    string absFolder = GetAbsolutePathFromAssetPath(assetFolderPath);
    if (string.IsNullOrEmpty(absFolder) || !Directory.Exists(absFolder))
        return 0;

    FileUtil.DeleteFileOrDirectory(assetFolderPath);
    FileUtil.DeleteFileOrDirectory(assetFolderPath + ".meta");
    return 1;
}
private static int NextPowerOfTwo(int value)
{
    int result = 1;
    while (result < value)
        result <<= 1;
    return result;
}
private static void ConfigureImportedTextureForPointSampling(string assetPath)
{
    ConfigureImportedTextureForPointSampling(assetPath, 8192);
}
private static void ConfigureImportedTextureForPointSampling(string assetPath, int desiredMaxSize)
{
    TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
    if (importer == null)
        return;

    if (desiredMaxSize < 32)
        desiredMaxSize = 32;

    desiredMaxSize = Mathf.Clamp(desiredMaxSize, 32, 8192);

    importer.textureType = TextureImporterType.Default;
    importer.alphaSource = TextureImporterAlphaSource.FromInput;
    importer.alphaIsTransparency = true;
    importer.mipmapEnabled = false;
    importer.filterMode = FilterMode.Point;
    importer.wrapMode = TextureWrapMode.Clamp;
    importer.textureCompression = TextureImporterCompression.Uncompressed;
    importer.isReadable = false;
    importer.maxTextureSize = desiredMaxSize;
    importer.npotScale = TextureImporterNPOTScale.None;
    importer.SaveAndReimport();
}

private static void ApplyStandardCutoutMaterialSettings(Material mat)
{
    if (mat == null)
        return;

    if (mat.HasProperty("_Glossiness"))
        mat.SetFloat("_Glossiness", 0f);

    if (mat.HasProperty("_Mode"))
        mat.SetFloat("_Mode", 1f);

    if (mat.HasProperty("_Cutoff"))
        mat.SetFloat("_Cutoff", 0.5f);

    if (mat.HasProperty("_SrcBlend"))
        mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);

    if (mat.HasProperty("_DstBlend"))
        mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.Zero);

    if (mat.HasProperty("_ZWrite"))
        mat.SetFloat("_ZWrite", 1f);

    mat.DisableKeyword("_ALPHABLEND_ON");
    mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
    mat.EnableKeyword("_ALPHATEST_ON");
    mat.SetOverrideTag("RenderType", "TransparentCutout");
    mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
}
private static object CreateBinaryExportOptions()
{
    object options = Activator.CreateInstance(cachedExportModelSettingsSerializeType);
    if (options == null)
        return null;

    cachedSetExportFormatMethod.Invoke(options, new object[] { cachedBinaryExportFormatValue });
    return options;
}
private static bool ExportFbxViaReflection(
    string exportPathAssetRelative,
    UnityEngine.Object obj,
    out string exportedAssetPath,
    out string error)
{
    exportedAssetPath = null;
    error = null;

    if (!HasFbxExporter(out error))
        return false;

    object exportOptions = CreateBinaryExportOptions();
    if (exportOptions == null)
    {
        error = "Could not create FBX export options instance.";
        return false;
    }

    object result = cachedExportMethod.Invoke(null, new object[] { exportPathAssetRelative, obj, exportOptions });
    exportedAssetPath = result as string;

    if (string.IsNullOrEmpty(exportedAssetPath))
    {
        error = "FBX exporter returned an empty path.";
        return false;
    }

    return true;
}

private static bool RemapImportedFbxMaterials(
    string fbxAssetPath,
    Material[] desiredMaterials,
    out string error)
{
    error = null;

    if (string.IsNullOrWhiteSpace(fbxAssetPath))
    {
        error = "FBX asset path was empty.";
        return false;
    }

    if (desiredMaterials == null || desiredMaterials.Length == 0)
    {
        error = "Desired materials array was empty.";
        return false;
    }

    ModelImporter importer = AssetImporter.GetAtPath(fbxAssetPath) as ModelImporter;
    if (importer == null)
    {
        error = "Could not get ModelImporter for: " + fbxAssetPath;
        return false;
    }

    GameObject importedRoot = AssetDatabase.LoadAssetAtPath<GameObject>(fbxAssetPath);
    if (importedRoot == null)
    {
        error = "Could not load imported FBX root object: " + fbxAssetPath;
        return false;
    }

    Renderer renderer = importedRoot.GetComponentInChildren<Renderer>();
    if (renderer == null)
    {
        error = "Imported FBX had no renderer to remap materials from.";
        return false;
    }

    Material[] importedMaterials = renderer.sharedMaterials;
    if (importedMaterials == null || importedMaterials.Length == 0)
    {
        error = "Imported FBX renderer had no materials.";
        return false;
    }

    int remapCount = Mathf.Min(importedMaterials.Length, desiredMaterials.Length);
    if (remapCount <= 0)
    {
        error = "No overlapping material slots to remap.";
        return false;
    }

    bool changed = false;

    for (int i = 0; i < remapCount; i++)
    {
        Material importedMaterial = importedMaterials[i];
        Material desiredMaterial = desiredMaterials[i];

        if (importedMaterial == null)
            continue;

        if (desiredMaterial == null)
        {
            error = "Desired material at slot " + i.ToString(CultureInfo.InvariantCulture) + " was null.";
            return false;
        }

        importer.AddRemap(
            new AssetImporter.SourceAssetIdentifier(typeof(Material), importedMaterial.name),
            desiredMaterial);

        changed = true;
    }

    if (!changed)
    {
        error = "No valid source materials were found to remap.";
        return false;
    }

    importer.SaveAndReimport();
    return true;
}


private static List<ExportRequest> ParseExportRequestsAndRanges(string text)
{
    List<ExportRequest> result = new List<ExportRequest>();
    HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    if (string.IsNullOrWhiteSpace(text))
        return result;

    string[] lines = text.Replace("\r", "").Split('\n');

    for (int i = 0; i < lines.Length; i++)
    {
        string line = StripComment(lines[i]).Trim();
        if (string.IsNullOrWhiteSpace(line))
            continue;

        string[] chunks = line.Split(',');
        for (int j = 0; j < chunks.Length; j++)
        {
            string chunk = chunks[j].Trim();
            if (string.IsNullOrWhiteSpace(chunk))
                continue;

            if (chunk.Contains("-") && !chunk.Contains("."))
            {
                ParseRangeChunkToRequests(chunk, result, seen);
                continue;
            }

            ExportRequest request;
            if (TryParseExportRequestToken(chunk, out request))
            {
                string dedupeKey = request.IsFlat
                    ? ("F:" + request.FlatArchive.ToString(CultureInfo.InvariantCulture) + "." + request.FlatRecord.ToString(CultureInfo.InvariantCulture))
                    : ("M:" + request.ModelId.ToString(CultureInfo.InvariantCulture));

                if (seen.Add(dedupeKey))
                    result.Add(request);
            }
            else
            {
                Debug.LogWarning("[NexusDev][DFU Export] Ignoring invalid export entry: '" + chunk + "'");
            }
        }
    }

    return result;
}

private static bool TryParseExportRequestToken(string chunk, out ExportRequest request)
{
    request = null;

    if (string.IsNullOrWhiteSpace(chunk))
        return false;

    int archive;
    int record;
    if (TryParseFlatArchiveRecord(chunk, out archive, out record))
    {
        request = new ExportRequest
        {
            IsFlat = true,
            FlatArchive = archive,
            FlatRecord = record,
        };
        return true;
    }

    uint modelId;
    if (uint.TryParse(chunk, NumberStyles.Integer, CultureInfo.InvariantCulture, out modelId))
    {
        request = new ExportRequest
        {
            IsFlat = false,
            ModelId = modelId,
        };
        return true;
    }

    return false;
}

private static void ParseRangeChunkToRequests(string chunk, List<ExportRequest> result, HashSet<string> seen)
{
    string[] parts = chunk.Split('-');
    if (parts.Length != 2)
    {
        Debug.LogWarning("[NexusDev][DFU Export] Ignoring invalid range entry: '" + chunk + "'");
        return;
    }

    uint start;
    uint end;

    if (!uint.TryParse(parts[0].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out start))
    {
        Debug.LogWarning("[NexusDev][DFU Export] Ignoring invalid range start: '" + chunk + "'");
        return;
    }

    if (!uint.TryParse(parts[1].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out end))
    {
        Debug.LogWarning("[NexusDev][DFU Export] Ignoring invalid range end: '" + chunk + "'");
        return;
    }

    if (end < start)
    {
        uint temp = start;
        start = end;
        end = temp;
    }

    for (uint i = start; i <= end; i++)
    {
        string dedupeKey = "M:" + i.ToString(CultureInfo.InvariantCulture);
        if (seen.Add(dedupeKey))
        {
            result.Add(new ExportRequest
            {
                IsFlat = false,
                ModelId = i,
            });
        }

        if (i == uint.MaxValue)
            break;
    }
}

        private static string StripComment(string line)
        {
            if (string.IsNullOrEmpty(line))
                return string.Empty;

            int hashIndex = line.IndexOf('#');
            if (hashIndex >= 0)
                return line.Substring(0, hashIndex);

            int slashIndex = line.IndexOf("//", StringComparison.Ordinal);
            if (slashIndex >= 0)
                return line.Substring(0, slashIndex);

            return line;
        }

        private static void EnsureExportFolders()
        {
            EnsureFolderExists(ExportFolderAssetRelative);
            EnsureFolderExists(TempTextureFolderAssetRelative);
            EnsureFolderExists(AtlasFolderAssetRelative);
            EnsureFolderExists(MaterialFolderAssetRelative);
           EnsureFolderExists(FbxFolderAssetRelative);
EnsureFolderExists(FbxModelFolderAssetRelative);
EnsureFolderExists(FbxFlatFolderAssetRelative);
        }

       private static void EnsureFolderExists(string assetRelativeFolder)
{
    if (string.IsNullOrEmpty(assetRelativeFolder))
        return;

    if (!assetRelativeFolder.StartsWith("Assets", StringComparison.OrdinalIgnoreCase))
    {
        string absNonAsset = GetAbsolutePathFromAssetPath(assetRelativeFolder);
        if (!string.IsNullOrEmpty(absNonAsset) && !Directory.Exists(absNonAsset))
            Directory.CreateDirectory(absNonAsset);
        return;
    }

    string normalized = assetRelativeFolder.Replace('\\', '/').TrimEnd('/');
    if (AssetDatabase.IsValidFolder(normalized))
        return;

    string[] parts = normalized.Split('/');
    if (parts.Length == 0 || !string.Equals(parts[0], "Assets", StringComparison.OrdinalIgnoreCase))
        return;

    string current = "Assets";
    for (int i = 1; i < parts.Length; i++)
    {
        string next = current + "/" + parts[i];
        if (!AssetDatabase.IsValidFolder(next))
            AssetDatabase.CreateFolder(current, parts[i]);

        current = next;
    }
}

private static void DeleteAssetFolder(string assetRelativeFolder)
{
    if (string.IsNullOrEmpty(assetRelativeFolder))
        return;

    FileUtil.DeleteFileOrDirectory(assetRelativeFolder);
    FileUtil.DeleteFileOrDirectory(assetRelativeFolder + ".meta");
}

private static string GetAbsolutePathFromAssetPath(string assetPath)
{
    if (string.IsNullOrEmpty(assetPath))
        return null;

    string projectRoot = Directory.GetParent(Application.dataPath).FullName;

    if (string.Equals(assetPath, "Assets", StringComparison.OrdinalIgnoreCase))
        return Application.dataPath;

    if (assetPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
        return Path.Combine(projectRoot, assetPath.Replace('/', Path.DirectorySeparatorChar));

    return assetPath;
}

private void ApplyImportSettingsToAllExportedMobileSprites()
{
    string rootAbs = GetAbsolutePathFromAssetPath(MobileSpritesFolderAssetRelative);
    if (string.IsNullOrWhiteSpace(rootAbs) || !Directory.Exists(rootAbs))
    {
        Debug.LogWarning("[NexusDev][DFU Export] MobileSprites folder not found for import-settings pass.");
        return;
    }

    string[] pngFiles = Directory.GetFiles(rootAbs, "*.png", SearchOption.AllDirectories);
    if (pngFiles == null || pngFiles.Length == 0)
    {
        Debug.LogWarning("[NexusDev][DFU Export] No exported MobileSprites PNG files found.");
        return;
    }

    int changedCount = 0;

    try
    {
        for (int i = 0; i < pngFiles.Length; i++)
        {
            EditorUtility.DisplayProgressBar(
                "DFU Export",
                "Applying import settings to MobileSprites PNGs (" +
                (i + 1).ToString(CultureInfo.InvariantCulture) + "/" +
                pngFiles.Length.ToString(CultureInfo.InvariantCulture) + ")",
                pngFiles.Length > 0 ? (float)(i + 1) / pngFiles.Length : 0f);

            if (ApplyImportSettingsToMobileSpritePng(pngFiles[i]))
                changedCount++;
        }

        AssetDatabase.Refresh();

        Debug.Log(string.Format(
            CultureInfo.InvariantCulture,
            "[NexusDev][DFU Export] Applied MobileSprites import settings to {0} PNG asset(s).",
            changedCount));
    }
    finally
    {
        EditorUtility.ClearProgressBar();
    }
}

private bool ApplyImportSettingsToMobileSpritePng(string absolutePngPath)
{
    if (string.IsNullOrWhiteSpace(absolutePngPath) || !File.Exists(absolutePngPath))
        return false;

    string normalizedAbsolute = absolutePngPath.Replace('\\', '/');
    string assetsAbsolute = Application.dataPath.Replace('\\', '/');

    if (!normalizedAbsolute.StartsWith(assetsAbsolute, StringComparison.OrdinalIgnoreCase))
        return false;

    string relativeAssetPath = "Assets" + normalizedAbsolute.Substring(assetsAbsolute.Length);

    AssetDatabase.ImportAsset(relativeAssetPath, ImportAssetOptions.ForceUpdate);

    TextureImporter importer = AssetImporter.GetAtPath(relativeAssetPath) as TextureImporter;
    if (importer == null)
        return false;

    importer.textureType = TextureImporterType.Default;
    importer.alphaSource = TextureImporterAlphaSource.FromInput;
    importer.alphaIsTransparency = true;
    importer.mipmapEnabled = false;
    importer.filterMode = mobileSpriteImportUsePointFilter ? FilterMode.Point : FilterMode.Bilinear;
    importer.wrapMode = TextureWrapMode.Clamp;
    importer.textureCompression = TextureImporterCompression.Uncompressed;
    importer.isReadable = false;
    importer.maxTextureSize = Mathf.Clamp(mobileSpriteImportMaxTextureSize, 32, 8192);
    importer.npotScale = TextureImporterNPOTScale.None;
    importer.SaveAndReimport();

    return true;
}
private static void ImportMobileSpritePngAtAbsolutePath(string absolutePngPath, int width, int height)
{
    if (string.IsNullOrWhiteSpace(absolutePngPath) || !File.Exists(absolutePngPath))
        return;

    string normalizedAbsolute = absolutePngPath.Replace('\\', '/');
    string assetsAbsolute = Application.dataPath.Replace('\\', '/');

    if (!normalizedAbsolute.StartsWith(assetsAbsolute, StringComparison.OrdinalIgnoreCase))
        return;

    string relativeAssetPath = "Assets" + normalizedAbsolute.Substring(assetsAbsolute.Length);
    int longestDimension = Mathf.Max(width, height);

    AssetDatabase.ImportAsset(relativeAssetPath, ImportAssetOptions.ForceUpdate);
    ConfigureImportedTextureForPointSampling(relativeAssetPath, longestDimension);
}

private static string GetReasonableStartingFolder(string path)
{
    if (!string.IsNullOrEmpty(path))
    {
        if (Directory.Exists(path))
            return path;

        if (File.Exists(path))
            return Path.GetDirectoryName(path);

        string dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
            return dir;
    }

    return Directory.GetParent(Application.dataPath).FullName;
}
}
}

#endif