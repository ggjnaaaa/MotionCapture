import logging
from typing import Dict, Optional
from app.services.pose_estimator import PoseEstimator


class MultiPoseEstimator:
    logger = logging.getLogger(__name__)
    
    def __init__(self):
        self.estimators_by_camera_index: Dict[int, PoseEstimator] = {}
        self.camera_count = 0
       
    def get_estimator(self, camera_index: int) -> Optional[PoseEstimator]:
        if camera_index not in self.estimators_by_camera_index:
            self.logger.warning(f"No estimator for camera {camera_index}. Available cameras: {list(self.estimators_by_camera_index.keys())}")
            return None
        
        return self.estimators_by_camera_index[camera_index]
    
    def add_estimator(self, index):
        self.estimators_by_camera_index[index] = PoseEstimator()
    
    def remove_estimators(self):
        for idx in list(self.estimators_by_camera_index.keys()):
            del self.estimators_by_camera_index[idx]
            
    def change_camera_index(self, previous, new):
        self.estimators_by_camera_index[new] = self.estimators_by_camera_index[previous]
        del(self.estimators_by_camera_index[previous])
