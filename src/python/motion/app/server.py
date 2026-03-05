import grpc
import logging
from concurrent import futures

from contracts import motion_service_pb2_grpc
from app.handlers.motion_handler import MotionHandler


def serve():

    logging.basicConfig(
        level=logging.DEBUG,
        format="%(asctime)s [%(levelname)s] %(name)s: %(message)s",
    )

    logger = logging.getLogger(__name__)

    server = grpc.server(futures.ThreadPoolExecutor(max_workers=10))
    
    motion_service_pb2_grpc.add_MotionServiceServicer_to_server(
        MotionHandler(),
        server
    )

    server.add_insecure_port("[::]:50051")
    server.start()
    
    logger.info("gRPC server started on port 50051")

    server.wait_for_termination()