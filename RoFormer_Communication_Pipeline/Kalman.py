import numpy as np


# ──────────────────────────────────────────────────────────────
# Kalman Smoother  (1-D scalar filter per joint)
# ──────────────────────────────────────────────────────────────
class KalmanSmoother:
    def __init__(self, process_noise=1e-4, measurement_noise=0.05, initial_value=0.0):
        self.Q = process_noise   # "Q": Higher = faster reaction, more noise
        self.R = measurement_noise # "R": Higher = smoother line, more lag
        self.P = 1.0             # Initial uncertainty
        self.x = initial_value   # Initial state estimate
        
    def update(self, measurement):
        # Prediction step
        self.P = self.P + self.Q
        
        # Correction step
        K = self.P / (self.P + self.R) # Kalman Gain
        self.x = self.x + K * (measurement - self.x)
        self.P = (1 - K) * self.P
        return self.x


def apply_kalman_smoothing(predictions, Q=1e-4, R=0.05):
    """
    Applies Kalman smoothing to all joints (columns) in the prediction array.
    
    Args:
        predictions: np.ndarray [N, num_joints]
        Q: process noise
        R: measurement noise
    Returns:
        smoothed_preds: np.ndarray [N, num_joints]
    """
    num_samples, num_joints = predictions.shape
    smoothed_preds = np.zeros_like(predictions)
    
    for j in range(num_joints):
        kf = KalmanSmoother(process_noise=Q, measurement_noise=R, initial_value=predictions[0, j])
        for i in range(num_samples):    
            smoothed_preds[i, j] = kf.update(predictions[i, j])
            
    return smoothed_preds

class AdaptiveKalmanSmoother:
    def __init__(self, process_noise=1e-4, measurement_noise=0.05, initial_value=0.0, alpha=0.1):
        self.Q0 = process_noise
        self.Q = process_noise
        self.R = measurement_noise
        self.P = 1.0
        self.x = initial_value
        self.alpha = alpha
        
    def update(self, measurement):
        self.P = self.P + self.Q
        innovation = measurement - self.x
        self.Q = self.Q0 + self.alpha * (innovation ** 2)
        K = self.P / (self.P + self.R)
        self.x = self.x + K * innovation
        self.P = (1 - K) * self.P
        return self.x

def apply_adaptive_kalman_smoothing(predictions, Q=1e-4, R=0.05, alpha=0.1):
    """
    Applies Adaptive Kalman smoothing to all joints.
    """
    num_samples, num_joints = predictions.shape
    smoothed_preds = np.zeros_like(predictions)
    
    for j in range(num_joints):
        kf = AdaptiveKalmanSmoother(process_noise=Q, measurement_noise=R, initial_value=predictions[0, j], alpha=alpha)
        for i in range(num_samples):
            smoothed_preds[i, j] = kf.update(predictions[i, j])
            
    return smoothed_preds