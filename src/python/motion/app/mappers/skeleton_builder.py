from app.domain.models.joint2d import Joint2D
from app.domain.enums.landmark_names import LandmarksNames


class SkeletonBuilder:

    def __init__(self):
        self.visibility_threshold = 0.7

    def build(self, smooth_mp_result, width: int, height: int) -> list:
        if not smooth_mp_result:
            return []

        landmarks = smooth_mp_result

        joints = []

        # HEAD (берём нос)
        joints.append(self._landmark_to_joint2d(landmarks[LandmarksNames.NOSE], LandmarksNames.NOSE.name, -1, width, height))

        # LEFT SHOULDER
        joints.append(self._landmark_to_joint2d(landmarks[LandmarksNames.LEFT_SHOULDER], LandmarksNames.LEFT_SHOULDER.name, 0, width, height))

        # RIGHT SHOULDER
        joints.append(self._landmark_to_joint2d(landmarks[LandmarksNames.RIGHT_SHOULDER], LandmarksNames.RIGHT_SHOULDER.name, 0, width, height))

        # LEFT ELBOW
        joints.append(self._landmark_to_joint2d(landmarks[LandmarksNames.LEFT_ELBOW], LandmarksNames.LEFT_ELBOW.name, 1, width, height))

        # RIGHT ELBOW
        joints.append(self._landmark_to_joint2d(landmarks[LandmarksNames.RIGHT_ELBOW], LandmarksNames.RIGHT_ELBOW.name, 2, width, height))

        # LEFT WRIST
        joints.append(self._landmark_to_joint2d(landmarks[LandmarksNames.LEFT_WRIST], LandmarksNames.LEFT_WRIST.name, 3, width, height))

        # RIGHT WRIST
        joints.append(self._landmark_to_joint2d(landmarks[LandmarksNames.RIGHT_WRIST], LandmarksNames.RIGHT_WRIST.name, 4, width, height))

        # LEFT HIP
        joints.append(self._landmark_to_joint2d(landmarks[LandmarksNames.LEFT_HIP], LandmarksNames.LEFT_HIP.name, 1, width, height))

        # RIGHT HIP
        joints.append(self._landmark_to_joint2d(landmarks[LandmarksNames.RIGHT_HIP], LandmarksNames.RIGHT_HIP.name, 2, width, height))

        # LEFT KNEE
        joints.append(self._landmark_to_joint2d(landmarks[LandmarksNames.LEFT_KNEE], LandmarksNames.LEFT_KNEE.name, 7, width, height))

        # RIGHT KNEE
        joints.append(self._landmark_to_joint2d(landmarks[LandmarksNames.RIGHT_KNEE], LandmarksNames.RIGHT_KNEE.name, 8, width, height))

        # LEFT ANKLE
        joints.append(self._landmark_to_joint2d(landmarks[LandmarksNames.LEFT_ANKLE], LandmarksNames.LEFT_ANKLE.name, 9, width, height))

        # RIGHT ANKLE
        joints.append(self._landmark_to_joint2d(landmarks[LandmarksNames.RIGHT_ANKLE], LandmarksNames.RIGHT_ANKLE.name, 10, width, height))

        return joints

    def _px(self, lm, width, height):
        return lm[0] * width, lm[1] * height

    def _landmark_to_joint2d(self, landmark, name: str, parent_index: int, width: int, height: int) -> Joint2D:
        x, y = self._px(landmark, width, height)
        return Joint2D(name=name, parent_index=parent_index, x=x, y=y, depth=landmark[2], is_visible=landmark[3] > self.visibility_threshold)