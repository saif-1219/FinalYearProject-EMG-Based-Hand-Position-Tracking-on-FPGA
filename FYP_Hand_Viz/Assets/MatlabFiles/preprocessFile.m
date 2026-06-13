[emg, glove_norm] = preprocess_data(emg, glove);

save('processed_data.mat', 'emg', 'glove_norm', 'glove');