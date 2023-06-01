# made by westingtyler with ChatGPT's help 2023.04.12 3:55pm.
# https://github.com/westingtyler/scripts/tree/main
# https://www.youtube.com/channel/UCdJHTLc_M3CIssLVmykWn_w
# I hope to add a description HERE soon for what this script does, and hopefully a link to a youtube tutorial I will have made, demonstrating the script's proper use.

import bpy

# get selected objects
objs = bpy.context.selected_objects

# get the first object in the selection
first_obj = objs[0]

# iterate over the rest of the objects and perform boolean union
for obj in objs[1:]:
    bool_modifier = first_obj.modifiers.new('Boolean', 'BOOLEAN')
    bool_modifier.object = obj
    bool_modifier.operation = 'UNION'
    bpy.ops.object.modifier_apply(modifier=bool_modifier.name)
