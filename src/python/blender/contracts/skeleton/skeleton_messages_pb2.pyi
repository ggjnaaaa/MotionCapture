from google.protobuf.internal import containers as _containers
from google.protobuf import descriptor as _descriptor
from google.protobuf import message as _message
from collections.abc import Iterable as _Iterable, Mapping as _Mapping
from typing import ClassVar as _ClassVar, Optional as _Optional, Union as _Union

DESCRIPTOR: _descriptor.FileDescriptor

class SkeletonJoint(_message.Message):
    __slots__ = ("name", "parent_index", "pos_x", "pos_y", "pos_z", "rot_x", "rot_y", "rot_z", "rot_w")
    NAME_FIELD_NUMBER: _ClassVar[int]
    PARENT_INDEX_FIELD_NUMBER: _ClassVar[int]
    POS_X_FIELD_NUMBER: _ClassVar[int]
    POS_Y_FIELD_NUMBER: _ClassVar[int]
    POS_Z_FIELD_NUMBER: _ClassVar[int]
    ROT_X_FIELD_NUMBER: _ClassVar[int]
    ROT_Y_FIELD_NUMBER: _ClassVar[int]
    ROT_Z_FIELD_NUMBER: _ClassVar[int]
    ROT_W_FIELD_NUMBER: _ClassVar[int]
    name: str
    parent_index: int
    pos_x: float
    pos_y: float
    pos_z: float
    rot_x: float
    rot_y: float
    rot_z: float
    rot_w: float
    def __init__(self, name: _Optional[str] = ..., parent_index: _Optional[int] = ..., pos_x: _Optional[float] = ..., pos_y: _Optional[float] = ..., pos_z: _Optional[float] = ..., rot_x: _Optional[float] = ..., rot_y: _Optional[float] = ..., rot_z: _Optional[float] = ..., rot_w: _Optional[float] = ...) -> None: ...

class Skeleton(_message.Message):
    __slots__ = ("joints", "timestamp_ms")
    JOINTS_FIELD_NUMBER: _ClassVar[int]
    TIMESTAMP_MS_FIELD_NUMBER: _ClassVar[int]
    joints: _containers.RepeatedCompositeFieldContainer[SkeletonJoint]
    timestamp_ms: int
    def __init__(self, joints: _Optional[_Iterable[_Union[SkeletonJoint, _Mapping]]] = ..., timestamp_ms: _Optional[int] = ...) -> None: ...

class SkeletonRequest(_message.Message):
    __slots__ = ()
    def __init__(self) -> None: ...
