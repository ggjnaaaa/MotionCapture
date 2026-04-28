import asyncio
import logging

import cv2
import numpy as np

from app.mappers.proto_mapper import ProtoMapper
from contracts.calibration import calibration_service_pb2_grpc
from app.domain.models.frame_landmarks2d import FrameLandmarks2D
from app.domain.models.joint2d import Joint2D

CHESSBOARD_SIZE = (9, 6)
SQUARE_SIZE = 2.0
REQUIRED_FRAMES = 50
QUALITY_THRESHOLD = 5.0 
IS_DEBUG = True


class CalibrationHandler(calibration_service_pb2_grpc.CalibrationServiceServicer):
    logger = logging.getLogger(__name__)
    
    def __init__(self):
        self.reset()

    def reset(self):
        self.mode = None
        self.objpoints = []
        self.imgpoints = {}
        self.img_sizes = {}
        self.last_corners = {}
        self.collected = 0

        self.objp = np.zeros((CHESSBOARD_SIZE[0] * CHESSBOARD_SIZE[1], 3), np.float32)
        self.objp[:, :2] = np.mgrid[0:CHESSBOARD_SIZE[0], 0:CHESSBOARD_SIZE[1]].T.reshape(-1, 2)
        self.objp *= SQUARE_SIZE

        self.proto_mapper = ProtoMapper()
    
    def detect_mode(self, cam_count):
        new_mode = None
        if cam_count == 1:
            new_mode = "single"
        elif cam_count >= 2:
            new_mode = "stereo"

        if self.mode is None:
            self.mode = new_mode
        elif self.mode == "single" and new_mode == "stereo":
            self.reset()
            self.mode = "stereo"
        elif self.mode == "stereo" and new_mode == "single":
            return False
        
        return True
    
    def quality_check(self, corners, cam_index):
        if cam_index in self.last_corners:
            diff = np.linalg.norm(corners - self.last_corners[cam_index])
            if diff < QUALITY_THRESHOLD:
                self.logger.info(f"Frame rejected due to low quality (diff={diff:.2f})")
                return False
        return True
    
    async def process_camera(self, cam_index, frame):
        img = self._bytes_to_image(frame.image)
        gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
        h, w = gray.shape[:2]
        self.img_sizes[cam_index] = [h, w]
        ret, corners = cv2.findChessboardCorners(gray, CHESSBOARD_SIZE, None)
        return cam_index, ret, corners

    async def Calibrate(self, request_iterator, context):
        self.reset()
        self.logger.info("Calibration started")

        async for request in request_iterator:
            if context.cancelled():
                self.logger.info("Calibration cancelled by client")
                return
    
            self.logger.info("Processing new frames...")

            frames = request.frames
            frames_by_cam = {f.camera_index: f for f in frames}
            cam_count = len(frames_by_cam)

            if not self.detect_mode(cam_count):
                self.logger.warning("Inconsistent camera count. Please restart calibration with the same number of cameras.")
                yield self.proto_mapper.to_calibration_response(
                    is_done=True,
                    success=False,
                    message="Inconsistent camera count. Please restart calibration with the same number of cameras."
                )
                return

            tasks = [self.process_camera(cam_index, frame) 
                 for cam_index, frame in frames_by_cam.items()]
            results = await asyncio.gather(*tasks)

            all_success = True
            corners_by_cam = {}
            landmarks_response = []
            images_for_debug = {}

            for cam_index, ret, corners in results:
                if IS_DEBUG:
                    img = self._bytes_to_image(frames_by_cam[cam_index].image)
                    images_for_debug[cam_index] = (img, None)

                if not ret or not self.quality_check(corners, cam_index):
                    self.logger.info("Frame rejected")
                    all_success = False
                    break
                corners_by_cam[cam_index] = corners
            
            if IS_DEBUG:
                for cam_index, (img, corners) in images_for_debug.items():
                    window_name = f"Camera {cam_index}"
                    if corners is not None:
                        cv2.drawChessboardCorners(img, CHESSBOARD_SIZE, corners, True)
                    cv2.imshow(window_name, img)
                cv2.waitKey(1)

            if not all_success:
                self.logger.info("Frame rejected: chessboard not found on all cameras")
                progress = self.collected / REQUIRED_FRAMES
                yield self.proto_mapper.to_calibration_response(
                    frames_collected=self.collected,
                    frames_required=REQUIRED_FRAMES,
                    progress=progress,
                    is_done=False,
                    landmarks=[],
                    message="Waiting for chessboard on all cameras..."
                )
                continue

            for cam_index, corners in corners_by_cam.items():
                self.last_corners[cam_index] = corners
                if cam_index not in self.imgpoints:
                    self.imgpoints[cam_index] = []
                self.imgpoints[cam_index].append(corners)

                joints = []
                for c in corners:
                    x, y = c.ravel()
                    joints.append(Joint2D(
                        name="corner", parent_index=-1,
                        x=float(x), y=float(y), depth=0, is_visible=True
                    ))
                landmarks_response.append(
                    FrameLandmarks2D(
                        joints2d=joints,
                        source_camera_index=cam_index,
                        timestamp_ms=frames_by_cam[cam_index].timestamp_ms
                    )
                )

            self.objpoints.append(self.objp)
            self.collected += 1
            self.logger.info(f"Accepted frame. Collected: {self.collected}/{REQUIRED_FRAMES}")

            progress = self.collected / REQUIRED_FRAMES
            yield self.proto_mapper.to_calibration_response(
                frames_collected=self.collected,
                frames_required=REQUIRED_FRAMES,
                progress=progress,
                is_done=False,
                landmarks=landmarks_response,
                message="Processing."
            )

            if self.collected >= REQUIRED_FRAMES:
                break

        if self.collected < 5:
            yield self.proto_mapper.to_calibration_response(
                is_done=True,
                success=False,
                message="Not enough data. Please ensure the chessboard is clearly visible in all cameras and try again."
            )
            return

        result = self._calibrate()
        self.logger.info("Calibration complete")

        if IS_DEBUG:
            cv2.destroyAllWindows()

        yield self.proto_mapper.to_calibration_response(
            frames_collected=self.collected,
            frames_required=REQUIRED_FRAMES,
            progress=1.0,
            is_done=True,
            success=True,
            message="Calibration complete."
        )

    def _calibrate(self):
        self.logger.info("Calibrating...")
        cams = list(self.imgpoints.keys())

        if len(cams) == 1:
            cam = cams[0]
            imgpoints = self.imgpoints[cam]

            h, w = self.img_sizes[cam]

            self.logger.info("Running single camera calibration...")
            _, mtx, dist, _, _ = cv2.calibrateCamera(
                self.objpoints,
                imgpoints,
                (w, h),
                None,
                None
            )

            self.logger.info("Single camera calibration complete")
            np.savez("calibration_single.npz",
                     mtx=mtx,
                     dist=dist)

        else:
            cam1, cam2 = cams[:2]

            imgpoints1 = self.imgpoints[cam1]
            imgpoints2 = self.imgpoints[cam2]

            h1, w1 = self.img_sizes[cam1]
            h2, w2 = self.img_sizes[cam2]

            self.logger.info("Running stereo calibration...")
            _, mtx1, dist1, _, _ = cv2.calibrateCamera(
                self.objpoints, imgpoints1, (w1, h1), None, None)

            self.logger.info("Stereo calibration complete for first camera. Calibrating second camera...")
            _, mtx2, dist2, _, _ = cv2.calibrateCamera(
                self.objpoints, imgpoints2, (w2, h2), None, None)

            self.logger.info("Stereo calibration complete for second camera. Computing stereo parameters...")
            ret, mtx1_opt, dist1_opt, mtx2_opt, dist2_opt, R, T, E, F = cv2.stereoCalibrate(
                self.objpoints,
                imgpoints1,
                imgpoints2,
                mtx1,
                dist1,
                mtx2,
                dist2,
                (w, h)
            )

            self.logger.info("Stereo calibration complete")
            np.savez("calibration_stereo.npz",
                mtx1=mtx1_opt, dist1=dist1_opt,
                mtx2=mtx2_opt, dist2=dist2_opt,
                R=R, T=T)

    def _bytes_to_image(self, image_bytes):
        np_arr = np.frombuffer(image_bytes, np.uint8)
        return cv2.imdecode(np_arr, cv2.IMREAD_COLOR)