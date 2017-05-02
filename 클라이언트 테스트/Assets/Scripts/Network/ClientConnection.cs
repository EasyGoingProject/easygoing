using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

public class ClientConnection
{
    public IOCPManager iocpManager;
    public Socket sSock;
    private int MAX_INC_DATA = 2048;

    public ClientConnection(Socket s, IOCPManager iocp)
    {
        sSock = s;
        iocpManager = iocp;
        ThreadPool.QueueUserWorkItem(new WaitCallback(HandleConnetion));
    }

    private void HandleConnetion(object x)
    {
        Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");

        try
        {
            while (sSock.Connected)
            {
                byte[] sizeInfo = new byte[4];

                int bytesRead = 0, currentRead = 0;

                currentRead = bytesRead = sSock.Receive(sizeInfo);

                Debug.Log("====================================");
                Debug.Log("Step 1 " + BitConverter.ToInt32(sizeInfo, 0));

                if(BitConverter.ToInt32(sizeInfo, 0) > MAX_INC_DATA)
                    continue;

                while (bytesRead < sizeInfo.Length && currentRead > 0)
                {
                    currentRead = sSock.Receive(sizeInfo, bytesRead, sizeInfo.Length - bytesRead, SocketFlags.None);
                    bytesRead += currentRead;
                }

                int messageSize = BitConverter.ToInt32(sizeInfo, 0);

                byte[] incMessage = new byte[messageSize];

                bytesRead = 0;
                currentRead = bytesRead = sSock.Receive(incMessage, bytesRead, incMessage.Length - bytesRead, SocketFlags.None);

                while (bytesRead < messageSize && currentRead > 0)
                {
                    currentRead = sSock.Receive(incMessage, bytesRead, incMessage.Length - bytesRead, SocketFlags.None);
                    bytesRead += currentRead;
                }

                Debug.Log("Step 2 " + messageSize + " Message Read Complete");

                string serverMessage = Encoding.Default.GetString(incMessage);

                if (serverMessage.Length > 6 && serverMessage.Substring(0, 6) == "Server")
                {
                    string[] serverMessageSplit = serverMessage.Split("-"[0]);

                    try
                    {
                        ServerMessageType servMesType = (ServerMessageType)Enum.Parse(typeof(ServerMessageType), serverMessageSplit[1]);
                        //IOCPManager.GetInstance.ReceiveMessage(servMesType, serverMessageSplit[2]);
                    }
                    catch (Exception e)
                    {
                        Debug.Log("Server message getted failed. " + e.Message);
                    }
                }
                else if(!string.IsNullOrEmpty(serverMessage))
                {
                    try
                    {
                        //NetworkData incMes = (NetworkData)ConverterTools.ConvertBytesToOjbect(incMessage);
                        //iocpManager.ReceiveMessage(incMes);
                    }
                    catch (Exception e)
                    {
                        Debug.Log("Server message getted failed. " + e.Message + " / " + serverMessage);
                    }
                }
            }
        }
        catch
        {
            Debug.Log("Server is Closed.");
        }

        Debug.Log("Disconnected from the server.");

        IOCPManager.GetInstance.Disconnect(sSock);
        sSock.Close();
    }
}

public enum ServerMessageType
{
    Join,
    ClientNumber,
    ClientList,
    Host
}