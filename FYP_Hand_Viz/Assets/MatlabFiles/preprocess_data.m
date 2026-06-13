function [emg_proc, glove_proc] = preprocess_data(emg, glove, fs, mu)
    % PREPROCESS_DATA Full pipeline for EMG and Glove data
    %
    % Inputs:
    %   emg   - [Samples x Channels] Raw EMG matrix
    %   glove - [Samples x Channels] Raw Glove/Kinematics matrix
    %   fs    - Sampling rate (Hz) (default: 2000)
    %   mu    - Mu-law parameter (default: 255)
    %
    % Returns:
    %   emg_proc   - Bandpassed, Peak-Normalized, and Mu-Law Companded EMG
    %   glove_proc - Min-Max Normalized Glove data [0, 1]

    % --- 0. Set Defaults ---
    if nargin < 4, mu = 255; end
    if nargin < 3, fs = 2000; end
    
    epsilon = 1e-8; % Prevent division by zero

    % --- 1. EMG: Bandpass Filter ---
    % 5-500Hz, 4th order (Standard defaults)
    emg_proc = abs(bandpass_butterworth(emg, 5, 500, fs, 4));

    % --- 2. EMG: Normalize by Max Absolute Value ---
    % Scales data to range [-1, 1] required for Mu-Law
    % max(X, [], 1) operates on columns (channels)
    emg_max_abs = max(abs(emg_proc), [], 1); 
    emg_proc = emg_proc ./ (emg_max_abs + epsilon);

    % --- 3. EMG: Mu-Law Companding ---
    emg_proc = mu_law_normalize(emg_proc, mu);

    % --- 4. Glove: Min-Max Normalization ---
    % Scales data to range [0, 1]
    glove_min = min(glove, [], 1);
    glove_max = max(glove, [], 1);
    
    glove_proc = (glove - glove_min) ./ (glove_max - glove_min + epsilon);
end