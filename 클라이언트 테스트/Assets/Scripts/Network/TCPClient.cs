//#define DEBUGGING

using UnityEngine;
using System;
using System.Text;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using System.Net;

public class TCPClient : MonoBehaviour
{
    public const string ServerMessageHead = "Server";
    public const int BUFFSIZE = 2048;

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

            BeginReceiveData(4);
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
            connectState = ConnectionState.NotConnected;
            SendClientData("End");
        }
    }

    private void OnDestroy()
    {
        Disconnect();
    }

    #endregion


    #region [ Receive Data ]

    byte[] willRecv = new byte[4];
    bool header = true;

    private void BeginReceiveData(int recvLen)
    {
        if (header)
            clientSocket.BeginReceive(willRecv, 0, recvLen, SocketFlags.None, EndReceiveData, null);
        else
            clientSocket.BeginReceive(readBuffer, 0, recvLen, SocketFlags.None, EndReceiveData, null);
    }

    int realWill;
    private void EndReceiveData(IAsyncResult iar)
    {
        if(header)
        {
            clientSocket.EndReceive(iar);
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(willRecv);
            }
            realWill = BitConverter.ToInt32(willRecv, 0);
            header = false;

            BeginReceiveData(realWill);
        }
        else
        {
            int numBytesReceived = clientSocket.EndReceive(iar);
            byte[] readData = readBuffer;

            readBuffer = new byte[BUFFSIZE];
            header = true;
            BeginReceiveData(4);

            if (numBytesReceived > 0)
                ProcessData(readData, numBytesReceived);
        }
    }

    #endregion


    #region [ Data Processing ]

    public static string ByteArrayToHexString(byte[] ba, int length)
    {
        StringBuilder hex = new StringBuilder(ba.Length * 3);
        for(int i=0;i<length;i++)
            hex.AppendFormat("{0:x2} ", ba[i]);
        return hex.ToString();
    }

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
                NetworkData incMes = (NetworkData)ConverterTools.ConvertBytesToOjbect(readData, numBytesRecv);
                iocpManager.ReceiveData(incMes);
            }
            catch (Exception e)
            {
                Debug.Log("Server message getted failed. " + e.Message + "\n"+ByteArrayToHexString(readData, numBytesRecv));
            }
        }
    }

    #endregion


    #region [ Send ]
    
    //데이터를 보내는데에 순서를 정하기위해 send 대기열(queue)을 만들었다.
    private Queue<DataToSend> sendpacketorder = new Queue<DataToSend>();

    //send 대기열의 정보다.
    private struct DataToSend
    {
        public byte[] data;             //보낼 데이터
        public int length;              //보낼 데이터의 크기
        public bool headerTransfered;   //헤더가 보내졌는지(true) 데이터가 보내졌는지(false)에 대한 정보다.
    };

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
        //이제 데이터를 보내기 위해서는 해더를 먼저 보내 데이터의 크기를 알려줘야 한다.
        SendHeaderFirst(msgArray);
    }
    
    public void SendData(NetworkData netData, bool activateCallback)
    {
        byte[] msgArray = ConverterTools.ConvertObjectToBytes(netData);
        //마찬가지로 해더를 먼저 보낸다.
        SendHeaderFirst(msgArray);
    }

    //해더를 보내고 DataToSend구조체를 구성하여 대기열에 저장한다. 
    //대기열에 아무것도 없을 때, 메인 스레드(게임 스레드)에서 대기열의 모든 데이터에 대한 전송을 triggering한다.
    public void SendHeaderFirst(byte[] tosend)
    {
        byte[] intBytes = BitConverter.GetBytes(tosend.Length);
        if(!BitConverter.IsLittleEndian)
            Array.Reverse(intBytes);
        byte[] result = intBytes;

        DataToSend data;
        data.data = tosend;
        data.length = tosend.Length;
        data.headerTransfered = true;

        if(sendpacketorder.Count == 0)
            clientSocket.BeginSend(result, 0, 4, SocketFlags.None, EndSend, data);

        sendpacketorder.Enqueue(data);
    }

    //비동기 작업의 Completion Callback
    //순서 있는 통신을 진행시키는 중요한 메소드
    private void EndSend(IAsyncResult iar)
    {
        int sended = clientSocket.EndSend(iar);
        DataToSend data = (DataToSend)iar.AsyncState;

        if (data.headerTransfered)//헤더 -> 데이터
        {
            data.headerTransfered = false;
            clientSocket.BeginSend(data.data, 0, data.length, SocketFlags.None, EndSend, data);
        }
        else//데이터 -> 헤더(데이터가 대기중에 있으면)
        {
            sendpacketorder.Dequeue();
            if(connectState == ConnectionState.NotConnected)
            {
                clientSocket.Close();
                clientSocket = null;
                return;
            }

            if(sendpacketorder.Count > 0)
            {
                DataToSend tosend = sendpacketorder.ToArray()[0];
                
                byte[] intBytes = BitConverter.GetBytes(tosend.length);
                if(!BitConverter.IsLittleEndian)
                    Array.Reverse(intBytes);
                byte[] result = intBytes;
                clientSocket.BeginSend(result, 0, 4, SocketFlags.None, EndSend, tosend);
            }
        }
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