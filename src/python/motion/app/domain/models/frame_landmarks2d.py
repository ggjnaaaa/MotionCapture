from dataclasses import dataclass
from typing import List
from app.domain.models.joint2d import Joint2D


@dataclass
class FrameLandmarks2D:
    joints2d: List[Joint2D]
    source_camera_index: int
    timestamp_ms: int