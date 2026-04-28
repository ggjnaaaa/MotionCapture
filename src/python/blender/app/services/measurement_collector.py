import csv
import math
import os
from datetime import datetime
from typing import Dict, List, Tuple, Optional

INDEX_TO_NAME = {
    0: "NOSE",
    1: "LEFT_EYE_INNER", 2: "LEFT_EYE", 3: "LEFT_EYE_OUTER",
    4: "RIGHT_EYE_INNER", 5: "RIGHT_EYE", 6: "RIGHT_EYE_OUTER",
    7: "LEFT_EAR", 8: "RIGHT_EAR",
    9: "MOUTH_LEFT", 10: "MOUTH_RIGHT",
    11: "LEFT_SHOULDER", 12: "RIGHT_SHOULDER",
    13: "LEFT_ELBOW", 14: "RIGHT_ELBOW",
    15: "LEFT_WRIST", 16: "RIGHT_WRIST",
    17: "LEFT_PINKY", 18: "RIGHT_PINKY",
    19: "LEFT_INDEX", 20: "RIGHT_INDEX",
    21: "LEFT_THUMB", 22: "RIGHT_THUMB",
    23: "LEFT_HIP", 24: "RIGHT_HIP",
    25: "LEFT_KNEE", 26: "RIGHT_KNEE",
    27: "LEFT_ANKLE", 28: "RIGHT_ANKLE",
    29: "LEFT_HEEL", 30: "RIGHT_HEEL",
    31: "LEFT_FOOT_INDEX", 32: "RIGHT_FOOT_INDEX"
}

SEGMENTS = [
    ("LEFT_SHOULDER", "RIGHT_SHOULDER", "shoulder_width"),
    ("LEFT_SHOULDER", "LEFT_ELBOW", "left_upper_arm"),
    ("LEFT_ELBOW", "LEFT_WRIST", "left_forearm"),
    ("RIGHT_SHOULDER", "RIGHT_ELBOW", "right_upper_arm"),
    ("RIGHT_ELBOW", "RIGHT_WRIST", "right_forearm"),
    ("LEFT_SHOULDER", "LEFT_HIP", "left_torso"),
    ("RIGHT_SHOULDER", "RIGHT_HIP", "right_torso"),
    ("LEFT_HIP", "LEFT_KNEE", "left_thigh"),
    ("LEFT_KNEE", "LEFT_ANKLE", "left_shin"),
    ("RIGHT_HIP", "RIGHT_KNEE", "right_thigh"),
    ("RIGHT_KNEE", "RIGHT_ANKLE", "right_shin"),
]

class MeasurementCollector:
    def __init__(self, output_dir: str = None, max_samples: int = 100, sample_interval: int = 2):
        """
        :param output_dir: папка для сохранения CSV (по умолчанию та же, где скрипт)
        :param max_samples: сколько измерений собрать
        :param sample_interval: собирать каждые N вызовов update_scene
        """
        self.max_samples = max_samples
        self.sample_interval = sample_interval
        self.frame_counter = 0
        self.samples_collected = 0
        self.measurements: List[Dict] = []
        self.is_active = True

        if output_dir is None:
            output_dir = os.path.dirname(os.path.abspath(__file__))
        self.output_dir = output_dir

    def process_skeleton(self, joints_list, scale_factor: float = 1.0/100.0):
        """
        Вызывается из update_scene при каждом обновлении.
        joints_list: список protobuf-объектов Joint (с полями name, pos_x, pos_y, pos_z)
        scale_factor: коэффициент, использованный для перевода в метры.
                      Чтобы получить сантиметры, делим координаты на scale_factor.
        """
        if not self.is_active:
            return

        self.frame_counter += 1
        if self.frame_counter % self.sample_interval != 0:
            return

        cm_factor = 1.0 / scale_factor

        points_cm = {}
        for joint in joints_list:
            x_cm = joint.pos_x * cm_factor
            y_cm = joint.pos_y * cm_factor
            z_cm = joint.pos_z * cm_factor
            points_cm[joint.name] = (x_cm, y_cm, z_cm)

        sample = {"sample_id": self.samples_collected + 1}
        for name1, name2, label in SEGMENTS:
            if name1 in points_cm and name2 in points_cm:
                p1 = points_cm[name1]
                p2 = points_cm[name2]
                dist = math.dist(p1, p2)
                sample[label] = round(dist, 3)
            else:
                sample[label] = None

        self.measurements.append(sample)
        self.samples_collected += 1
        print(f"[Measurement] Collected sample {self.samples_collected}/{self.max_samples}")

        if self.samples_collected >= self.max_samples:
            self.save_to_csv()
            self.is_active = False
            print("[Measurement] Collection finished. CSV saved.")

    def save_to_csv(self):
        if not self.measurements:
            return

        timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        filename = f"skeleton_measurements_{timestamp}.csv"
        filepath = os.path.join(self.output_dir, filename)

        fieldnames = ["sample_id"] + [label for _, _, label in SEGMENTS]

        with open(filepath, 'w', newline='', encoding='utf-8') as csvfile:
            writer = csv.DictWriter(csvfile, fieldnames=fieldnames)
            writer.writeheader()
            writer.writerows(self.measurements)

        print(f"[Measurement] Data saved to {filepath}")

    def start(self):
        self.frame_counter = 0
        self.samples_collected = 0
        self.measurements = []
        self.is_active = True
        print(f"[Measurement] Started collection: {self.max_samples} samples, every {self.sample_interval} frames.")

    def stop(self):
        self.is_active = False
        if self.measurements:
            self.save_to_csv()
        print("[Measurement] Collection stopped.")