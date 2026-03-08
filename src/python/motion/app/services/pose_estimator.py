import logging

import mediapipe as mp
from mediapipe.tasks.python import vision
from pathlib import Path
import threading


class PoseEstimator:
    logger = logging.getLogger(__name__)
    
    _lite_model_rel = "../../mediapipe_models/pose_landmarker_lite.task"
    _heavy_model_rel = "../../mediapipe_models/pose_landmarker_heavy.task"

    def __init__(self):
        self._current_model_type = "heavy"
        self._landmarker = None

        self._initialized = True
        self._create_landmarker()

    # ----------------------------------------
    # Public API
    # ----------------------------------------

    def set_model_type(self, model_type: str):
        if model_type not in ("lite", "heavy"):
            raise ValueError("Model type must be 'lite' or 'heavy'")

        if self._current_model_type != model_type:
            self._current_model_type = model_type
            self._create_landmarker()

    def process(self, image_rgb, timestamp_ms: int):
        try:
            return self._landmarker.detect_for_video(image_rgb, timestamp_ms)
        except Exception as e:
            self.logger.error(f"Error during pose estimation: {e}")
            return None

    # ----------------------------------------
    # Internal
    # ----------------------------------------

    def _create_landmarker(self):
        if self._landmarker:
            self._landmarker.close()

        model_path = self._resolve_model_path()

        options = vision.PoseLandmarkerOptions(
            base_options=mp.tasks.BaseOptions(model_asset_path=str(model_path)),
            running_mode=vision.RunningMode.VIDEO
        )

        self._landmarker = vision.PoseLandmarker.create_from_options(options)

    def _resolve_model_path(self):
        base_dir = Path(__file__).parent
        rel_path = self._heavy_model_rel if self._current_model_type == "heavy" else self._lite_model_rel
        return (base_dir / rel_path).resolve()