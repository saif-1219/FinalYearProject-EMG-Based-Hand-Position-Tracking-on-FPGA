%% --- CONFIGURATION ---
unityIP = '127.0.0.1';
% portEMG = 8052;
portGlove = 8051;
windowSize = 400;    % Size of EMG chunk
stride = 50;
gloveStep = 50;     % How many frames to skip for glove_norm (to keep sync with EMG)

% Get data sizes
[totalSamplesEMG, numChannels] = size(emg);
[totalSamplesGlove, numJoints] = size(glove_norm);

% Initialize indices
idx_emg = 1;
idx_glove = 400;

%% --- SETUP UDP ONCE ---
% We only need one UDP object to send to multiple destinations
u = udpport('IPV4');

disp('Starting Combined Stream (EMG + Glove)...');
indexxxx = 1;
%% --- MAIN LOOP ---
while 1 == 1 %indexxxx < 11
    % =========================================================
    % 1. PROCESS EMG DATA (Port 8051)
    
    % Check if we need to loop back EMG
    if (idx_emg + windowSize - 1) > totalSamplesEMG
        idx_emg = 1;
        disp('EMG: Reached end of data. Looping.');
    end

    % Extract 400 rows of EMG
    emgChunk = emg(idx_emg : idx_emg + windowSize - 1, :);
    
    % Send EMG
    % write(u, jsonencode(emgChunk), "string", unityIP, portEMG);

    
    % =========================================================
    % 2. PROCESS GLOVE DATA (Port 8052)
    % =========================================================
    
    % Check if we need to loop back Glove
    % (We use a separate check in case glove_norm data length differs from EMG)
    if (idx_glove + 1) > totalSamplesGlove % Checking +1 because we take single rows
        idx_glove = 1;
        disp('Glove: Reached end of data. Looping.');
    end

    % Extract specific joints for the CURRENT frame
    % Note: We are taking the single row at idx_glove
    i = idx_glove; 
    JointToSend = [ glove_norm(i,2) glove_norm(i,3) glove_norm(i,5) glove_norm(i,6) glove_norm(i,8) glove_norm(i,9) glove_norm(i,12) glove_norm(i,13) glove_norm(i,16) glove_norm(i,17) 0 0];

    combMsgToSend = [emgChunk; JointToSend];

    % Send Glove
    write(u, jsonencode(combMsgToSend), "string", unityIP, portGlove);

    
    % =========================================================
    % 3. UPDATE & PAUSE
    % =========================================================
    
    % Advance indices
    idx_emg = idx_emg + windowSize;
    
    % Advance glove_norm index by same amount to keep sync
    % (Or change 'gloveStep' to 1 if you want slow-motion glove_norm playback)
    idx_glove = idx_glove + windowSize; 
    indexxxx = indexxxx + 1;
    pause(0.1); 
end