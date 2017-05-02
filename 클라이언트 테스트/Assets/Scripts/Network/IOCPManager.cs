//#define DEBUGGING

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

public class IOCPManager : Singleton<IOCPManager>
{
    public GameManager gameManager;

    private void Start()
    {
        InitPrefabData();
        InitConnect();
    }


    #region [ Data ]

    [Header("[ Data ]")]
    public UILabel lbServerIp;
    public UIInput inputServerIP;

    public UILabel lbServerPort;
    public UIInput inputServerPort;

    public UILabel lbUserName;
    public UIInput inputUserName;

    private const string PrefabDataIP = "ServerIP";
    private const string PrefabDataPort = "ServerPort";
    private const string PrefabDataPlayerName = "PlayerName";

    private const string DefaultPlayerName = "UnknownPlayer";

    private void InitPrefabData()
    {
        if (!PlayerPrefs.HasKey(PrefabDataIP))
            PlayerPrefs.SetString(PrefabDataIP, serverAddress);

        if (!PlayerPrefs.HasKey(PrefabDataPort))
            PlayerPrefs.SetString(PrefabDataPort, serverPort.ToString());

        if (!PlayerPrefs.HasKey(PrefabDataPlayerName))
            PlayerPrefs.SetString(PrefabDataPlayerName, DefaultPlayerName);

        LoadPrefabData();
    }

    private void LoadPrefabData()
    {
        serverAddress = PlayerPrefs.GetString(PrefabDataIP);
        serverPort = int.Parse(PlayerPrefs.GetString(PrefabDataPort));
        playerName = PlayerPrefs.GetString(PrefabDataPlayerName);

        lbServerIp.text = inputServerIP.value = serverAddress;
        lbServerPort.text = inputServerPort.value = serverPort.ToString();
        lbUserName.text = inputUserName.value = playerName;
    }

    private void SavePrefabData()
    {
        serverAddress = inputServerIP.value;
        serverPort = int.Parse(inputServerPort.value);
        playerName = inputUserName.value;

        PlayerPrefs.SetString(PrefabDataIP, serverAddress);
        PlayerPrefs.SetString(PrefabDataPort, serverPort.ToString());
        PlayerPrefs.SetString(PrefabDataPlayerName, playerName);
    }

    #endregion


    #region [ Connect ]

    [Header("[ Connect ]")]
    public string serverAddress = "127.0.0.1";
    public int serverPort = 2738;
    public string playerName = "UnknownPlayer";
    public int characterIndex = 0;
    public static ClientData connectionData;

    public static int senderId = -1;

    public TCPClient client;

    public GameObject panelServerConnect;
    public UIButton btnServerConnect;

    private string[] receiveSplit;

    private void InitConnect()
    {
        EventDelegate.Add(btnServerConnect.onClick, ConnectClick);
    }

    private void ResetConnect()
    {
        senderId = -1;
    }

    public void ConnectClick()
    {
        SavePrefabData();
        Connect();
    }

    public void Connect()
    {
        ResetConnect();
        ResetClientDataList();

        try
        {
            client.StartConnect(this);
            panelServerConnect.SetActive(false);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    #endregion


    #region [ Send Message ]

    public void SendJoin(int _senderID)
    {
        SendToServerMessage(new NetworkData()
        {
            senderId = _senderID,
            sendType = SendType.JOIN
        });
    }

    public void SendString(string _dataString)
    {
        SendToServerMessage(new NetworkData()
        {
            senderId = senderId,
            sendType = SendType.MESSAGE,
            message = _dataString
        });
    }

    public void SendToServerMessage(NetworkData netData)
    {
        if (client.connectState == TCPClient.ConnectionState.Connected
            || client.connectState == TCPClient.ConnectionState.SetClient)
        {
            try
            {
                byte[] mesObj = ConverterTools.ConvertObjectToBytes(netData);
                client.SendData(netData);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }
    }

    #endregion


    #region [ Receive Message ]

    public void ReceiveData(NetworkData netData)
    {
        try
        {
            switch (netData.sendType)
            {
                case SendType.SYNCTRANSFORM:
                    clientControlList[netData.senderId].netSyncTrans.SetTransform(netData.position, netData.rotation);
                    break;

                case SendType.ANIMATOR_MOVE:
                    clientControlList[netData.senderId].netSyncAnimator.SetAnimatorMove(netData);
                    break;

                case SendType.ANIMATOR_TRIGGER:
                    clientControlList[netData.senderId].netSyncAnimator.NetworkReceiveTrigger(netData);
                    break;

                case SendType.ATTACK:
                    gameManager.attackDataList.Add(netData);
                    break;

                case SendType.HIT:
                    clientControlList[netData.targetId].LossHealth(netData.power);
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.Log("Receive Error " + e.ToString());
        }
    }

    public void ReceiveData(ServerMessageType servMesType, string innerData)
    {
#if DEBUGGING
        Debug.Log("==> Server : " + servMesType + " : " + innerData);
#endif

        switch (servMesType)
        {
            case ServerMessageType.ClientNumber:
                receiveSplit = innerData.Split(","[0]);
                int clientCount = int.Parse(receiveSplit[0]);
                senderId = int.Parse(receiveSplit[1]);

                break;

            case ServerMessageType.ClientList:
                client.connectState = TCPClient.ConnectionState.Connected;

                receiveSplit = innerData.Split(","[0]);

                for (int i = 0; i < receiveSplit.Length - 1; i++)
                {
                    string[] dataSplit = receiveSplit[i].Split("="[0]);

                    ClientData clientData = new ClientData()
                    {
                        isDie = false,
                        isSpawned = false,

                        clientIndex = int.Parse(dataSplit[0]),
                        isHost = int.Parse(dataSplit[0]) == 0,
                        clientNumber = int.Parse(dataSplit[1]),
                        isLocalPlayer = int.Parse(dataSplit[1]) == senderId,
                        clientName = dataSplit[2],
                        characterType = (CharacterType)(int.Parse(dataSplit[3]))
                    };

                    if (clientData.isLocalPlayer)
                        connectionData = clientData;

                    AddClient(clientData);
                }

                break;

            case ServerMessageType.Join:
                break;
        }
    }

    #endregion


    #region [ Client List ]

    public static List<ClientData> clientDataList = new List<ClientData>();
    public static Dictionary<int, PlayerControl> clientControlList = new Dictionary<int, PlayerControl>();

    private void ResetClientDataList()
    {
        clientDataList = new List<ClientData>();
        clientControlList = new Dictionary<int, PlayerControl>();
    }

    private void AddClient(ClientData clientData)
    {
        if (clientDataList.Find(x => x.clientNumber == clientData.clientNumber) == null)
        {
#if DEBUGGING
            Debug.Log("Add Client " + clientData.clientNumber);
#endif
            clientDataList.Add(clientData);
        }
    }

    #endregion


    #region [ Disconnect ]

    public void Disconnect(Socket client)
    {
        try
        {
            client.Shutdown(SocketShutdown.Both);
            client.Close();
        }
        catch { }
    }

    #endregion
}

public class ClientData
{
    public int clientNumber;
    public CharacterType characterType;
    public bool isSpawned = false;
    public bool isDie = false;
    public bool isLocalPlayer = false;
    public bool isHost = false;
    public int clientIndex;
    public string clientName;
}