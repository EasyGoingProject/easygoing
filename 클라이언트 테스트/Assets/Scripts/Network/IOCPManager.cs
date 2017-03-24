using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net.Sockets;
using System.Threading;
using System.Net;

public class IOCPManager : Singleton<IOCPManager>
{
    private NetworkData networkData;
    private string serverAddress = "127.0.0.1";
    private int serverPort = 2738;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DebugDelegate(string str);

    DebugDelegate callback_delegate;
    IntPtr intptr_delegate;

    [DllImport("IOCPEchoClientDLL")]
    public static extern int SetDebug(IntPtr str);

    [DllImport("IOCPEchoClientDLL")]
    public static extern int SetServer(string address, string port);

    [DllImport("IOCPEchoClientDLL")]
    public static extern int ClientSendMessage(string message);

    [DllImport("IOCPEchoClientDLL")]
    public static extern int StartReceiveMessage();

    [DllImport("IOCPEchoClientDLL")]
    public static extern int StartClient();

    [DllImport("IOCPEchoClientDLL")]
    public static extern int StopClient();

    public bool isConnected = false;

    static void CallBackDebug(string str)
    {
        Debug.Log("IOCP : " + str);
    }

    void Start()
    {
        networkData = new NetworkData();

        callback_delegate = new DebugDelegate(CallBackDebug);
        intptr_delegate = Marshal.GetFunctionPointerForDelegate(callback_delegate);

        Debug.Log("Step 1 : C Library Debugging");
        SetDebug(intptr_delegate);

        Debug.Log("Step 2 : IOCP Server Connection Setting");
        SetServer(serverAddress, serverPort.ToString());

        Debug.Log("Step 3 : Join Server");
        StartClient();

        isConnected = true;

        ThreadPool.QueueUserWorkItem(new WaitCallback(HandleConnetion));
        //BinaryFormatter bf = new BinaryFormatter();
        //Test_Packet packet = (Test_Packet)bf.Deserialize(Packet_Deserialize(data));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
            ClientSendMessage("Left");
    }


    private void OnApplicationQuit()
    {
        Debug.Log(StopClient());
    }


    private void HandleConnetion(object x)
    {
        try
        {
            while (isConnected)
            {
                //byte[] sizeInfo = new byte[4];

                //int bytesRead = 0, currentRead = 0;

                //currentRead = bytesRead = sSock.Receive(sizeInfo);

                //while (bytesRead < sizeInfo.Length && currentRead > 0)
                //{
                //    currentRead = sSock.Receive(sizeInfo, bytesRead, sizeInfo.Length - bytesRead, SocketFlags.None);
                //    bytesRead += currentRead;
                //}

                //int messageSize = BitConverter.ToInt32(sizeInfo, 0);
                //byte[] incMessage = new byte[messageSize];

                //bytesRead = 0;
                //currentRead = bytesRead = sSock.Receive(incMessage, bytesRead, incMessage.Length - bytesRead, SocketFlags.None);

                //while (bytesRead < messageSize && currentRead > 0)
                //{
                //    currentRead = sSock.Receive(incMessage, bytesRead, incMessage.Length - bytesRead, SocketFlags.None);
                //    bytesRead += currentRead;
                //}

                try
                {
                    //Debug.Log(incMessage);
                    //message incMes = (message)conversionTools.convertBytesToOjbect(incMessage);
                }
                catch (Exception e)
                {
                    Debug.Log("Server message getted failed. " + e.Message);
                }
            }
        }
        catch
        {
            Debug.Log("Server is Closed.");
        }

        Debug.Log("Disconnected from the server.");
        StopClient();
    }

}
