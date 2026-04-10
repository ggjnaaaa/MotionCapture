import asyncio
import logging
from grpc import aio

from contracts.motion import motion_service_pb2_grpc
from contracts.calibration import calibration_service_pb2_grpc
from contracts.camera import camera_service_pb2_grpc
from contracts.health import health_check_service_pb2_grpc

from app.handlers.motion_handler import MotionHandler
from app.handlers.calibration_handler import CalibrationHandler
from app.handlers.camera_handler import CameraHandler
from app.handlers.health_handler import HealthCheckHandler

from app.services.multi_pose_estimator import MultiPoseEstimator
from app.services.multi_pose_smoother import MultiPoseSmoother


async def serve():
    
    logging.basicConfig(
        level=logging.DEBUG,
        format="%(asctime)s [%(levelname)s] %(name)s: %(message)s",
    )

    logger = logging.getLogger(__name__)

    server = aio.server()
    estimator_manager = MultiPoseEstimator()
    pose_smoother_manager = MultiPoseSmoother()

    motion_service_pb2_grpc.add_MotionServiceServicer_to_server(
        MotionHandler(estimator_manager, pose_smoother_manager),
        server
    )

    calibration_service_pb2_grpc.add_CalibrationServiceServicer_to_server(
        CalibrationHandler(),
        server
    )

    camera_service_pb2_grpc.add_CameraServiceServicer_to_server(
        CameraHandler(estimator_manager, pose_smoother_manager),
        server
    )

    health_check_service_pb2_grpc.add_HealthCheckServiceServicer_to_server(
        HealthCheckHandler(),
        server
    )

    server.add_insecure_port("[::]:50051")

    await server.start()
    logger.info("gRPC aio server started on port 50051")

    await server.wait_for_termination()


if __name__ == "__main__":
    asyncio.run(serve())