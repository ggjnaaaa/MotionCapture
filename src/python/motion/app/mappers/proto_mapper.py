from typing import List
from contracts.motion import motion_messages_pb2
from app.domain.models import CameraFrame, Joint2D, JointPosition3D, FrameLandmarks2D
from contracts.calibration import calibration_messages_pb2


class ProtoMapper:

    # ---------- proto → domain ----------

    @staticmethod
    def to_domain_frame(proto_frame) -> CameraFrame:
        return CameraFrame(
            camera_index=proto_frame.camera_index,
            timestamp_ms=proto_frame.timestamp_ms,
            image=proto_frame.image
        )

    # ---------- domain → proto ----------

    @staticmethod
    def to_proto_joint2d(domain_joint: Joint2D) -> motion_messages_pb2.Joint2D:
        return motion_messages_pb2.Joint2D(
            name=domain_joint.name,
            parent_index=domain_joint.parent_index,
            x=domain_joint.x,
            y=domain_joint.y,
            depth=domain_joint.depth,
            is_visible=domain_joint.is_visible
        )

    @staticmethod
    def to_proto_jointPosition3d(domain_joint: JointPosition3D) -> motion_messages_pb2.JointPosition3D:
        return motion_messages_pb2.JointPosition3D(
            name=domain_joint.name,
            parent_index=domain_joint.parent_index,
            pos_x=domain_joint.pos_x,
            pos_y=domain_joint.pos_y,
            pos_z=domain_joint.pos_z
        )

    @staticmethod
    def to_proto_response(domain_frame_landmarks: FrameLandmarks2D, joints: List[JointPosition3D]) -> motion_messages_pb2.MotionResponse:
        return motion_messages_pb2.MotionResponse(
            joints=[
                ProtoMapper.to_proto_jointPosition3d(j)
                for j in joints
            ],
            frames_to_draw=[
                ProtoMapper.to_proto_frame_landmarks2d(f)
                for f in domain_frame_landmarks
            ]
        )
    
    @staticmethod
    def to_proto_frame_landmarks2d(domain_frame_landmarks: FrameLandmarks2D) -> motion_messages_pb2.FrameLandmarks2D:
        return motion_messages_pb2.FrameLandmarks2D(
            source_camera_index=domain_frame_landmarks.source_camera_index,
            timestamp_ms=domain_frame_landmarks.timestamp_ms,
            joints2d=[
                ProtoMapper.to_proto_joint2d(j)
                for j in domain_frame_landmarks.joints2d
            ],
    )

    @staticmethod
    def to_calibration_response(
        frames_collected: int = 0,
        frames_required: int = 0,
        progress: float = 0.0,
        is_done: bool = False,
        success: bool = False,
        message: str = "",
        landmarks: List[FrameLandmarks2D] = None
        ) -> calibration_messages_pb2.CalibrationStatus:
        landmarks = landmarks or []
        return calibration_messages_pb2.CalibrationStatus(
            frames_collected=frames_collected,
            frames_required=frames_required,
            progress=progress,
            is_done=is_done,
            success=success,
            message=message,
            landmarks=[
                ProtoMapper.to_proto_frame_landmarks2d(f)
                for f in landmarks
            ]
        )