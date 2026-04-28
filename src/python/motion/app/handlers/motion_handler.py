import logging

import cv2
import numpy as np
import mediapipe as mp
import os
import grpc
from typing import Optional, Dict, Tuple

from app.services.multi_pose_estimator import MultiPoseEstimator
from contracts.motion import motion_service_pb2_grpc
from app.mappers.proto_mapper import ProtoMapper
from app.domain.models import JointPosition3D, FrameLandmarks2D
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
        cam_landmarks = {}

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
            cam_landmarks[frame.camera_index] = joints2d
        
        if len(all_frame_landmarks2d) == 1:
            self.logger.info("Only one frame detected, performing single‑camera 3D estimation")
            frame_landmarks = all_frame_landmarks2d[0]

            single_calib = self._load_single_calibration()
            if single_calib is None:
                self.logger.warning("No single camera calibration available, returning only 2D")
                return self.proto_mapper.to_proto_response(
                    domain_frame_landmarks=all_frame_landmarks2d,
                    joints=[]
                )

            mtx = single_calib["mtx"]
            dist = single_calib["dist"]

            joints_3d = []
            for j2d in frame_landmarks.joints2d:
                if not j2d.is_visible:
                    continue
                pt3d = self.project_2d_to_3d(
                    (j2d.x, j2d.y),
                    j2d.depth,
                    mtx, dist
                )
                if pt3d is None:
                    continue
                joint3d = JointPosition3D(
                    name=j2d.name,
                    parent_index=j2d.parent_index,
                    pos_x=pt3d[0],
                    pos_y=pt3d[1],
                    pos_z=pt3d[2]
                )
                joints_3d.append(joint3d)

            self.logger.info(f"Generated {len(joints_3d)} 3D joints from single camera")
            return self.proto_mapper.to_proto_response(
                domain_frame_landmarks=all_frame_landmarks2d,
                joints=joints_3d
            )
        
        calib = self._load_stereo_calibration()

        if not calib:
            self.logger.warning("Stereo calibration data not available, skipping 3D estimation")
            context.abort(grpc.StatusCode.FAILED_PRECONDITION, "Stereo calibration data not available")
            return self.proto_mapper.to_proto_response(
                domain_frame_landmarks=all_frame_landmarks2d,
                joints=[]
            )
        
        if len(cam_landmarks) < 2:
            self.logger.warning("Less than 2 sets of landmarks detected, cannot perform stereo triangulation")
            context.abort(grpc.StatusCode.FAILED_PRECONDITION, "Not enough landmark data for stereo triangulation")
            return self.proto_mapper.to_proto_response(
                domain_frame_landmarks=all_frame_landmarks2d,
                joints=[]
            )
        
        cam_indices = sorted(cam_landmarks.keys())
        cam1, cam2 = cam_indices[0], cam_indices[1]
        joints1 = cam_landmarks[cam1]
        joints2 = cam_landmarks[cam2]

        joints2_dict = {j.name: j for j in joints2}

        mtx1 = calib["mtx1"]
        dist1 = calib["dist1"]
        mtx2 = calib["mtx2"]
        dist2 = calib["dist2"]
        R = calib["R"]
        T = calib["T"]

        for j1 in joints1:
            j2 = joints2_dict.get(j1.name)
            if j2 is None or not j1.is_visible or not j2.is_visible:
                continue

            pt3d = self.triangulate_point(
                (j1.x, j1.y), (j2.x, j2.y),
                mtx1, dist1, mtx2, dist2, R, T
            )
            if pt3d is None:
                continue

            joint3d = JointPosition3D(
                name=j1.name,
                parent_index=j1.parent_index,
                pos_x=pt3d[0],
                pos_y=pt3d[1],
                pos_z=pt3d[2]
            )
            all_joints3d.append(joint3d)

        self.logger.info(f"Triangulated {len(all_joints3d)} 3D joints.")

        return self.proto_mapper.to_proto_response(
            domain_frame_landmarks=all_frame_landmarks2d,
            joints=all_joints3d
        )
    
    def _load_single_calibration(self) -> Optional[Dict[str, np.ndarray]]:
        single_path = "calibration_single.npz"
        if not os.path.exists(single_path):
            self.logger.warning("Single calibration file not found")
            return None
        try:
            data = np.load(single_path)
            return {
                "mtx": data["mtx"],
                "dist": data["dist"],
            }
        except Exception as e:
            self.logger.error(f"Failed to load single calibration: {e}")
            return None
    
    def _load_stereo_calibration(self) -> Optional[Dict[str, np.ndarray]]:
        """Load parameters from stereo calibration file and return them as a dictionary."""
        stereo_path = "calibration_stereo.npz"
        if not os.path.exists(stereo_path):
            self.logger.warning("Stereo calibration file not found")
            return None
        try:
            data = np.load(stereo_path)
            return {
                "mtx1": data["mtx1"],
                "dist1": data["dist1"],
                "mtx2": data["mtx2"],
                "dist2": data["dist2"],
                "R": data["R"],
                "T": data["T"],
            }
        except Exception as e:
            self.logger.error(f"Failed to load calibration: {e}")
            return None
        
    def triangulate_point(self,
    pt1: Tuple[float, float],
    pt2: Tuple[float, float],
    mtx1: np.ndarray,
    dist1: np.ndarray,
    mtx2: np.ndarray,
    dist2: np.ndarray,
    R: np.ndarray,
    T: np.ndarray,
    ) -> Optional[Tuple[float, float, float]]:
        """
        Realizes triangulation of a 3D point based on two 2D projections.
        Returns (X, Y, Z) in the coordinate system of the first camera or None in case of an error.
        """
        #  Convert to format (N,1,2) for cv2.undistortPoints
        pts1 = np.array([[pt1]], dtype=np.float32)
        pts2 = np.array([[pt2]], dtype=np.float32)

        # Remove distortion
        undist_pts1 = cv2.undistortPoints(pts1, mtx1, dist1, P=mtx1)
        undist_pts2 = cv2.undistortPoints(pts2, mtx2, dist2, P=mtx2)

        # Projection matrices
        P1 = mtx1 @ np.hstack((np.eye(3), np.zeros((3, 1))))
        P2 = mtx2 @ np.hstack((R, T))

        # Triangulate points
        pts4d = cv2.triangulatePoints(P1, P2, undist_pts1.T, undist_pts2.T)
        if pts4d[3] == 0:
            return None
        pts3d = pts4d[:3] / pts4d[3]
        return tuple(pts3d.flatten())
    
    def project_2d_to_3d(self,
    pt_2d: Tuple[float, float],
    depth_z: float,
    mtx: np.ndarray,
    dist: np.ndarray,
    ) -> Optional[Tuple[float, float, float]]:
        """
        Converts a 2D point and depth (z) to 3D coordinates in the camera coordinate system.
        depth_z — depth estimate from MediaPipe (usually in the range [-1..1]).
        """
        # Convert to format for undistortPoints
        pts = np.array([[pt_2d]], dtype=np.float32)
        undistorted = cv2.undistortPoints(pts, mtx, dist, P=mtx)
        x_norm = undistorted[0, 0, 0]
        y_norm = undistorted[0, 0, 1]

        # Depth can be scaled by an arbitrary factor.
        # MediaPipe returns a relative depth, where 0 is the center of the body.
        # Multiply by a constant for visual convenience (e.g., 1.0).
        scale = 1.0
        Z = depth_z * scale

        X = x_norm * Z
        Y = y_norm * Z
        return (X, Y, Z)