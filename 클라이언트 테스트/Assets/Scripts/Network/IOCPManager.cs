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
    private NetworkData networkData;
    private string serverAddress = "127.0.0.1";
    private int serverPort = 2738;
    private Socket serverSocket;

    public static bool isConnected = false;
    public static int senderId = 0;

    private void Start()
    {
        Connect();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
            SendToServerMessage("Left");
        else if(Input.GetKeyDown(KeyCode.W))
            SendToServerMessage("Up");
        else if (Input.GetKeyDown(KeyCode.D))
            SendToServerMessage("Right");
        else if (Input.GetKeyDown(KeyCode.S))
            SendToServerMessage("Down");
    }

    public void Connect()
    {
        try
        {
            //Security.PrefetchSocketPolicy(serverAddress, serverPort);
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Connect(new IPEndPoint(IPAddress.Parse(serverAddress), serverPort));

            ClientConnection serv = new ClientConnection(serverSocket);

            isConnected = true;
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    public void SendTransform(Vector3 position, Quaternion rotation)
    {
        return;

        SendToServerMessage(new NetworkData(
            senderId,
            DataType.SYNCTRANSFORM,
            new NetworkData.NetworkVector()
            {
                x = position.x,
                y = position.y,
                z = position.z
            },
            new NetworkData.NetworkQuaternion()
            {
                x = rotation.x,
                y = rotation.y,
                z = rotation.z,
                w = rotation.w
            }));
    }

    public void SendToServerMessage(string data)
    {
        if (isConnected)
        {
            try
            {
                byte[] mesObj = ConverterTools.ConvertObjectToBytes(data);
                byte[] readyToSend = ConverterTools.WrapMessage(mesObj);
                serverSocket.Send(readyToSend);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }
    }

    public void SendToServerMessage(NetworkData netData)
    {
        if (isConnected)
        {
            try
            {
                byte[] mesObj = ConverterTools.ConvertObjectToBytes(netData);
                byte[] readyToSend = ConverterTools.WrapMessage(mesObj);
                serverSocket.Send(mesObj);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }
    }


    public void ReceiveMessage(NetworkData netData)
    {
        Debug.Log(netData.SenderId + " : " + netData.DataType + " : " + netData.Position + " : " + netData.Rotation);
    }


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


