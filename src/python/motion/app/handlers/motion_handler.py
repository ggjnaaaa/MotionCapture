import logging

import cv2
import numpy as np
import mediapipe as mp
from google.protobuf import empty_pb2

from app.services.multi_pose_estimator import MultiPoseEstimator
from contracts import motion_service_pb2_grpc
from app.mappers.proto_mapper import ProtoMapper
from app.domain.models import Joint3D, FrameLandmarks2D
from app.mappers.skeleton_builder import SkeletonBuilder


class MotionHandler(motion_service_pb2_grpc.MotionServiceServicer):
    logger = logging.getLogger(__name__)

    def __init__(self):
        self.estimator_manager = MultiPoseEstimator()
        self.proto_mapper = ProtoMapper()
        self.skeleton_builder = SkeletonBuilder()

    def ProcessMotion(self, request, context):
        if not self.estimator_manager.is_initialized():
            self.logger.error("PoseEstimator manager not initialized. Call ChangeCameraCount first.")
            context.abort(context.code.FAILED_PRECONDITION, 
                         "Motion handler not initialized. Please set camera count first.")
        
        sorted_frames = sorted(request.frames, key=lambda f: f.camera_index)
        
        all_joints3d = []
        all_frame_landmarks2d = []

        for frame in sorted_frames:
            self.logger.info(f"Processing frame from camera {frame.camera_index} at timestamp {frame.timestamp_ms} ms")

            estimator = self.estimator_manager.get_estimator(frame.camera_index)
            if estimator is None:
                self.logger.error(f"No PoseEstimator available for camera {frame.camera_index}")
                continue

            # ------------------------------------
            # bytes → numpy
            # ------------------------------------
            np_arr = np.frombuffer(frame.image, np.uint8)
            image_bgr = cv2.imdecode(np_arr, cv2.IMREAD_COLOR)

            if image_bgr is None:
                self.logger.warning(f"Failed to decode image for frame {frame.frame_index}")
                continue

            image_rgb = cv2.cvtColor(image_bgr, cv2.COLOR_BGR2RGB)

            height, width, _ = image_rgb.shape

            mp_image = mp.Image(
                image_format=mp.ImageFormat.SRGB,
                data=image_rgb
            )

            self.logger.info(f"Image shape: {image_rgb.shape}")

            # ------------------------------------
            # MediaPipe detect
            # ------------------------------------
            result = estimator.process(
                mp_image,
                frame.timestamp_ms
            )

            self.logger.info(f"MediaPipe detection done. Is result empty? {'Yes' if not result or not result.pose_landmarks else 'No'}")

            # ------------------------------------
            # Map to domain Joint2D
            # ------------------------------------
            joints2d = self.skeleton_builder.build(
                mp_result=result,
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
    
    def AddCameraIndex(self, request, context):
        self.logger.info(f"Added camera index {request.camera_Index}")

        self.estimator_manager.add_estimator(request.camera_Index)

        return empty_pb2.Empty()
    
    def RemoveCameras(self, request, context):
        self.logger.info("Remove all estimators")

        self.estimator_manager.remove_estimators()

        return empty_pb2.Empty()
    
    def ChangeCameraIndex(self, request, context):
        self.logger.info(f"Camera index {request.previous_camera_Index} changed to {request.new_camera_Index}")

        self.estimator_manager.change_camera_index(request.previous_camera_Index, request.new_camera_Index)
        
        return empty_pb2.Empty()