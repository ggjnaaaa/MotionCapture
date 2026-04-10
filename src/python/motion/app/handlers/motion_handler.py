import logging

import cv2
import numpy as np
import mediapipe as mp

from app.services.multi_pose_estimator import MultiPoseEstimator
from contracts.motion import motion_service_pb2_grpc
from app.mappers.proto_mapper import ProtoMapper
from app.domain.models import Joint3D, FrameLandmarks2D
from app.mappers.skeleton_builder import SkeletonBuilder
from app.services.multi_pose_smoother import MultiPoseSmoother


class MotionHandler(motion_service_pb2_grpc.MotionServiceServicer):
    logger = logging.getLogger(__name__)

    def __init__(self, estimator_manager: MultiPoseEstimator, pose_smoother_manager: MultiPoseSmoother):
        self.estimator_manager = estimator_manager
        self.proto_mapper = ProtoMapper()
        self.skeleton_builder = SkeletonBuilder()
        self.pose_smoother_manager = pose_smoother_manager

    async def ProcessMotion(self, request, context):
        sorted_frames = sorted(request.frames, key=lambda f: f.camera_index)
        
        all_joints3d = []
        all_frame_landmarks2d = []

        for frame in sorted_frames:
            self.logger.info(f"Processing frame from camera {frame.camera_index} at timestamp {frame.timestamp_ms} ms")

            estimator = self.estimator_manager.get_estimator(frame.camera_index)
            if estimator is None:
                self.logger.error(f"No PoseEstimator available for camera {frame.camera_index}")
                continue

            np_arr = np.frombuffer(frame.image, np.uint8)
            image_bgr = cv2.imdecode(np_arr, cv2.IMREAD_COLOR)

            if image_bgr is None:
                self.logger.warning(f"Failed to decode image for frame {frame.frame_index}")
                continue

            image_rgb = cv2.cvtColor(image_bgr, cv2.COLOR_BGR2RGB)

            height, width, _ = image_rgb.shape

            mp_image = mp.Image(
                image_format=mp.ImageFormat.SRGB,
                data=np.ascontiguousarray(image_rgb)
            )

            result = estimator.process(
                mp_image,
                frame.timestamp_ms
            )

            self.logger.info(f"MediaPipe detection done. Is result empty? {'Yes' if not result or not result.pose_landmarks else 'No'}")

            if not result or not result.pose_landmarks:
                continue

            smooth_pose_landmarks = self.pose_smoother_manager.smooth(frame.camera_index, result.pose_landmarks[0])

            # ------------------------------------
            # Map to domain Joint2D
            # ------------------------------------
            joints2d = self.skeleton_builder.build(
                smooth_mp_result=smooth_pose_landmarks,
                height=height,
                width=width
            )

            self.logger.info(f"Mapped to {len(joints2d)} joints2d for frame")


            frame_landmarks2d = FrameLandmarks2D(
                joints2d=joints2d,
                source_camera_index=frame.camera_index,
                timestamp_ms=frame.timestamp_ms
            )

            all_frame_landmarks2d.append(frame_landmarks2d)

        if len(all_frame_landmarks2d) == 1:
            self.logger.info("Only one frame detected, skipping 3D estimation")
            # todo сделать маппинг из точек в 3D
            return self.proto_mapper.to_proto_response(
                domain_frame_landmarks=all_frame_landmarks2d,
                joints=[]
            )
        # ----------------------------------------
        # triangulation
        # Пока 3D заглушка
        # ----------------------------------------
        dummy_joint3d = Joint3D(
            name="root",
            parent_index=-1,
            pos_x=0.0,
            pos_y=0.0,
            pos_z=0.0,
            rot_x=0.0,
            rot_y=0.0,
            rot_z=0.0,
            rot_w=1.0
        )

        all_joints3d.append(dummy_joint3d)

        # ----------------------------------------
        # Proto response
        # ----------------------------------------
        return self.proto_mapper.to_proto_response(
            domain_frame_landmarks=all_frame_landmarks2d,
            joints=all_joints3d
        )