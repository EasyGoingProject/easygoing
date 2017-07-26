//#define DEBUGGING

using UnityEngine;
using System;
using System.Text;
using System.Net.Sockets;
using System.Collections;


public class TCPClient : MonoBehaviour
{
    public const string ServerMessageHead = "Server";
    public const int BUFFSIZE = 1024;
    public const int SIZEOFBUFF = 2048;

    public enum ConnectionState
    {
        NotConnected,
        AttemptingConnect,
        SetClient,
        Connected
    }

    [Header("[ Client State ]")]
    private string serverAddress;
    private int serverPort;
    public ConnectionState connectState;

    private Socket clientSocket;
    private byte[] readBuffer;
    private IOCPManager iocpManager;

    private void Start()
    {
        connectState = ConnectionState.NotConnected;
        readBuffer = new byte[BUFFSIZE];
    }

    #region [ Connect ]

    public void StartConnect(IOCPManager iocp)
    {
        iocpManager = iocp;
        serverAddress = iocpManager.serverAddress;
        serverPort = iocpManager.serverPort;

        Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");

        try
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IAsyncResult result = clientSocket.BeginConnect(serverAddress, serverPort, EndConnect, null);
            bool connectSuccess = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(10));
            if (!connectSuccess)
            {
                clientSocket.Close();
                Debug.LogError(string.Format("Client unable to connect. Failed"));
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(string.Format("Client exception on beginconnect: {0}", ex.Message));
        }
        connectState = ConnectionState.AttemptingConnect;
    }

    private void EndConnect(IAsyncResult iar)
    {
        if (clientSocket.Connected)
        {
            clientSocket.EndConnect(iar);
            clientSocket.NoDelay = true;
            connectState = ConnectionState.SetClient;

#if DEBUGGING
        Debug.Log("Client connected");
#endif

            BeginReceiveData();
        }
        else
        {
            iocpManager.ConnectFailed();
        }
    }

    #endregion


    #region [ Disconnect ] 

    public void Disconnect()
    {
        if (clientSocket != null)
        {
            clientSocket.Close();
            clientSocket = null;
        }
    }

    private void OnDestroy()
    {
        Disconnect();
    }

    #endregion


    #region [ Receive Data ]

    private void BeginReceiveData()
    {
        clientSocket.BeginReceive(readBuffer, 0, readBuffer.Length, SocketFlags.None, EndReceiveData, null);
    }

    private void EndReceiveData(IAsyncResult iar)
    {
        int numBytesReceived = clientSocket.EndReceive(iar);
        byte[] readData = readBuffer;

        readBuffer = new byte[BUFFSIZE];
        BeginReceiveData();

        if (numBytesReceived > 0)
            ProcessData(readData, numBytesReceived);
    }

    #endregion


    #region [ Data Processing ]

    private void ProcessData(byte[] readData, int numBytesRecv)
    {
        string serverMessage = Encoding.ASCII.GetString(readData, 0, numBytesRecv);

#if DEBUGGING
        Debug.Log("Receive " + serverMessage);
#endif

        if (serverMessage.Length > ServerMessageHead.Length 
            && serverMessage.Substring(0, ServerMessageHead.Length) == ServerMessageHead)
        {
            string[] serverMessageSplit = serverMessage.Split("-"[0]);

            try
            {
                ServerMessageType servMesType = (ServerMessageType)Enum.Parse(typeof(ServerMessageType), serverMessageSplit[1]);
                iocpManager.ReceiveData(servMesType, serverMessageSplit[2]);

                if(servMesType == ServerMessageType.ClientNumber)
                    SendClientConnected();

#if DEBUGGING
                Debug.Log(servMesType);
#endif
            }
            catch (Exception e)
            {
                Debug.Log("Server message getted failed. " + e.Message);
                Debug.Log("Error " + serverMessage);
            }
        }
        else if (!string.IsNullOrEmpty(serverMessage) && serverMessage.Length > 5)
        {
            try
            {
                NetworkData incMes = (NetworkData)ConverterTools.ConvertBytesToOjbect(readData);
                iocpManager.ReceiveData(incMes);
            }
            catch (Exception e)
            {
                Debug.Log("Server message getted failed. " + e.Message + " / " + serverMessage);
            }
        }
    }

    #endregion


    #region [ Send ]

    public void SendClientConnected()
    {
        SendClientData(string.Concat(
                        "Connected", ",",
                        IOCPManager.senderId, ",",
                        iocpManager.playerName, ",",
                        (int)iocpManager.characterType, ",",
                        "0", ","));
    }

    public void SendClientReady(bool isReady)
    {
        SendClientData(string.Concat(
                        "Ready", ",",
                        IOCPManager.senderId, ",",
                        isReady ? "1" : "0", ","));
    }

    public void SendClientData(string message)
    {
        SendData(string.Concat("Client-", message));
    }

    public void SendData(string message)
    {
        byte[] msgArray = Encoding.UTF8.GetBytes(message);
        int len = msgArray.Length;
        clientSocket.BeginSend(msgArray, 0, len, SocketFlags.None, EndSend, msgArray);
    }

    public void SendData(NetworkData netData)
    {
        byte[] msgArray = ConverterTools.ConvertObjectToBytes(netData);
        int len = msgArray.Length;
        clientSocket.BeginSend(msgArray, 0, len, SocketFlags.None, EndSend, msgArray);
    }

    private void EndSend(IAsyncResult iar)
    {
        clientSocket.EndSend(iar);
    }

    #endregion


    #region [ Utility ]

    public static string CompileBytesIntoString(byte[] msg, int len = -1)
    {
        string temp = "";
        int count = len;

        if (count < 1)
            count = msg.Length;

        for (int i = 0; i < count; i++)
            temp += string.Format("{0} ", msg[i]);

        return temp;
    }

    #endregion
}