import threading
import logging
from contracts.skeleton import skeleton_messages_pb2
from contracts.skeleton import skeleton_service_pb2_grpc
import grpc

from app.services.scene_editor import update_scene

logger = logging.getLogger(__name__)

def listen_to_stream():
    logger.info("gRPC thread started, connecting to localhost:5001...")
    try:
        channel = grpc.insecure_channel("localhost:5001")
        stub = skeleton_service_pb2_grpc.SkeletonServiceStub(channel)
        logger.info("Channel created, calling StreamSkeleton...")
        request = skeleton_messages_pb2.SkeletonRequest()
        for skeleton in stub.StreamSkeleton(request):
            logger.debug(f"Received skeleton: {skeleton}")
            update_scene(skeleton)
        logger.warning("Thread is closed, connection with server is lost.")
    except grpc.RpcError as e:
        logger.error(f"gRPC error: {e.code()} - {e.details()}")
    except Exception as e:
        logger.exception("Unexpected error in gRPC thread")

def start_grpc():
    logger.info("Starting gRPC thread...")
    t = threading.Thread(target=listen_to_stream, daemon=True)
    t.start()