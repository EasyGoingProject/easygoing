using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public class ClientConnection
{
    public Socket sSock;
    private int MAX_INC_DATA = 512000;

    public ClientConnection(Socket s)
    {
        sSock = s;
        ThreadPool.QueueUserWorkItem(new WaitCallback(HandleConnetion));
    }

    private void HandleConnetion(object x)
    {
        Debug.Log("Connected to server.");

        try
        {
            while (sSock.Connected)
            {
                byte[] sizeInfo = new byte[4];

                int bytesRead = 0, currentRead = 0;

                currentRead = bytesRead = sSock.Receive(sizeInfo);

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

                try
                {
                    Debug.Log("Receive 1 : " + (ConverterTools.ConvertBytesToOjbect(incMessage)));

                    NetworkData incMes = (NetworkData)ConverterTools.ConvertBytesToOjbect(incMessage);

                    IOCPManager.GetInstance.ReceiveMessage(incMes);
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

        IOCPManager.GetInstance.Disconnect();
        sSock.Close();
    }
}