from google.protobuf import descriptor as _descriptor
from google.protobuf import message as _message
from typing import ClassVar as _ClassVar, Optional as _Optional

DESCRIPTOR: _descriptor.FileDescriptor

class AddCameraIndexRequest(_message.Message):
    __slots__ = ("camera_Index",)
    CAMERA_INDEX_FIELD_NUMBER: _ClassVar[int]
    camera_Index: int
    def __init__(self, camera_Index: _Optional[int] = ...) -> None: ...

class ChangeCameraIndexRequest(_message.Message):
    __slots__ = ("previous_camera_Index", "new_camera_Index")
    PREVIOUS_CAMERA_INDEX_FIELD_NUMBER: _ClassVar[int]
    NEW_CAMERA_INDEX_FIELD_NUMBER: _ClassVar[int]
    previous_camera_Index: int
    new_camera_Index: int
    def __init__(self, previous_camera_Index: _Optional[int] = ..., new_camera_Index: _Optional[int] = ...) -> None: ...
