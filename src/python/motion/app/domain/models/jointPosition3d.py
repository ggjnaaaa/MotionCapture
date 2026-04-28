from dataclasses import dataclass


@dataclass
class JointPosition3D:
    name: str
    parent_index: int
    pos_x: float
    pos_y: float
    pos_z: float