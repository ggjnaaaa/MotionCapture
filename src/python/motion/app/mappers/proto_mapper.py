from typing import List
from contracts import motion_messages_pb2
from app.domain.models import CameraFrame, Joint2D, Joint3D, FrameLandmarks2D


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
            y=domain_joint.y
        )

    @staticmethod
    def to_proto_joint3d(domain_joint: Joint3D) -> motion_messages_pb2.Joint3D:
        return motion_messages_pb2.Joint3D(
            name=domain_joint.name,
            parent_index=domain_joint.parent_index,
            pos_x=domain_joint.pos_x,
            pos_y=domain_joint.pos_y,
            pos_z=domain_joint.pos_z,
            rot_x=domain_joint.rot_x,
            rot_y=domain_joint.rot_y,
            rot_z=domain_joint.rot_z,
            rot_w=domain_joint.rot_w
        )

    @staticmethod
    def to_proto_response(domain_frame_landmarks: FrameLandmarks2D, joints: List[Joint3D]) -> motion_messages_pb2.MotionResponse:
        return motion_messages_pb2.MotionResponse(
            joints=[
                ProtoMapper.to_proto_joint3d(j)
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