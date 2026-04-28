from dataclasses import dataclass


@dataclass
class Joint2D:
    name: str
    parent_index: int
    x: float
    y: float
    depth: float
    is_visible: bool