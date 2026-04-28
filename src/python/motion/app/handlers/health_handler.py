import logging

from google.protobuf import empty_pb2

from contracts.health import health_check_service_pb2_grpc


class HealthCheckHandler(health_check_service_pb2_grpc.HealthCheckServiceServicer):
    logger = logging.getLogger(__name__)

    async def CheckHealth(self, request, context):
        self.logger.info("Health check")
        return empty_pb2.Empty()