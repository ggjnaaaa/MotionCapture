from google.protobuf.internal import containers as _containers
from google.protobuf import descriptor as _descriptor
from google.protobuf import message as _message
from collections.abc import Iterable as _Iterable, Mapping as _Mapping
from typing import ClassVar as _ClassVar, Optional as _Optional, Union as _Union

DESCRIPTOR: _descriptor.FileDescriptor

class CameraFrame(_message.Message):
    __slots__ = ("camera_index", "timestamp_ms", "image")
    CAMERA_INDEX_FIELD_NUMBER: _ClassVar[int]
    TIMESTAMP_MS_FIELD_NUMBER: _ClassVar[int]
    IMAGE_FIELD_NUMBER: _ClassVar[int]
    camera_index: int
    timestamp_ms: int
    image: bytes
    def __init__(self, camera_index: _Optional[int] = ..., timestamp_ms: _Optional[int] = ..., image: _Optional[bytes] = ...) -> None: ...

class Joint2D(_message.Message):
    __slots__ = ("name", "parent_index", "x", "y", "depth", "is_visible")
    NAME_FIELD_NUMBER: _ClassVar[int]
    PARENT_INDEX_FIELD_NUMBER: _ClassVar[int]
    X_FIELD_NUMBER: _ClassVar[int]
    Y_FIELD_NUMBER: _ClassVar[int]
    DEPTH_FIELD_NUMBER: _ClassVar[int]
    IS_VISIBLE_FIELD_NUMBER: _ClassVar[int]
    name: str
    parent_index: int
    x: float
    y: float
    depth: float
    is_visible: bool
    def __init__(self, name: _Optional[str] = ..., parent_index: _Optional[int] = ..., x: _Optional[float] = ..., y: _Optional[float] = ..., depth: _Optional[float] = ..., is_visible: bool = ...) -> None: ...

class FrameLandmarks2D(_message.Message):
    __slots__ = ("joints2d", "source_camera_index", "timestamp_ms")
    JOINTS2D_FIELD_NUMBER: _ClassVar[int]
    SOURCE_CAMERA_INDEX_FIELD_NUMBER: _ClassVar[int]
    TIMESTAMP_MS_FIELD_NUMBER: _ClassVar[int]
    joints2d: _containers.RepeatedCompositeFieldContainer[Joint2D]
    source_camera_index: int
    timestamp_ms: int
    def __init__(self, joints2d: _Optional[_Iterable[_Union[Joint2D, _Mapping]]] = ..., source_camera_index: _Optional[int] = ..., timestamp_ms: _Optional[int] = ...) -> None: ...

class JointPosition3D(_message.Message):
    __slots__ = ("name", "parent_index", "pos_x", "pos_y", "pos_z")
    NAME_FIELD_NUMBER: _ClassVar[int]
    PARENT_INDEX_FIELD_NUMBER: _ClassVar[int]
    POS_X_FIELD_NUMBER: _ClassVar[int]
    POS_Y_FIELD_NUMBER: _ClassVar[int]
    POS_Z_FIELD_NUMBER: _ClassVar[int]
    name: str
    parent_index: int
    pos_x: float
    pos_y: float
    pos_z: float
    def __init__(self, name: _Optional[str] = ..., parent_index: _Optional[int] = ..., pos_x: _Optional[float] = ..., pos_y: _Optional[float] = ..., pos_z: _Optional[float] = ...) -> None: ...

class MotionRequest(_message.Message):
    __slots__ = ("frames",)
    FRAMES_FIELD_NUMBER: _ClassVar[int]
    frames: _containers.RepeatedCompositeFieldContainer[CameraFrame]
    def __init__(self, frames: _Optional[_Iterable[_Union[CameraFrame, _Mapping]]] = ...) -> None: ...

class MotionResponse(_message.Message):
    __slots__ = ("joints", "frames_to_draw")
    JOINTS_FIELD_NUMBER: _ClassVar[int]
    FRAMES_TO_DRAW_FIELD_NUMBER: _ClassVar[int]
    joints: _containers.RepeatedCompositeFieldContainer[JointPosition3D]
    frames_to_draw: _containers.RepeatedCompositeFieldContainer[FrameLandmarks2D]
    def __init__(self, joints: _Optional[_Iterable[_Union[JointPosition3D, _Mapping]]] = ..., frames_to_draw: _Optional[_Iterable[_Union[FrameLandmarks2D, _Mapping]]] = ...) -> None: ...
