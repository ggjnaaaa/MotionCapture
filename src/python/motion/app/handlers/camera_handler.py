import logging

from google.protobuf import empty_pb2

from app.services.multi_pose_estimator import MultiPoseEstimator
from contracts.camera import camera_service_pb2_grpc
from app.services.multi_pose_smoother import MultiPoseSmoother


class CameraHandler(camera_service_pb2_grpc.CameraServiceServicer):
    logger = logging.getLogger(__name__)

    def __init__(self, estimator_manager: MultiPoseEstimator, pose_smoother_manager: MultiPoseSmoother):
        self.estimator_manager = estimator_manager
        self.pose_smoother_manager = pose_smoother_manager

    async def AddCameraIndex(self, request, context):
        self.logger.info(f"Added camera index {request.camera_Index}")

        self.estimator_manager.add_estimator(request.camera_Index)
        self.pose_smoother_manager.add_smoother(request.camera_Index)

        return empty_pb2.Empty()
    
    async def RemoveCameras(self, request, context):
        self.logger.info("Remove all estimators")

        self.estimator_manager.remove_estimators()
        self.pose_smoother_manager.remove_smoothers()

        return empty_pb2.Empty()
    
    async def ChangeCameraIndex(self, request, context):
        self.logger.info(f"Camera index {request.previous_camera_Index} changed to {request.new_camera_Index}")

        self.estimator_manager.change_camera_index(request.previous_camera_Index, request.new_camera_Index)
        self.pose_smoother_manager.change_camera_index(request.previous_camera_Index, request.new_camera_Index)
        
        return empty_pb2.Empty()