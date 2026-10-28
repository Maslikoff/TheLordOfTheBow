import os
import sys

import bpy


def arguments_after_separator():
    if "--" not in sys.argv:
        raise RuntimeError("Expected input and output paths after --")
    values = sys.argv[sys.argv.index("--") + 1:]
    if len(values) != 2:
        raise RuntimeError("Expected exactly two arguments: input.glb output.fbx")
    return values[0], values[1]


def main():
    input_path, output_path = arguments_after_separator()
    if not os.path.isfile(input_path):
        raise FileNotFoundError(input_path)

    os.makedirs(os.path.dirname(output_path), exist_ok=True)
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.gltf(filepath=input_path)

    # Preserve every imported action. Mesh2Motion exports multiple clips in one GLB.
    for action in bpy.data.actions:
        action.use_fake_user = True

    bpy.ops.export_scene.fbx(
        filepath=output_path,
        check_existing=False,
        use_selection=False,
        object_types={'ARMATURE', 'MESH', 'EMPTY'},
        use_mesh_modifiers=True,
        add_leaf_bones=False,
        primary_bone_axis='Y',
        secondary_bone_axis='X',
        axis_forward='-Z',
        axis_up='Y',
        apply_unit_scale=True,
        apply_scale_options='FBX_SCALE_ALL',
        path_mode='COPY',
        embed_textures=True,
        bake_anim=True,
        bake_anim_use_all_bones=True,
        bake_anim_use_nla_strips=True,
        bake_anim_use_all_actions=True,
        bake_anim_force_startend_keying=True,
        bake_anim_simplify_factor=0.0,
    )


if __name__ == "__main__":
    main()
