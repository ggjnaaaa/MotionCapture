from dataclasses import dataclass


@dataclass
class Joint3D:
    name: str
    parent_index: int
    pos_x: float
    pos_y: float
    pos_z: float
    rot_x: float
    rot_y: float
    rot_z: float
    rot_w: float