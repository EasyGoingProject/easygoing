using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net.Sockets;
using System.Threading;
using System.Net;

public class IOCPManager : Singleton<IOCPManager>
{
    private string serverAddress = "127.0.0.1";
    private int serverPort = 2738;
    private Socket serverSocket;

    public static bool isConnected = false;
    public static int senderId = 0;

    public UILabel lbServerMessage;

    public UIButton btnServerConnect;

    private void Start()
    {
        EventDelegate.Add(btnServerConnect.onClick, ConnectClick);
    }

    public void ConnectClick()
    {
        StartCoroutine(Connect());
    }

    public IEnumerator Connect()
    {
        try
        {
            //Security.PrefetchSocketPolicy(serverAddress, serverPort);
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Connect(new IPEndPoint(IPAddress.Parse(serverAddress), serverPort));

            ClientConnection serv = new ClientConnection(serverSocket);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }

        while (!serverSocket.Connected)
            yield return null;

        isConnected = true;

        btnServerConnect.gameObject.SetActive(false);

        SendJoin(senderId);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.W))
            SendString("UP");
        if (Input.GetKeyDown(KeyCode.A))
            SendString("LEFT");
        if (Input.GetKeyDown(KeyCode.D))
            SendString("RIGHT");
        if (Input.GetKeyDown(KeyCode.S))
            SendString("DOWN");
    }


    #region [ Send Message ]

    public void SendJoin(int _senderID)
    {
        NetworkData sendData = new NetworkData()
        {
            senderId = _senderID,
            dataType = DataType.JOIN
        };
        SendToServerMessage(sendData);
    }

    public void SendString(string _dataString)
    {
        NetworkData sendData = new NetworkData()
        {
            senderId = senderId,
            dataType = DataType.MESSAGE,
            message = _dataString
        };
        SendToServerMessage(sendData);
    }

    public void SendTransform(Vector3 syncPosition, Quaternion syncRotation)
    {
        NetworkVector networkPosition = new NetworkVector()
        {
            x = syncPosition.x,
            y = syncPosition.y,
            z = syncPosition.z,
        };
        NetworkQuaternion networkRotation = new NetworkQuaternion()
        {
            x = syncRotation.x,
            y = syncRotation.y,
            z = syncRotation.z,
            w = syncRotation.w
        };

        NetworkData sendData = new NetworkData()
        {
            senderId = IOCPManager.senderId,
            dataType = DataType.SYNCTRANSFORM,
            position = networkPosition,
            rotation = networkRotation
        };

        //SendToServerMessage(sendData);
    }

    public void SendToServerMessage(NetworkData netData)
    {
        if (isConnected)
        {
            try
            {
                byte[] mesObj = ConverterTools.ConvertObjectToBytes(netData);
                byte[] readyToSend = ConverterTools.WrapMessage(mesObj);

                serverSocket.Send(readyToSend);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }
    }

    #endregion


    #region [ Receive Message ]

    public void ReceiveMessage(NetworkData netData)
    {
        //lbServerMessage.text += string.Concat(netData.senderId, " : ", netData.dataType, "\n");
        Debug.Log("Sender : " + netData.senderId + " : " + netData.dataType + " : " + netData.message);
    }

    #endregion


    #region [ Reset ]

    #endregion


    #region [ Disconnect ]

    public void OnApplicationQuit()
    {
        Disconnect();
    }

    public void Disconnect()
    {
        try
        {
            serverSocket.Close();
        }
        catch { }
        isConnected = false;
    }

    #endregion

}


