using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine.XR.Hands;
using System;

[Serializable]
public class HandData
{
    public float[,] Left;
    public float[,] Right;
}

public class MatlabHandSimulator : MonoBehaviour
{
    private UdpClient udpClient;
    private Thread receiveThread;
    private bool isRunning = true;
    public int port = 8051;  // Matching MATLAB port

    public XRHandSubsystem handSubsystem;
    public XRHand leftHand;
    public XRHand rightHand;

    private HandData currentHandData;
    private object lockObject = new object();

    void Start()
    {
        InitializeUDP();
    }

    void InitializeUDP()
    {
        udpClient = new UdpClient(port);
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    private void ReceiveData()
    {
        while (isRunning)
        {
            try
            {
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, port);
                byte[] data = udpClient.Receive(ref remoteEndPoint);
                string jsonMessage = Encoding.UTF8.GetString(data);
                
                // Parse JSON data
                HandData receivedData = JsonUtility.FromJson<HandData>(jsonMessage);
                lock (lockObject)
                {
                    currentHandData = receivedData;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"UDP Receive Error: {e}");
            }
        }
    }

    void Update()
    {
        if (currentHandData != null)
        {
            lock (lockObject)
            {
                ApplyHandRotations(leftHand, currentHandData.Left, true);
                ApplyHandRotations(rightHand, currentHandData.Right, false);
            }
        }
    }

    private void ApplyHandRotations(XRHand hand, float[,] angles, bool isLeft)
    {
        if (!hand.isTracked) return;

        // Apply rotations to each joint
        // Note: You'll need to map the 25 joints to your specific hand model hierarchy
        for (int i = 0; i < 25 && i < angles.GetLength(0); i++)
        {
            // Get the corresponding joint transform
            XRHandJoint joint = hand.GetJoint(GetJointIdForIndex(i));
            if (joint.TryGetPose(out Pose pose))
            {
                // Apply rotation from MATLAB (assuming XYZ order)
                Vector3 rotation = new Vector3(
                    angles[i, 0],
                    angles[i, 1],
                    angles[i, 2]
                );
                
                pose.rotation = Quaternion.Euler(rotation);
                // Apply the pose to your hand model
                // You'll need to implement this based on your specific hand setup
            }
        }
    }

    private XRHandJointID GetJointIdForIndex(int index)
    {
        switch (index)
        {
            // Thumb
            case 0: return XRHandJointID.ThumbMetacarpal;
            case 1: return XRHandJointID.ThumbProximal;
            case 2: return XRHandJointID.ThumbDistal;
            case 3: return XRHandJointID.ThumbTip;

            // Index
            case 4: return XRHandJointID.IndexMetacarpal;
            case 5: return XRHandJointID.IndexProximal;
            case 6: return XRHandJointID.IndexIntermediate;
            case 7: return XRHandJointID.IndexDistal;
            case 8: return XRHandJointID.IndexTip;

            // Middle
            case 9: return XRHandJointID.MiddleMetacarpal;
            case 10: return XRHandJointID.MiddleProximal;
            case 11: return XRHandJointID.MiddleIntermediate;
            case 12: return XRHandJointID.MiddleDistal;
            case 13: return XRHandJointID.MiddleTip;

            // Ring
            case 14: return XRHandJointID.RingMetacarpal;
            case 15: return XRHandJointID.RingProximal;
            case 16: return XRHandJointID.RingIntermediate;
            case 17: return XRHandJointID.RingDistal;
            case 18: return XRHandJointID.RingTip;

            // Little
            case 19: return XRHandJointID.LittleMetacarpal;
            case 20: return XRHandJointID.LittleProximal;
            case 21: return XRHandJointID.LittleIntermediate;
            case 22: return XRHandJointID.LittleDistal;
            case 23: return XRHandJointID.LittleTip;

            // Wrist
            case 24: return XRHandJointID.Wrist;

            default: return XRHandJointID.Wrist; // Default fallback
        }
    }

    void OnDisable()
    {
        isRunning = false;
        if (udpClient != null)
            udpClient.Close();
        if (receiveThread != null)
            receiveThread.Abort();
    }
}
