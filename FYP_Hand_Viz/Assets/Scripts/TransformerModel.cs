// using UnityEngine;
// using Unity.Sentis;
// using System.Net.Sockets;
// using System.Text;
// using System.Net;
// using System;
// using System.IO;

// public class EMGInferenceProvider : MonoBehaviour
// {
//     [Header("Model Settings")]
//     public ModelAsset emgModelAsset;
//     public const int receivedSequenceLength = 401; // MATLAB sends 401 time steps
//     public const int modelSequenceLength = 400; // The model needs 400 time steps
//     public const int inputChannels = 12;

//     [Header("UDP Settings")]
//     public int port = 8051;

//     [Header("Output Settings")]
//     public bool convertRadiansToDegrees = false;
    
//     // PUBLIC PROPERTY
//     public float[] PredictedJointAngles { get; private set; }
//     public float[] ActualJointAngles { get; private set; }

//     // Private vars for Sentis
//     private Model runtimeModel;
//     private Worker worker;
//     private Tensor<float> inputTensor;
    
//     // UDP & Data Buffering
//     private UdpClient udpClient;
//     private IPEndPoint remoteEndPoint;
    
//     // This buffer holds the full 401x12 (4800 floats) history required by the model
//     private float[] rollingBuffer;
    
//     // File output
//     private StreamWriter outputWriter;
//     private StreamWriter actualAnglesWriter; 

//     void Start()
//     {
//         // 1. Initialize Sentis
//         runtimeModel = ModelLoader.Load(emgModelAsset);
//         worker = new Worker(runtimeModel, BackendType.GPUCompute);

//         // 2. Initialize the Rolling Buffer
//         // Size = 401 rows * 12 columns = 4812 floats (to receive from MATLAB)
//         int totalBufferSize = receivedSequenceLength * inputChannels;
//         rollingBuffer = new float[totalBufferSize];

//         // 3. Initialize UDP
//         try 
//         {
//             udpClient = new UdpClient(port);
//             // IPAddress.Any allows listening to localhost or external devices
//             remoteEndPoint = new IPEndPoint(IPAddress.Any, 0); 
//             Debug.Log($"UDP Listener started on port {port}. Buffer size: {totalBufferSize}");
//         }
//         catch (Exception e)
//         {
//             Debug.LogError($"UDP Start Error: {e.Message}");
//         }

//         // 4. Initialize output file writer
//         try
//         {
//             outputWriter = new StreamWriter("predicted_angles.txt", false) { AutoFlush = true };
//             string header = "Frame";
//             for (int i = 0; i < inputChannels; i++)
//             {
//                 header += ",Joint_" + i;
//             }
//             outputWriter.WriteLine(header);
//             Debug.Log("Output file writer initialized: predicted_angles.txt");
//         }
//         catch (Exception e)
//         {
//             Debug.LogError($"File Writer Start Error: {e.Message}");
//         }

//         // 5. Initialize actual angles file writer
//         try
//         {
//             actualAnglesWriter = new StreamWriter("actual_angles.txt", false) { AutoFlush = true };
//             string header = "Frame";
//             for (int i = 0; i < inputChannels; i++)
//             {
//                 header += ",Joint_" + i;
//             }
//             actualAnglesWriter.WriteLine(header);
//             Debug.Log("Actual angles file writer initialized: actual_angles.txt");
//         }
//         catch (Exception e)
//         {
//             Debug.LogError($"Actual Angles File Writer Start Error: {e.Message}");
//         }
//     }

//     void Update()
//     {
//         // 1. Check for new UDP data and update the buffer
//         ProcessIncomingUDP();

//         // 2. Extract the model input (first 400x12) and actual joint angles (last 1x12)
//         float[] modelInputData = ExtractModelInput();
//         ExtractActualJointAngles();

//         // Write actual joint angles to file
//         if (actualAnglesWriter != null && ActualJointAngles != null)
//         {
//             actualAnglesWriter.WriteLine(Time.frameCount + "," + string.Join(",", System.Array.ConvertAll(ActualJointAngles, f => f.ToString("F6"))));
//         }

//         // 3. Run Inference
//         TensorShape shape = new TensorShape(1, inputChannels, modelSequenceLength);
        
//         inputTensor?.Dispose();
        
//         // Create tensor safely
//         inputTensor = new Tensor<float>(shape, modelInputData);

//         worker.Schedule(inputTensor);

//         // 4. Get Output
//         var outputTensor = worker.PeekOutput() as Tensor<float>;

//         if (outputTensor != null)
//         {
//             float[] rawAngles = outputTensor.DownloadToArray();

//             // Process
//             PredictedJointAngles = ProcessAngles(rawAngles);
//             //printing for debug
//             // string anglesStr = string.Join(", ", PredictedJointAngles);
//             // Debug.Log("Current Joint Angles: " + anglesStr);

//             // Write predicted joint angles to file
//             if (outputWriter != null)
//             {
//                 outputWriter.WriteLine(Time.frameCount + "," + string.Join(",", System.Array.ConvertAll(PredictedJointAngles, f => f.ToString("F6"))));
//             }
            
//             outputTensor.Dispose();


//         }
//     }

//     // --- UPDATED FUNCTION ---
//     // Extracts the first 400x12 rows from the 401x12 buffer for model input
//     float[] ExtractModelInput()
//     {
//         int modelInputSize = modelSequenceLength * inputChannels;
//         float[] modelInput = new float[modelInputSize];
//         Array.Copy(rollingBuffer, 0, modelInput, 0, modelInputSize);
//         // Debug.Log("modelSequenceLength: " + modelSequenceLength);
//         // Debug.Log("inputChannels: " + inputChannels);
//         // Debug.Log("modelInputSize: " + modelInputSize);
//         // Debug.Log("Model Input Shape: From Function: " + modelInput.Length);

//         return modelInput;
//         // return rollingBuffer;
//     }

//     // --- NEW FUNCTION ---
//     // Extracts the last 1x12 row from the 401x12 buffer as actual joint angles
//     void ExtractActualJointAngles()
//     {
//         ActualJointAngles = new float[inputChannels];
//         int startIndex = modelSequenceLength * inputChannels; // Start of last row
//         Array.Copy(rollingBuffer, startIndex, ActualJointAngles, 0, inputChannels);
//     }

//     // Instead of random numbers, this returns the actual UDP buffer.
//     float[] GetEMGInput()
//     {
//         // Returns the current state of the rolling buffer.
//         // The buffer is updated separately in ProcessIncomingUDP to prevent blocking.
//         return rollingBuffer;
//     }

//     // --- HELPER: Handles UDP Reception & Buffer Shifting ---
//     void ProcessIncomingUDP()
//     {
//         if (udpClient == null)
//         {
//             Debug.LogWarning("UDP Client not initialized.");
//             return;
//         } 

//         // Loop to drain the queue (in case MATLAB sends faster than Unity updates)
//         while (udpClient.Available > 0)
//         {
//             try 
//             {
//                 // A. Receive Raw Bytes
//                 byte[] data = udpClient.Receive(ref remoteEndPoint);
//                 string jsonString = Encoding.UTF8.GetString(data);

//                 // B. Parse JSON string to Float Array
//                 // MATLAB sends data like "[[0.1, 0.2...], [0.3...]]"
//                 // We strip brackets and split by comma to get a flat list.
//                 string cleanString = jsonString.Replace("[", "").Replace("]", "").Replace("\"", "");
//                 string[] numberStrings = cleanString.Split(',');

//                 int receivedCount = numberStrings.Length;
//                 float[] newData = new float[receivedCount];

//                 for(int i = 0; i < receivedCount; i++)
//                 {
//                     if (float.TryParse(numberStrings[i], out float val))
//                     {
//                         newData[i] = val;
//                     }
//                 }

//                 // C. Update Rolling Buffer (Sliding Window)
//                 // If we receive 100 new rows (1200 floats), we must:
//                 // 1. Shift the existing data to the LEFT by 1200 spots.
//                 // 2. Append the NEW data at the END.

//                 int totalSize = rollingBuffer.Length;
                
//                 // Safety check: Don't crash if packet is huge
//                 if (receivedCount > totalSize) 
//                 {
//                     // If packet is larger than buffer, just take the last part of the packet
//                     Array.Copy(newData, receivedCount - totalSize, rollingBuffer, 0, totalSize);
//                 }
//                 else
//                 {
//                     // 1. Shift old data left
//                     int amountToShift = totalSize - receivedCount;
//                     Array.Copy(rollingBuffer, receivedCount, rollingBuffer, 0, amountToShift);

//                     // 2. Paste new data at the end
//                     Array.Copy(newData, 0, rollingBuffer, amountToShift, receivedCount);
//                 }

//                 // Printing the processed data received for debug
//                 // Debug.Log("UDP Received Data: " + string.Join(", ", newData));


//             }
//             catch (Exception e)
//             {
//                 Debug.LogError($"UDP Parse Error: {e.Message}");
//             }
//         }
//     }

//     float[] ProcessAngles(float[] rawData)
//     {
//         if (!convertRadiansToDegrees) return rawData;
//         float[] processed = new float[rawData.Length];
//         for (int i = 0; i < rawData.Length; i++)
//         {
//             processed[i] = rawData[i] * Mathf.Rad2Deg;
//         }
//         return processed;
//     }

//     private void OnDisable()
//     {
//         worker?.Dispose();
//         inputTensor?.Dispose();
//         udpClient?.Close();
//         outputWriter?.Close();
//         outputWriter?.Dispose();
//         actualAnglesWriter?.Close();
//         actualAnglesWriter?.Dispose();
//     }
// }


//////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////

// using UnityEngine;
// using Unity.Sentis;
// using System.Net.Sockets;
// using System.Text;
// using System.Net;
// using System;
// using System.IO;

// public class EMGInferenceProvider : MonoBehaviour
// {
//     [Header("Model Settings")]
//     public ModelAsset emgModelAsset;
//     public const int receivedSequenceLength = 401; 
//     public const int modelSequenceLength = 400; 
//     public const int inputChannels = 12;
//     public const int outputAnglesCount = 10; 

//     [Header("UDP Settings")]
//     public int port = 8051;

//     [Header("Output Settings")]
//     public bool convertRadiansToDegrees = false;
//     public float scaleFactor = 100f; 
    
//     // PUBLIC PROPERTY
//     public float[] PredictedJointAngles { get; private set; }
//     public float[] ActualJointAngles { get; private set; }

//     // Private vars for Sentis
//     private Model runtimeModel;
//     private Worker worker;
//     private Tensor<float> inputTensor;

//     // UDP & Data Buffering
//     private UdpClient udpClient;
//     private IPEndPoint remoteEndPoint;
//     private float[] rollingBuffer;
    
//     // File output
//     private StreamWriter outputWriter;
//     private StreamWriter actualAnglesWriter; 

//     void Start()
//     {
//         runtimeModel = ModelLoader.Load(emgModelAsset);
//         worker = new Worker(runtimeModel, BackendType.GPUCompute);

//         int totalBufferSize = receivedSequenceLength * inputChannels;
//         rollingBuffer = new float[totalBufferSize];
        
//         PredictedJointAngles = new float[outputAnglesCount];

//         try 
//         {
//             udpClient = new UdpClient(port);
//             // Non-blocking mode is handled by udpClient.Available, but setting a timeout is a good fallback
//             udpClient.Client.ReceiveTimeout = 10; 
//             remoteEndPoint = new IPEndPoint(IPAddress.Any, 0); 
//             Debug.Log($"UDP Listener started on port {port}.");
//         }
//         catch (Exception e) { Debug.LogError($"UDP Start Error: {e.Message}"); }

//         outputWriter = new StreamWriter("predicted_angles.txt", false) { AutoFlush = true };
//         actualAnglesWriter = new StreamWriter("actual_angles.txt", false) { AutoFlush = true };
//     }

//     void Update()
//     {
//         // 1. Check if we actually received new UDP data this frame
//         bool hasNewData = ProcessIncomingUDP();

//         // 2. If no new data arrived, STOP and zero out the movement
//         if (!hasNewData)
//         {
//             for (int i = 0; i < outputAnglesCount; i++)
//             {
//                 PredictedJointAngles[i] = 0f; // Hand stops moving
//             }
//             return; // Skip Sentis inference entirely to save GPU power
//         }

//         // 3. If we DO have new data, run the normal inference pipeline
//         float[] modelInputData = ExtractModelInput();
//         ExtractActualJointAngles();

//         if (actualAnglesWriter != null && ActualJointAngles != null)
//         {
//             actualAnglesWriter.WriteLine(Time.frameCount + "," + string.Join(",", System.Array.ConvertAll(ActualJointAngles, f => f.ToString("F6"))));
//         }

//         TensorShape shape = new TensorShape(1, inputChannels, modelSequenceLength);
//         inputTensor?.Dispose();
//         inputTensor = new Tensor<float>(shape, TransposeForSentis(modelInputData));

//         worker.Schedule(inputTensor);

//         var outputTensor = worker.PeekOutput() as Tensor<float>;

//         if (outputTensor != null)
//         {
//             float[] rawDifferences = outputTensor.DownloadToArray();

//             for (int i = 0; i < outputAnglesCount; i++)
//             {
//                 float scaledDiff = rawDifferences[i] / scaleFactor;
//                 PredictedJointAngles[i] = scaledDiff;
//             }

//             PredictedJointAngles = ProcessAngles(PredictedJointAngles);

//             if (outputWriter != null)
//             {
//                 outputWriter.WriteLine(Time.frameCount + "," + string.Join(",", System.Array.ConvertAll(PredictedJointAngles, f => f.ToString("F6"))));
//             }
//         }
//     }

//     // --- CHANGED: Now returns a boolean (True if data was received, False if empty) ---
//     bool ProcessIncomingUDP()
//     {
//         if (udpClient == null) return false;

//         bool gotNewData = false;

//         while (udpClient.Available > 0)
//         {
//             try 
//             {
//                 gotNewData = true;

//                 // 1. Receive the raw binary bytes from Python
//                 byte[] data = udpClient.Receive(ref remoteEndPoint);

//                 // 2. Calculate how many floats we received (4 bytes per float)
//                 int receivedCount = data.Length / 4;
//                 float[] newData = new float[receivedCount];

//                 // 3. Instantly map the bytes into our float array (Ultra-fast, 0 string allocation!)
//                 Buffer.BlockCopy(data, 0, newData, 0, data.Length);

//                 // 4. Update the Rolling Buffer (Same logic as before)
//                 int totalSize = rollingBuffer.Length;
                
//                 if (receivedCount > totalSize) 
//                 {
//                     Array.Copy(newData, receivedCount - totalSize, rollingBuffer, 0, totalSize);
//                 }
//                 else
//                 {
//                     int amountToShift = totalSize - receivedCount;
//                     Array.Copy(rollingBuffer, receivedCount, rollingBuffer, 0, amountToShift);
//                     Array.Copy(newData, 0, rollingBuffer, amountToShift, receivedCount);
//                 }
//             }
//             catch (Exception e) { Debug.LogError($"UDP Parse Error: {e.Message}"); }
//         }

//         return gotNewData;
//     }

//     float[] TransposeForSentis(float[] flatBuffer)
//     {
//         float[] transposed = new float[flatBuffer.Length];
//         for (int t = 0; t < modelSequenceLength; t++)
//         {
//             for (int c = 0; c < inputChannels; c++)
//             {
//                 transposed[c * modelSequenceLength + t] = flatBuffer[t * inputChannels + c];
//             }
//         }
//         return transposed;
//     }

//     float[] ExtractModelInput()
//     {
//         int modelInputSize = modelSequenceLength * inputChannels;
//         float[] modelInput = new float[modelInputSize];
//         Array.Copy(rollingBuffer, 0, modelInput, 0, modelInputSize);
//         return modelInput;
//     }

//     void ExtractActualJointAngles()
//     {
//         ActualJointAngles = new float[inputChannels]; 
//         int startIndex = modelSequenceLength * inputChannels; 
//         Array.Copy(rollingBuffer, startIndex, ActualJointAngles, 0, inputChannels);
//     }

//     float[] ProcessAngles(float[] rawData)
//     {
//         if (!convertRadiansToDegrees) return rawData;
//         float[] processed = new float[rawData.Length];
//         for (int i = 0; i < rawData.Length; i++)
//         {
//             processed[i] = rawData[i] * Mathf.Rad2Deg;
//         }
//         return processed;
//     }

//     private void OnDisable()
//     {
//         worker?.Dispose();
//         inputTensor?.Dispose();
//         udpClient?.Close();
//         outputWriter?.Close();
//         actualAnglesWriter?.Close();
//     }
// }





using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System;
using System.IO;

public class EMGInferenceProvider : MonoBehaviour
{
    [Header("UDP Settings")]
    public int port = 8051;

    [Header("Output Settings")]
    public bool convertRadiansToDegrees = false;
    public const int outputAnglesCount = 10; 

    // PUBLIC PROPERTIES (Other scripts read these to move the hands)
    public float[] PredictedJointAngles { get; private set; }
    public float[] ActualJointAngles { get; private set; }

    // UDP
    private UdpClient udpClient;
    private IPEndPoint remoteEndPoint;
    
    // File output
    private StreamWriter outputWriter;
    private StreamWriter actualAnglesWriter; 

    void Start()
    {
        PredictedJointAngles = new float[outputAnglesCount];
        ActualJointAngles = new float[outputAnglesCount];

        try 
        {
            udpClient = new UdpClient(port);
            udpClient.Client.ReceiveTimeout = 10; 
            remoteEndPoint = new IPEndPoint(IPAddress.Any, 0); 
            Debug.Log($"UDP Listener started on port {port}. Waiting for Python pipeline...");
        }
        catch (Exception e) { Debug.LogError($"UDP Start Error: {e.Message}"); }

        outputWriter = new StreamWriter("predicted_angles.txt", false) { AutoFlush = true };
        actualAnglesWriter = new StreamWriter("actual_angles.txt", false) { AutoFlush = true };
    }

    void Update()
    {
        // 1. Check if we received new UDP data this frame
        bool hasNewData = ProcessIncomingUDP();

        // 2. If no new data arrived, zero out the velocities so the hands stop moving
        if (!hasNewData)
        {
            // for (int i = 0; i < outputAnglesCount; i++)
            // {
            //     PredictedJointAngles[i] = 0f;
            //     ActualJointAngles[i] = 0f;
            // }
            return; 
        }

        // 3. Log the newly received data to text files
        if (actualAnglesWriter != null && ActualJointAngles != null)
        {
            actualAnglesWriter.WriteLine(Time.frameCount + "," + string.Join(",", Array.ConvertAll(ActualJointAngles, f => f.ToString("F6"))));
        }
        
        if (outputWriter != null && PredictedJointAngles != null)
        {
            outputWriter.WriteLine(Time.frameCount + "," + string.Join(",", Array.ConvertAll(PredictedJointAngles, f => f.ToString("F6"))));
        }

        // Debug.Log("Updated Joint Angles - Actual: " + string.Join(", ", ActualJointAngles) + " | Predicted: " + string.Join(", ", PredictedJointAngles));
    }

    bool ProcessIncomingUDP()
    {
        if (udpClient == null) return false;

        bool gotNewData = false;

        while (udpClient.Available > 0)
        {
            try 
            {
                gotNewData = true;

                // 1. Receive the raw binary bytes from Python (Expected: 96 bytes)
                byte[] data = udpClient.Receive(ref remoteEndPoint);

                // 2. Calculate float count (4 bytes per float -> expects 24 floats)
                int receivedCount = data.Length / 4;
                
                if (receivedCount < 24) continue; // Safety check

                float[] newData = new float[receivedCount];
                Buffer.BlockCopy(data, 0, newData, 0, data.Length);

                // 3. Extract Ground Truth and Predictions
                // Because Python pads the 10 joints to 12...
                // Ground Truth is at indices 0-9
                // Predictions are at indices 12-21
                for (int i = 0; i < outputAnglesCount; i++)
                {
                    float gt = newData[i];
                    float pred = newData[12 + i];

                    if (convertRadiansToDegrees)
                    {
                        gt *= Mathf.Rad2Deg;
                        pred *= Mathf.Rad2Deg;
                    }

                    ActualJointAngles[i] = gt;
                    PredictedJointAngles[i] = pred;
                }
                
            }
            catch (Exception e) { Debug.LogError($"UDP Parse Error: {e.Message}"); }
        }

        return gotNewData;
    }

    private void OnDisable()
    {
        udpClient?.Close();
        outputWriter?.Close();
        actualAnglesWriter?.Close();
    }
}