import os
import logging

def setup_logging():
    log_dir = os.path.dirname(__file__)
    log_file = os.path.join(log_dir, "blender_MOTIONCAPTURE.log")

    logging.basicConfig(
        filename=log_file,
        level=logging.DEBUG,
        format="%(asctime)s [%(levelname)s] %(message)s",
        filemode="w"
    )

