import math


class PoseSmoother:

    def __init__(self, alpha=0.2):
        self.alpha = alpha
        self.prev = None
        self.visibility_threshold = 0.7

    def smooth(self, joints):
        if self.prev is None:
            self.prev = [(j.x, j.y, j.z, j.visibility) for j in joints]
            return self.prev

        result = []

        for prev, curr in zip(self.prev, joints):
            if curr.visibility < self.visibility_threshold:
                result.append(prev)
                continue
        
            self.calc_alpha(curr, prev)

            x = prev[0] * (1 - self.alpha) + curr.x * self.alpha
            y = prev[1] * (1 - self.alpha) + curr.y * self.alpha
            z = prev[2] * (1 - self.alpha) + curr.z * self.alpha

            result.append((x, y, z, curr.visibility))

        self.prev = result
        return result
    
    def calc_alpha(self, curr, prev):
        velocity = self.distance(curr, prev)

        if velocity < 0.01:
            self.alpha = 0.1
        elif velocity < 0.05:
            self.alpha = 0.3
        else:
            self.alpha = 0.8
        return self.alpha
    
    def distance(self, a, b):
        return math.sqrt(
            (a.x - b[0]) ** 2 +
            (a.y - b[1]) ** 2 +
            (a.z - b[2]) ** 2
        )