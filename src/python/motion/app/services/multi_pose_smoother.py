from app.services.pose_smoother import PoseSmoother


class MultiPoseSmoother:

    def __init__(self):
        self.smoothers = {}

    def add_smoother(self, camera_index):
        self.smoothers[camera_index] = PoseSmoother()

    def remove_smoothers(self):
        for idx in list(self.smoothers.keys()):
            del self.smoothers[idx]
    
    def change_camera_index(self, previous, new):
        self.smoothers[new] = self.smoothers[previous]
        del(self.smoothers[previous])

    def smooth(self, camera_index, joints2d):
        return self.smoothers[camera_index].smooth(joints2d)