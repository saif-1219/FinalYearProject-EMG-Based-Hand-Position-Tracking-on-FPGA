function filtered_emg = bandpass_butterworth(emg, lowcut, highcut, fs, order)
    % BANDPASS_BUTTERWORTH Apply a Butterworth bandpass filter to EMG data.
    %
    % Inputs:
    %   emg     - [samples x channels] matrix of EMG data
    %   lowcut  - Low frequency cutoff (Hz) (default: 5)
    %   highcut - High frequency cutoff (Hz) (default: 500)
    %   fs      - Sampling rate (Hz) (default: 2000)
    %   order   - Filter order (default: 4)
    %
    % Returns:
    %   filtered_emg - Filtered data with zero phase lag

    % Set defaults if arguments are missing
    if nargin < 5, order = 4; end
    if nargin < 4, fs = 2000; end
    if nargin < 3, highcut = 500; end
    if nargin < 2, lowcut = 5; end

    % 1. Calculate Nyquist frequency
    nyq = 0.5 * fs;

    % 2. Normalize cutoffs
    Wn = [lowcut, highcut] / nyq;

    % 3. Design Butterworth filter
    % We use ZPK (Zero-Pole-Gain) to generate SOS (Second-Order Sections)
    % This matches Python's output='sos' and prevents instability at higher orders
    [z, p, k] = butter(order, Wn, 'bandpass');
    [sos, g] = zp2sos(z, p, k);

    % 4. Apply Zero-Phase Filtering
    % filtfilt automatically handles matrix inputs by filtering down columns (axis 0)
    filtered_emg = filtfilt(sos, g, emg);
end



% Then min max normalization on glove joint data