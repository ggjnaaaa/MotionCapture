import bpy
import logging

from app.services.measurement_collector import MeasurementCollector

joint_objects = {}
bone_mesh = None
bone_edges = []

logger = logging.getLogger(__name__)

collector = MeasurementCollector(max_samples=100, sample_interval=2)
collector.start() 

def update_scene(skeleton):
    global bone_mesh

    if not skeleton.joints:
        return 0.01

    joints_list = list(skeleton.joints)
    SCALE_FACTOR = 1.0 / 100.0

    if collector.is_active:
        collector.process_skeleton(joints_list, 1.0)

    for joint in joints_list:
        obj = joint_objects.get(joint.name)
        if obj:
            obj.location = (
                joint.pos_x * SCALE_FACTOR,
                joint.pos_z * SCALE_FACTOR,
                -joint.pos_y * SCALE_FACTOR
            )

    if not joint_objects:
        names = [j.name for j in joints_list]
        init_joints(names)

    idx_map = {j.name: i for i, j in enumerate(joints_list)}
    edges = []
    for i, joint in enumerate(joints_list):
        parent_idx = joint.parent_index
        if 0 <= parent_idx < len(joints_list):
            parent = joints_list[parent_idx]
            parent_i = idx_map[parent.name]
            edges.append((parent_i, i))

    if bone_mesh is None:
        mesh_data = bpy.data.meshes.new("MotionCapture_Skeleton_Mesh")
        bone_mesh = bpy.data.objects.new("MotionCapture_Skeleton", mesh_data)
        bpy.context.collection.objects.link(bone_mesh)
        bone_mesh.show_wire = True
        bone_mesh.show_in_front = True

    verts = [
        (joint.pos_x * SCALE_FACTOR,
         joint.pos_z * SCALE_FACTOR,
         -joint.pos_y * SCALE_FACTOR)
        for joint in joints_list
    ]
    mesh = bone_mesh.data
    mesh.clear_geometry()
    mesh.from_pydata(verts, edges, [])
    mesh.update()

    return 0.01

def create_joint(name):
    obj = bpy.data.objects.new(f"MotionCapture_{name}", None)
    obj.empty_display_type = 'SPHERE'
    obj.empty_display_size = 0.001
    bpy.context.collection.objects.link(obj)
    return obj

def init_joints(joint_names):
    for name in joint_names:
        joint_objects[name] = create_joint(name)