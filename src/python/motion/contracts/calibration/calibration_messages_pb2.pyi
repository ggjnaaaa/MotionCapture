from motion import motion_messages_pb2 as _motion_messages_pb2
from google.protobuf.internal import containers as _containers
from google.protobuf import descriptor as _descriptor
from google.protobuf import message as _message
from collections.abc import Iterable as _Iterable, Mapping as _Mapping
from typing import ClassVar as _ClassVar, Optional as _Optional, Union as _Union

DESCRIPTOR: _descriptor.FileDescriptor

class CalibrationFrame(_message.Message):
    __slots__ = ("frames",)
    FRAMES_FIELD_NUMBER: _ClassVar[int]
    frames: _containers.RepeatedCompositeFieldContainer[_motion_messages_pb2.CameraFrame]
    def __init__(self, frames: _Optional[_Iterable[_Union[_motion_messages_pb2.CameraFrame, _Mapping]]] = ...) -> None: ...

class CalibrationStatus(_message.Message):
    __slots__ = ("frames_collected", "frames_required", "progress", "is_done", "success", "message", "landmarks")
    FRAMES_COLLECTED_FIELD_NUMBER: _ClassVar[int]
    FRAMES_REQUIRED_FIELD_NUMBER: _ClassVar[int]
    PROGRESS_FIELD_NUMBER: _ClassVar[int]
    IS_DONE_FIELD_NUMBER: _ClassVar[int]
    SUCCESS_FIELD_NUMBER: _ClassVar[int]
    MESSAGE_FIELD_NUMBER: _ClassVar[int]
    LANDMARKS_FIELD_NUMBER: _ClassVar[int]
    frames_collected: int
    frames_required: int
    progress: float
    is_done: bool
    success: bool
    message: str
    landmarks: _containers.RepeatedCompositeFieldContainer[_motion_messages_pb2.FrameLandmarks2D]
    def __init__(self, frames_collected: _Optional[int] = ..., frames_required: _Optional[int] = ..., progress: _Optional[float] = ..., is_done: bool = ..., success: bool = ..., message: _Optional[str] = ..., landmarks: _Optional[_Iterable[_Union[_motion_messages_pb2.FrameLandmarks2D, _Mapping]]] = ...) -> None: ...
