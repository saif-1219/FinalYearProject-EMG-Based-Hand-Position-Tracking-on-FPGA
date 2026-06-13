// using UnityEngine;
// using System.Net.Sockets;
// using System.Text;
// using System.Net;

// public class JointReceiver : MonoBehaviour
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
//         CacheHandTransforms();

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



//     void CacheHandTransforms()
//     {
//         // Helper to safely find transforms without crashing if one is missing
//         Transform FindT(string path) => GameObject.Find(path)?.transform;

//         string basePath = "HandManager (1)/Camera Offset/RightHand/R_Wrist/";

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


//////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////


// using UnityEngine;

// public class JointReceiver : MonoBehaviour
// {
//     [Header("Dependencies")]
//     public EMGInferenceProvider transformerModel;

//     [Header("Integration Settings")]
//     [Tooltip("The minimum physical limit of the joint (usually 0)")]
//     public float minAngleLimit = 0f;
//     [Tooltip("The maximum physical limit of the joint. If your model outputs radians, this might be 1.57. If normalized, 1.0.")]
//     public float maxAngleLimit = 1.57f; // Set to 1.0f if your data was strictly normalized [0, 1]
    
//     [Tooltip("Check this if the model outputs Radians and Unity needs Degrees.")]
//     public bool convertToDegrees = true;

//     [Header("Hand Transforms")]
//     private Transform rIndexProximal, rIndexIntermediate, rIndexDistal;
//     private Transform rMiddleProximal, rMiddleIntermediate, rMiddleDistal;
//     private Transform rRingProximal, rRingIntermediate, rRingDistal;
//     private Transform rLittleProximal, rLittleIntermediate, rLittleDistal;
//     private Transform rThumbProximal, rThumbDistal;

//     // --- NEW: The "Memory" of the hand ---
//     // This array holds the integrated, absolute positions of the joints
//     private float[] currentAbsoluteAngles = new float[10];

//     void Start()
//     {
//         CacheHandTransforms();

//         if (transformerModel == null)
//         {
//             transformerModel = FindObjectOfType<EMGInferenceProvider>();
//             if (transformerModel == null)
//             {
//                 Debug.LogError("EMGInferenceProvider not found in scene!");
//             }
//         }
        
//         // Thumb (CyberGlove Indices 1 and 2)
//         currentAbsoluteAngles[0] = 0.2924528f; // rThumbProximal
//         currentAbsoluteAngles[1] = 0.4025975f; // rThumbDistal
        
//         // Index Finger (CyberGlove Indices 4 and 5)
//         currentAbsoluteAngles[2] = 0.6407767f; // rIndexProximal
//         currentAbsoluteAngles[3] = 0.3757226f; // rIndexIntermediate
        
//         // Middle Finger (CyberGlove Indices 7 and 8)
//         currentAbsoluteAngles[4] = 0.5086207f; // rMiddleProximal
//         currentAbsoluteAngles[5] = 0.4050635f; // rMiddleIntermediate
        
//         // Ring Finger (CyberGlove Indices 10 and 11)
//         currentAbsoluteAngles[6] = 0.7184466f; // rRingProximal
//         currentAbsoluteAngles[7] = 0.4444444f; // rRingIntermediate
        
//         // Little Finger (CyberGlove Indices 13 and 14)
//         currentAbsoluteAngles[8] = 0.0853483f; // rLittleProximal
//         currentAbsoluteAngles[9] = 0.6797386f; // rLittleIntermediate
//     }

//     void Update()
//     {
//         // We now pull the PREDICTED differences from the model
//         if (transformerModel != null && transformerModel.PredictedJointAngles != null)
//         {
//             float[] diffs = transformerModel.PredictedJointAngles;
            
//             // Safety check
//             if (diffs.Length >= 10)
//             {
//                 // Debug.Log("Predicted Angle differences: " + string.Join(", ", diffs));
//                 IntegrateAndMoveHand(diffs);
//             }
//         }
//     }

//     void IntegrateAndMoveHand(float[] diffs)
//     {
//         // 1. INTEGRATE & CLAMP
//         for (int i = 0; i < 10; i++)
//         {
//             // Add the velocity difference to the current position
//             currentAbsoluteAngles[i] += diffs[i];
            
//             // Clamp it so the fingers don't break physical boundaries
//             currentAbsoluteAngles[i] = Mathf.Clamp(currentAbsoluteAngles[i], minAngleLimit, maxAngleLimit);
//         }
//         Debug.Log("Integrated predicted Absolute Angles: " + string.Join(", ", currentAbsoluteAngles));
//         // 2. APPLY ROTATIONS
//         // We apply the rotation based on the integrated 'currentAbsoluteAngles' array
        
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

//     void CacheHandTransforms()
//     {
//         Transform FindT(string path) => GameObject.Find(path)?.transform;

//         string basePath = "HandManager (1)/Camera Offset/RightHand/R_Wrist/";

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

public class JointReceiver : MonoBehaviour
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
        CacheHandTransforms();

        if (transformerModel == null)
        {
            transformerModel = FindObjectOfType<EMGInferenceProvider>();
            if (transformerModel == null)
            {
                Debug.LogError("EMGInferenceProvider not found in scene!");
            }
        }
        
        // NOTE: Hardcoded integration memory and starting poses have been removed.
        // Python handles all absolute positioning now!
    }

    void Update()
    {
        // We now pull the ABSOLUTE PREDICTED angles directly from the model
        if (transformerModel != null && transformerModel.PredictedJointAngles != null)
        {
            float[] angles = new float[10];
            System.Array.Copy(transformerModel.PredictedJointAngles, 0, angles, 0, 10);
            
            UpdateHandPoseFromAngles(angles);
        }
    }

    // --- UPDATED: Directly applies the incoming absolute angles ---
    void UpdateHandPoseFromAngles(float[] angles)
    {
        if (angles.Length < 10) return; 

        // APPLY ROTATIONS
        float multiplier = convertToDegrees ? Mathf.Rad2Deg : 1f;
        // Debug.Log("Applying Absolute Predicted Joint Angles: " + string.Join(", ", angles));
        // Thumb
        if (rThumbProximal){
             rThumbProximal.localRotation = Quaternion.Euler(angles[0] * multiplier, 0, 0);
             Debug.Log("Thumb Proximal Angle Applied: " + angles[0] * multiplier);
        }
        if (rThumbDistal) {
            rThumbDistal.localRotation = Quaternion.Euler(angles[1] * multiplier, 0, 0);
            Debug.Log("Thumb Distal Angle Applied: " + angles[1] * multiplier);
        }

        // Index Finger
        if (rIndexProximal) {
            rIndexProximal.localRotation = Quaternion.Euler(angles[2] * multiplier, 0, 0);
            Debug.Log("Index Proximal Angle Applied: " + angles[2] * multiplier);
        }
        if (rIndexIntermediate) {
            rIndexIntermediate.localRotation = Quaternion.Euler(angles[3] * multiplier, 0, 0);
            Debug.Log("Index Intermediate Angle Applied: " + angles[3] * multiplier);
        }

        // Middle Finger
        if (rMiddleProximal) {
            rMiddleProximal.localRotation = Quaternion.Euler(angles[4] * multiplier, 0, 0);
            Debug.Log("Middle Proximal Angle Applied: " + angles[4] * multiplier);
        }
        if (rMiddleIntermediate) {
            rMiddleIntermediate.localRotation = Quaternion.Euler(angles[5] * multiplier, 0, 0);
            Debug.Log("Middle Intermediate Angle Applied: " + angles[5] * multiplier);
        }

        // Ring Finger
        if (rRingProximal) {
            rRingProximal.localRotation = Quaternion.Euler(angles[6] * multiplier, 0, 0);
            Debug.Log("Ring Proximal Angle Applied: " + angles[6] * multiplier);
        }
        if (rRingIntermediate) {
            rRingIntermediate.localRotation = Quaternion.Euler(angles[7] * multiplier, 0, 0);
            Debug.Log("Ring Intermediate Angle Applied: " + angles[7] * multiplier);
        }

        // Little Finger
        if (rLittleProximal) {
            rLittleProximal.localRotation = Quaternion.Euler(angles[8] * multiplier, 0, 0);
            Debug.Log("Little Proximal Angle Applied: " + angles[8] * multiplier);
        }
        if (rLittleIntermediate) {
            rLittleIntermediate.localRotation = Quaternion.Euler(angles[9] * multiplier, 0, 0);
            Debug.Log("Little Intermediate Angle Applied: " + angles[9] * multiplier);
        }
    }

    void CacheHandTransforms()
    {
        Transform FindT(string path) => GameObject.Find(path)?.transform;

        // Keeps your specific path for the predicted hand (HandManager (1))
        string basePath = "HandManager (1)/Camera Offset/RightHand/R_Wrist/";

        rIndexProximal = FindT(basePath + "R_IndexMetacarpal/R_IndexProximal");
        rIndexIntermediate = FindT(basePath + "R_IndexMetacarpal/R_IndexProximal/R_IndexIntermediate");
        rIndexDistal = FindT(basePath + "R_IndexMetacarpal/R_IndexProximal/R_IndexIntermediate/R_IndexDistal");

        rMiddleProximal = FindT(basePath + "R_MiddleMetacarpal/R_MiddleProximal");
        rMiddleIntermediate = FindT(basePath + "R_MiddleMetacarpal/R_MiddleProximal/R_MiddleIntermediate");
        rMiddleDistal = FindT(basePath + "R_MiddleMetacarpal/R_MiddleProximal/R_MiddleIntermediate/R_MiddleDistal");

        rRingProximal = FindT(basePath + "R_RingMetacarpal/R_RingProximal");
        rRingIntermediate = FindT(basePath + "R_RingMetacarpal/R_RingProximal/R_RingIntermediate");
        rRingDistal = FindT(basePath + "R_RingMetacarpal/R_RingMetacarpal/R_RingProximal/R_RingIntermediate/R_RingDistal");

        rLittleProximal = FindT(basePath + "R_LittleMetacarpal/R_LittleProximal");
        rLittleIntermediate = FindT(basePath + "R_LittleMetacarpal/R_LittleProximal/R_LittleIntermediate");
        rLittleDistal = FindT(basePath + "R_LittleMetacarpal/R_LittleProximal/R_LittleIntermediate/R_LittleDistal");

        rThumbProximal = FindT(basePath + "R_ThumbMetacarpal/R_ThumbProximal");
        rThumbDistal = FindT(basePath + "R_ThumbMetacarpal/R_ThumbProximal/R_ThumbDistal");
    }
}










