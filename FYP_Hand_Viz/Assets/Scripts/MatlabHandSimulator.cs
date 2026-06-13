// using UnityEngine;
// using System.Net.Sockets;
// using System.Text;
// using System.Net;

// public class MatlabHandSimulator : MonoBehaviour
// {
//     UdpClient udpClient;
//     string dataBuffer = "";

//     [Header("Dependencies")]
//     public EMGInferenceProvider emgProvider;

//     [Header("Hand Transforms")]
//     // We cache these in Start() so we don't look them up every single frame (Performance boost)
//     private Transform rIndexProximal, rIndexIntermediate, rIndexDistal;
//     private Transform rMiddleProximal, rMiddleIntermediate, rMiddleDistal;
//     private Transform rRingProximal, rRingIntermediate, rRingDistal;
//     private Transform rLittleProximal, rLittleIntermediate, rLittleDistal;
//     private Transform rThumbProximal, rThumbDistal;
//     // Add wrist if your model outputs it
//     // private Transform rWrist; 

//     void Start()
//     {
//         // 1. Cache Transforms (Find them once)
//         CacheHandTransforms2();

//         // 2. Start UDP
//         // try
//         // {
//         //     udpClient = new UdpClient(8051);
//         //     Debug.Log("UDP client started on port 8051.");
//         // }
//         // catch (SocketException e)
//         // {
//         //     Debug.LogError($"Failed to start UDP client: {e.Message}");
//         //     udpClient = null;
//         // }
//     }

//     void Update()
//     {
//         // --- PRIORITY 1: CHECK EMG MODEL DATA ---
//         if (emgProvider != null && emgProvider.PredictedJointAngles != null)
//         {
//             float[] angles = emgProvider.PredictedJointAngles;

//             // Optional: Debug print to ensure data is changing
//             // Debug.Log($"EMG Angles [0]: {angles[0]}");
//             Debug.Log("EMG Predicted Joint Angles: " + string.Join(", ", angles));



//             // *** THIS IS THE MISSING LINE ***
//             UpdateHandPoseFromAngles(angles);
//         }
//         else
//         {
//             // Only warn if you expect it to be running and it's not
//             Debug.LogWarning("Waiting for EMG Data...");
//         }


//         // --- PRIORITY 2: CHECK UDP (Legacy/Fallback) ---
//         // if (udpClient != null && udpClient.Available > 0)
//         // {
//         //     IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
//         //     byte[] data = udpClient.Receive(ref remoteEP);
//         //     dataBuffer = Encoding.UTF8.GetString(data);
            
//         //     // This uses the old String parsing method
//         //     UpdateHandPoseFromData(dataBuffer);
//         // }
//     }

//     // --- NEW FUNCTION: Handles Float Array from Sentis ---
//     void UpdateHandPoseFromAngles(float[] angles)
//     {
//         // Check if we have enough data points. 
//         // NOTE: Adjust '14' if your model outputs 15 (including wrist) or 12.


//         if (angles.Length < 10) return; 

//         // Apply Rotations. 
//         // NOTE: Check your index mapping! 
//         // I am assuming the model output array starts at Index 0 for IndexProximal.
//         // If your model output [0] is Wrist, shift these indices by +1.

//         // Index Finger
// // 1. Define the Min and Max values for the 10 joints (Order: Thumb to Little)
//         float[] minVals = new float[] { -41.438919f, -38.004795f, -25.831825f, -22.392052f, -27.264063f, -44.533894f, -33.170750f, -17.829218f, -27.456064f, -26.391592f };
//         float[] maxVals = new float[] { 69.342514f, 70.418991f, 73.274292f, 102.168236f, 105.046837f, 156.218216f, 124.607719f, 106.872253f, 100.224236f, 90.333389f };

//         // 2. Un-normalize the angles
//         // Formula: real_angle = normalized_val * (max - min) + min
//         float[] realAngles = new float[10];
//         // float[] realAngles = angles;
//         for (int i = 0; i < 10; i++)
//         {
//             // realAngles[i] = angles[i] * (maxVals[i] - minVals[i]) + minVals[i];
            
//             //converting from radians to degrees
//             realAngles[i] = angles[i] * Mathf.Rad2Deg;
//         }

//         // 3. Apply Rotations (Using realAngles, no Rad2Deg)

//         // Thumb
//         if (rThumbProximal) rThumbProximal.localRotation = Quaternion.Euler(realAngles[0], 0, 0);
//         if (rThumbDistal) rThumbDistal.localRotation = Quaternion.Euler(realAngles[1], 0, 0);

//         // Index Finger
//         if (rIndexProximal) rIndexProximal.localRotation = Quaternion.Euler(realAngles[2], 0, 0);
//         if (rIndexIntermediate) rIndexIntermediate.localRotation = Quaternion.Euler(realAngles[3], 0, 0);
//         // if(rIndexDistal) rIndexDistal.localRotation = Quaternion.Euler(realAngles[3], 0, 0); // Usually follows intermediate

//         // Middle Finger
//         if (rMiddleProximal) rMiddleProximal.localRotation = Quaternion.Euler(realAngles[4], 0, 0);
//         if (rMiddleIntermediate) rMiddleIntermediate.localRotation = Quaternion.Euler(realAngles[5], 0, 0);
//         // if(rMiddleDistal) rMiddleDistal.localRotation = Quaternion.Euler(realAngles[5], 0, 0);

//         // Ring Finger
//         if (rRingProximal) rRingProximal.localRotation = Quaternion.Euler(realAngles[6], 0, 0);
//         if (rRingIntermediate) rRingIntermediate.localRotation = Quaternion.Euler(realAngles[7], 0, 0);
//         // if(rRingDistal) rRingDistal.localRotation = Quaternion.Euler(realAngles[7], 0, 0);

//         // Little Finger
//         if (rLittleProximal) rLittleProximal.localRotation = Quaternion.Euler(realAngles[8], 0, 0);
//         if (rLittleIntermediate) rLittleIntermediate.localRotation = Quaternion.Euler(realAngles[9], 0, 0);
//         // if(rLittleDistal) rLittleDistal.localRotation = Quaternion.Euler(realAngles[9], 0, 0);
//     }

//     // --- OLD FUNCTION: Handles String from UDP ---
//     void UpdateHandPoseFromData(string data)
//     {
//         // (Kept for compatibility with your existing UDP setup)
//         string jsonData = data.Replace("{", "").Replace("}", "").Replace("\"", "").Replace("[", "").Replace("]", "");
//         string[] strAngles = jsonData.Split(',');

//         if (strAngles.Length >= 14)
//         {
//             float[] parsedAngles = new float[strAngles.Length];
//             for(int i=0; i<strAngles.Length; i++) 
//             {
//                 float.TryParse(strAngles[i], out parsedAngles[i]);
//             }

//             // Note: Your UDP logic used indices 1-14. I am passing the parsed array
//             // but we might need to shift inputs if we reuse the function above.
//             // For safety, I'll just map manually here as you did before:
            
//             if(rIndexProximal) rIndexProximal.localRotation = Quaternion.Euler(parsedAngles[1], 0, 0);
//             if(rIndexIntermediate) rIndexIntermediate.localRotation = Quaternion.Euler(parsedAngles[2], 0, 0);
//             if(rIndexDistal) rIndexDistal.localRotation = Quaternion.Euler(parsedAngles[3], 0, 0);
//             // ... (fill in rest if you need UDP backup)
//         }
//     }

//     void CacheHandTransforms2()
//     {
//         // Helper to safely find transforms without crashing if one is missing
//         Transform FindT(string path) => GameObject.Find(path)?.transform;

//         string basePath = "HandManager/Camera Offset/RightHand/R_Wrist/";

//         rIndexProximal = FindT(basePath + "R_IndexMetacarpal/R_IndexProximal");
//         rIndexIntermediate = FindT(basePath + "R_IndexMetacarpal/R_IndexProximal/R_IndexIntermediate");
//         rIndexDistal = FindT(basePath + "R_IndexMetacarpal/R_IndexProximal/R_IndexIntermediate/R_IndexDistal");

//         rMiddleProximal = FindT(basePath + "R_MiddleMetacarpal/R_MiddleProximal");
//         rMiddleIntermediate = FindT(basePath + "R_MiddleMetacarpal/R_MiddleProximal/R_MiddleIntermediate");
//         rMiddleDistal = FindT(basePath + "R_MiddleMetacarpal/R_MiddleProximal/R_MiddleIntermediate/R_MiddleDistal");

//         rRingProximal = FindT(basePath + "R_RingMetacarpal/R_RingProximal");
//         rRingIntermediate = FindT(basePath + "R_RingMetacarpal/R_RingProximal/R_RingIntermediate");
//         rRingDistal = FindT(basePath + "R_RingMetacarpal/R_RingProximal/R_RingIntermediate/R_RingDistal");

//         rLittleProximal = FindT(basePath + "R_LittleMetacarpal/R_LittleProximal");
//         rLittleIntermediate = FindT(basePath + "R_LittleMetacarpal/R_LittleProximal/R_LittleIntermediate");
//         rLittleDistal = FindT(basePath + "R_LittleMetacarpal/R_LittleProximal/R_LittleIntermediate/R_LittleDistal");

//         rThumbProximal = FindT(basePath + "R_ThumbMetacarpal/R_ThumbProximal");
//         rThumbDistal = FindT(basePath + "R_ThumbMetacarpal/R_ThumbProximal/R_ThumbDistal");
//     }

//     void OnApplicationQuit()
//     {
//         udpClient?.Close();
//     }
// }










// using UnityEngine;
// using System.Net.Sockets;
// using System.Text;
// using System.Net;

// public class MatlabHandSimulator : MonoBehaviour
// {
//     [Header("Dependencies")]
//     public EMGInferenceProvider transformerModel; // Reference to the TransformerModel/EMGInferenceProvider

//     [Header("Hand Transforms")]
//     // We cache these in Start() so we don't look them up every single frame (Performance boost)
//     private Transform rIndexProximal, rIndexIntermediate, rIndexDistal;
//     private Transform rMiddleProximal, rMiddleIntermediate, rMiddleDistal;
//     private Transform rRingProximal, rRingIntermediate, rRingDistal;
//     private Transform rLittleProximal, rLittleIntermediate, rLittleDistal;
//     private Transform rThumbProximal, rThumbDistal;
//     // Add wrist if your model outputs it
//     // private Transform rWrist; 

//     void Start()
//     {
//         // 1. Cache Transforms (Find them once)
//         CacheHandTransforms2();

//         // 2. Find TransformerModel reference if not assigned in inspector
//         if (transformerModel == null)
//         {
//             transformerModel = FindObjectOfType<EMGInferenceProvider>();
//             if (transformerModel == null)
//             {
//                 Debug.LogError("EMGInferenceProvider not found in scene!");
//             }
//         }
//     }

//     void Update()
//     {
//         // Get joint angles from the TransformerModel's ActualJointAngles
//         if (transformerModel != null && transformerModel.ActualJointAngles != null)
//         {
//             // Use only the first 10 values (last 2 are 0)
//             float[] angles = new float[10];
//             System.Array.Copy(transformerModel.ActualJointAngles, 0, angles, 0, 10);
//             UpdateHandPoseFromAngles(angles);
//         }
//     }

//     // --- NEW FUNCTION: Handles Float Array from Sentis ---
//     void UpdateHandPoseFromAngles(float[] angles)
//     {
//         // Check if we have enough data points. 
//         // NOTE: Adjust '14' if your model outputs 15 (including wrist) or 12.


//         if (angles.Length < 10) return; 

//         //print the angles
//         Debug.Log("Actual Received Joint Angles: " + string.Join(", ", angles));

//         // Apply Rotations. 
//         // NOTE: Check your index mapping! 
//         // I am assuming the model output array starts at Index 0 for IndexProximal.
//         // If your model output [0] is Wrist, shift these indices by +1.

    
//         // float[] realAngles = angles;
//         for (int i = 0; i < 10; i++)
//         {            
//             //converting from radians to degrees
//             angles[i] = angles[i] * Mathf.Rad2Deg;
//         }

//         // Index Finger
//         if(rIndexProximal) rIndexProximal.localRotation = Quaternion.Euler(angles[2], 0, 0);
//         if(rIndexIntermediate) rIndexIntermediate.localRotation = Quaternion.Euler(angles[3], 0, 0);
//         // if(rIndexDistal) rIndexDistal.localRotation = Quaternion.Euler(angles[2] * Mathf.Rad2Deg, 0, 0);

//         // Middle Finger
//         if(rMiddleProximal) rMiddleProximal.localRotation = Quaternion.Euler(angles[4], 0, 0);
//         if(rMiddleIntermediate) rMiddleIntermediate.localRotation = Quaternion.Euler(angles[5], 0, 0);
//         // if(rMiddleDistal) rMiddleDistal.localRotation = Quaternion.Euler(angles[5] * Mathf.Rad2Deg, 0, 0);

//         // Ring Finger
//         if(rRingProximal) rRingProximal.localRotation = Quaternion.Euler(angles[6], 0, 0);
//         if(rRingIntermediate) rRingIntermediate.localRotation = Quaternion.Euler(angles[7], 0, 0);
//         // if(rRingDistal) rRingDistal.localRotation = Quaternion.Euler(angles[8] * Mathf.Rad2Deg, 0, 0);

//         // Little Finger
//         if(rLittleProximal) rLittleProximal.localRotation = Quaternion.Euler(angles[8], 0, 0);
//         if(rLittleIntermediate) rLittleIntermediate.localRotation = Quaternion.Euler(angles[9], 0, 0);
//         // if(rLittleDistal) rLittleDistal.localRotation = Quaternion.Euler(angles[10] * Mathf.Rad2Deg, 0, 0);

//         // Thumb
//         if(rThumbProximal) rThumbProximal.localRotation = Quaternion.Euler(angles[0], 0, 0);
//         if(rThumbDistal) rThumbDistal.localRotation = Quaternion.Euler(angles[1], 0, 0);
//     }



//     void CacheHandTransforms2()
//     {
//         // Helper to safely find transforms without crashing if one is missing
//         Transform FindT(string path) => GameObject.Find(path)?.transform;

//         string basePath = "HandManager/Camera Offset/RightHand/R_Wrist/";

//         rIndexProximal = FindT(basePath + "R_IndexMetacarpal/R_IndexProximal");
//         rIndexIntermediate = FindT(basePath + "R_IndexMetacarpal/R_IndexProximal/R_IndexIntermediate");
//         rIndexDistal = FindT(basePath + "R_IndexMetacarpal/R_IndexProximal/R_IndexIntermediate/R_IndexDistal");

//         rMiddleProximal = FindT(basePath + "R_MiddleMetacarpal/R_MiddleProximal");
//         rMiddleIntermediate = FindT(basePath + "R_MiddleMetacarpal/R_MiddleProximal/R_MiddleIntermediate");
//         rMiddleDistal = FindT(basePath + "R_MiddleMetacarpal/R_MiddleProximal/R_MiddleIntermediate/R_MiddleDistal");

//         rRingProximal = FindT(basePath + "R_RingMetacarpal/R_RingProximal");
//         rRingIntermediate = FindT(basePath + "R_RingMetacarpal/R_RingProximal/R_RingIntermediate");
//         rRingDistal = FindT(basePath + "R_RingMetacarpal/R_RingProximal/R_RingIntermediate/R_RingDistal");

//         rLittleProximal = FindT(basePath + "R_LittleMetacarpal/R_LittleProximal");
//         rLittleIntermediate = FindT(basePath + "R_LittleMetacarpal/R_LittleProximal/R_LittleIntermediate");
//         rLittleDistal = FindT(basePath + "R_LittleMetacarpal/R_LittleProximal/R_LittleIntermediate/R_LittleDistal");

//         rThumbProximal = FindT(basePath + "R_ThumbMetacarpal/R_ThumbProximal");
//         rThumbDistal = FindT(basePath + "R_ThumbMetacarpal/R_ThumbProximal/R_ThumbDistal");
//     }

//     void OnApplicationQuit()
//     {
//         // No UDP client to close anymore
//     }
// }































// using UnityEngine;

// public class MatlabHandSimulator : MonoBehaviour
// {
//     [Header("Dependencies")]
//     public EMGInferenceProvider transformerModel;

//     [Header("Integration Settings")]
//     [Tooltip("The minimum physical limit of the joint (usually 0)")]
//     public float minAngleLimit = 0f;
//     [Tooltip("The maximum physical limit of the joint. If normalized, 1.0.")]
//     public float maxAngleLimit = 1.0f; 
    
//     [Tooltip("Check this if the data is in Radians/Normalized and Unity needs Degrees.")]
//     public bool convertToDegrees = true;

//     [Header("Hand Transforms")]
//     private Transform rIndexProximal, rIndexIntermediate, rIndexDistal;
//     private Transform rMiddleProximal, rMiddleIntermediate, rMiddleDistal;
//     private Transform rRingProximal, rRingIntermediate, rRingDistal;
//     private Transform rLittleProximal, rLittleIntermediate, rLittleDistal;
//     private Transform rThumbProximal, rThumbDistal;

//     // --- NEW: The "Memory" of the ground-truth hand ---
//     private float[] currentAbsoluteAngles = new float[10];

//     void Start()
//     {
//         CacheHandTransforms2();

//         if (transformerModel == null)
//         {
//             transformerModel = FindObjectOfType<EMGInferenceProvider>();
//             if (transformerModel == null)
//             {
//                 Debug.LogError("EMGInferenceProvider not found in scene!");
//             }
//         }

//         // --- INITIALIZE STARTING POSE ---
//         // We must start the ground truth hand in the exact same pose as the predicted hand
        
//         // Thumb
//         currentAbsoluteAngles[0] = 0.2924528f; 
//         currentAbsoluteAngles[1] = 0.4025975f; 
        
//         // Index Finger
//         currentAbsoluteAngles[2] = 0.6407767f; 
//         currentAbsoluteAngles[3] = 0.3757226f; 
        
//         // Middle Finger
//         currentAbsoluteAngles[4] = 0.5086207f; 
//         currentAbsoluteAngles[5] = 0.4050635f; 
        
//         // Ring Finger
//         currentAbsoluteAngles[6] = 0.7184466f; 
//         currentAbsoluteAngles[7] = 0.4444444f; 
        
//         // Little Finger
//         currentAbsoluteAngles[8] = 0.0853483f; 
//         currentAbsoluteAngles[9] = 0.6797386f; 
//     }

//     void Update()
//     {
//         if (transformerModel != null && transformerModel.ActualJointAngles != null)
//         {
//             float[] diffs = new float[10];
//             System.Array.Copy(transformerModel.ActualJointAngles, 0, diffs, 0, 10);
            
//             // We are now passing the differences to be integrated
//             UpdateHandPoseFromDifferences(diffs);
//         }
//     }

//     // --- RENAMED & UPDATED: Now integrates differences instead of setting absolute angles ---
//     void UpdateHandPoseFromDifferences(float[] diffs)
//     {
//         if (diffs.Length < 10) return; 

//         // 1. INTEGRATE & CLAMP
//         for (int i = 0; i < 10; i++)
//         {
//             // Add the ground-truth velocity difference to the current position
//             currentAbsoluteAngles[i] += diffs[i];
            
//             // Clamp it so the fingers don't break physical boundaries
//             currentAbsoluteAngles[i] = Mathf.Clamp(currentAbsoluteAngles[i], minAngleLimit, maxAngleLimit);
//         }

//         // 2. APPLY ROTATIONS
//         float multiplier = convertToDegrees ? Mathf.Rad2Deg : 1f;

//         // Thumb
//         if (rThumbProximal) rThumbProximal.localRotation = Quaternion.Euler(currentAbsoluteAngles[0] * multiplier, 0, 0);
//         if (rThumbDistal) rThumbDistal.localRotation = Quaternion.Euler(currentAbsoluteAngles[1] * multiplier, 0, 0);

//         // Index Finger
//         if (rIndexProximal) rIndexProximal.localRotation = Quaternion.Euler(currentAbsoluteAngles[2] * multiplier, 0, 0);
//         if (rIndexIntermediate) rIndexIntermediate.localRotation = Quaternion.Euler(currentAbsoluteAngles[3] * multiplier, 0, 0);

//         // Middle Finger
//         if (rMiddleProximal) rMiddleProximal.localRotation = Quaternion.Euler(currentAbsoluteAngles[4] * multiplier, 0, 0);
//         if (rMiddleIntermediate) rMiddleIntermediate.localRotation = Quaternion.Euler(currentAbsoluteAngles[5] * multiplier, 0, 0);

//         // Ring Finger
//         if (rRingProximal) rRingProximal.localRotation = Quaternion.Euler(currentAbsoluteAngles[6] * multiplier, 0, 0);
//         if (rRingIntermediate) rRingIntermediate.localRotation = Quaternion.Euler(currentAbsoluteAngles[7] * multiplier, 0, 0);

//         // Little Finger
//         if (rLittleProximal) rLittleProximal.localRotation = Quaternion.Euler(currentAbsoluteAngles[8] * multiplier, 0, 0);
//         if (rLittleIntermediate) rLittleIntermediate.localRotation = Quaternion.Euler(currentAbsoluteAngles[9] * multiplier, 0, 0);
//     }

//     void CacheHandTransforms2()
//     {
//         Transform FindT(string path) => GameObject.Find(path)?.transform;

//         string basePath = "HandManager/Camera Offset/RightHand/R_Wrist/";

//         rIndexProximal = FindT(basePath + "R_IndexMetacarpal/R_IndexProximal");
//         rIndexIntermediate = FindT(basePath + "R_IndexMetacarpal/R_IndexProximal/R_IndexIntermediate");
//         rIndexDistal = FindT(basePath + "R_IndexMetacarpal/R_IndexProximal/R_IndexIntermediate/R_IndexDistal");

//         rMiddleProximal = FindT(basePath + "R_MiddleMetacarpal/R_MiddleProximal");
//         rMiddleIntermediate = FindT(basePath + "R_MiddleMetacarpal/R_MiddleProximal/R_MiddleIntermediate");
//         rMiddleDistal = FindT(basePath + "R_MiddleMetacarpal/R_MiddleProximal/R_MiddleIntermediate/R_MiddleDistal");

//         rRingProximal = FindT(basePath + "R_RingMetacarpal/R_RingProximal");
//         rRingIntermediate = FindT(basePath + "R_RingMetacarpal/R_RingProximal/R_RingIntermediate");
//         rRingDistal = FindT(basePath + "R_RingMetacarpal/R_RingProximal/R_RingIntermediate/R_RingDistal");

//         rLittleProximal = FindT(basePath + "R_LittleMetacarpal/R_LittleProximal");
//         rLittleIntermediate = FindT(basePath + "R_LittleMetacarpal/R_LittleProximal/R_LittleIntermediate");
//         rLittleDistal = FindT(basePath + "R_LittleMetacarpal/R_LittleProximal/R_LittleIntermediate/R_LittleDistal");

//         rThumbProximal = FindT(basePath + "R_ThumbMetacarpal/R_ThumbProximal");
//         rThumbDistal = FindT(basePath + "R_ThumbMetacarpal/R_ThumbProximal/R_ThumbDistal");
//     }
// }



using UnityEngine;

public class MatlabHandSimulator : MonoBehaviour
{
    [Header("Dependencies")]
    public EMGInferenceProvider transformerModel;

    [Header("Settings")]
    [Tooltip("Check this if the incoming data needs to be converted to Degrees. (Make sure you don't check this if the EMGInferenceProvider is already doing the conversion!)")]
    public bool convertToDegrees = true;

    [Header("Hand Transforms")]
    private Transform rIndexProximal, rIndexIntermediate, rIndexDistal;
    private Transform rMiddleProximal, rMiddleIntermediate, rMiddleDistal;
    private Transform rRingProximal, rRingIntermediate, rRingDistal;
    private Transform rLittleProximal, rLittleIntermediate, rLittleDistal;
    private Transform rThumbProximal, rThumbDistal;

    void Start()
    {
        CacheHandTransforms2();

        if (transformerModel == null)
        {
            transformerModel = FindObjectOfType<EMGInferenceProvider>();
            if (transformerModel == null)
            {
                Debug.LogError("EMGInferenceProvider not found in scene!");
            }
        }
        
        // NOTE: The hardcoded starting pose has been removed! 
        // Python now dictates the exact starting pose on the very first frame.
    }

    void Update()
    {
        // Get absolute joint angles from the updated EMGInferenceProvider
        if (transformerModel != null && transformerModel.ActualJointAngles != null)
        {
            float[] angles = new float[10];
            System.Array.Copy(transformerModel.ActualJointAngles, 0, angles, 0, 10);
            
            UpdateHandPoseFromAngles(angles);
        }
    }

    // --- UPDATED: Now simply applies absolute angles directly to the joints ---
    void UpdateHandPoseFromAngles(float[] angles)
    {
        if (angles.Length < 10) return; 

        // Determine if we need to convert to degrees
        float multiplier = convertToDegrees ? Mathf.Rad2Deg : 1f;

        // Thumb
        if (rThumbProximal) rThumbProximal.localRotation = Quaternion.Euler(angles[0] * multiplier, 0, 0);
        if (rThumbDistal) rThumbDistal.localRotation = Quaternion.Euler(angles[1] * multiplier, 0, 0);

        // Index Finger
        if (rIndexProximal) rIndexProximal.localRotation = Quaternion.Euler(angles[2] * multiplier, 0, 0);
        if (rIndexIntermediate) rIndexIntermediate.localRotation = Quaternion.Euler(angles[3] * multiplier, 0, 0);

        // Middle Finger
        if (rMiddleProximal) rMiddleProximal.localRotation = Quaternion.Euler(angles[4] * multiplier, 0, 0);
        if (rMiddleIntermediate) rMiddleIntermediate.localRotation = Quaternion.Euler(angles[5] * multiplier, 0, 0);

        // Ring Finger
        if (rRingProximal) rRingProximal.localRotation = Quaternion.Euler(angles[6] * multiplier, 0, 0);
        if (rRingIntermediate) rRingIntermediate.localRotation = Quaternion.Euler(angles[7] * multiplier, 0, 0);

        // Little Finger
        if (rLittleProximal) rLittleProximal.localRotation = Quaternion.Euler(angles[8] * multiplier, 0, 0);
        if (rLittleIntermediate) rLittleIntermediate.localRotation = Quaternion.Euler(angles[9] * multiplier, 0, 0);
    }

    void CacheHandTransforms2()
    {
        Transform FindT(string path) => GameObject.Find(path)?.transform;

        string basePath = "HandManager/Camera Offset/RightHand/R_Wrist/";

        rIndexProximal = FindT(basePath + "R_IndexMetacarpal/R_IndexProximal");
        rIndexIntermediate = FindT(basePath + "R_IndexMetacarpal/R_IndexProximal/R_IndexIntermediate");
        rIndexDistal = FindT(basePath + "R_IndexMetacarpal/R_IndexProximal/R_IndexIntermediate/R_IndexDistal");

        rMiddleProximal = FindT(basePath + "R_MiddleMetacarpal/R_MiddleProximal");
        rMiddleIntermediate = FindT(basePath + "R_MiddleMetacarpal/R_MiddleProximal/R_MiddleIntermediate");
        rMiddleDistal = FindT(basePath + "R_MiddleMetacarpal/R_MiddleProximal/R_MiddleIntermediate/R_MiddleDistal");

        rRingProximal = FindT(basePath + "R_RingMetacarpal/R_RingProximal");
        rRingIntermediate = FindT(basePath + "R_RingMetacarpal/R_RingProximal/R_RingIntermediate");
        rRingDistal = FindT(basePath + "R_RingMetacarpal/R_RingProximal/R_RingIntermediate/R_RingDistal");

        rLittleProximal = FindT(basePath + "R_LittleMetacarpal/R_LittleProximal");
        rLittleIntermediate = FindT(basePath + "R_LittleMetacarpal/R_LittleProximal/R_LittleIntermediate");
        rLittleDistal = FindT(basePath + "R_LittleMetacarpal/R_LittleProximal/R_LittleIntermediate/R_LittleDistal");

        rThumbProximal = FindT(basePath + "R_ThumbMetacarpal/R_ThumbProximal");
        rThumbDistal = FindT(basePath + "R_ThumbMetacarpal/R_ThumbProximal/R_ThumbDistal");
    }
}