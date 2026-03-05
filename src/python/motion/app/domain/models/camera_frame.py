from dataclasses import dataclass


@dataclass
class CameraFrame:
    camera_index: int
    timestamp_ms: int
    image: bytes
