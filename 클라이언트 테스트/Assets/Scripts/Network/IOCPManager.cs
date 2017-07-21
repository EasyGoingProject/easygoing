//#define DEBUGGING

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;

public class IOCPManager : Singleton<IOCPManager>
{
    public GameManager gameManager;
    public UIManager uiManager;

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
    private const string PrefabDataPoint = "Point";
    private const string PrefabDataPotion = "Potion";

    private const string DefaultPlayerName = "UnknownPlayer";

    private void InitPrefabData()
    {
        if (!PlayerPrefs.HasKey(PrefabDataIP))
            PlayerPrefs.SetString(PrefabDataIP, serverAddress);

        if (!PlayerPrefs.HasKey(PrefabDataPort))
            PlayerPrefs.SetString(PrefabDataPort, serverPort.ToString());

        if (!PlayerPrefs.HasKey(PrefabDataPlayerName))
            PlayerPrefs.SetString(PrefabDataPlayerName, DefaultPlayerName);

        if (!PlayerPrefs.HasKey(PrefabDataPoint))
            PlayerPrefs.SetInt(PrefabDataPoint, 0);

        if (!PlayerPrefs.HasKey(PrefabDataPotion))
            PlayerPrefs.SetInt(PrefabDataPotion, 0);

        LoadPrefabData();
    }

    private void LoadPrefabData()
    {
        serverAddress = PlayerPrefs.GetString(PrefabDataIP);
        serverPort = int.Parse(PlayerPrefs.GetString(PrefabDataPort));
        playerName = PlayerPrefs.GetString(PrefabDataPlayerName);
        GameManager.GetInstance.SetPoint(PlayerPrefs.GetInt(PrefabDataPoint));
        GameManager.GetInstance.SetPotion(PlayerPrefs.GetInt(PrefabDataPotion));

        lbServerIp.text = inputServerIP.value = serverAddress;
        lbServerPort.text = inputServerPort.value = serverPort.ToString();
        lbUserName.text = inputUserName.value = playerName;
    }

    public void SavePrefabData()
    {
        serverAddress = inputServerIP.value;
        serverPort = int.Parse(inputServerPort.value);
        playerName = inputUserName.value;

        PlayerPrefs.SetString(PrefabDataIP, serverAddress);
        PlayerPrefs.SetString(PrefabDataPort, serverPort.ToString());
        PlayerPrefs.SetString(PrefabDataPlayerName, playerName);
        PlayerPrefs.SetInt(PrefabDataPoint, GameManager.Point);
        PlayerPrefs.SetInt(PrefabDataPotion, GameManager.PotionCount);
    }

    #endregion


    #region [ Update ]

    private void Update()
    {
        if (isConnectionFailed)
        {
            isConnectionFailed = false;
            StartCoroutine(ConnectFailedCoroutine());
        }

        if (isChangeHost)
        {
            isChangeHost = false;

            int minClientNumber = clientDataList[0].clientNumber;

            for (int i = 0; i < clientDataList.Count; i++)
            {
                if (clientDataList[0].clientNumber < minClientNumber)
                    minClientNumber = clientDataList[0].clientNumber;
            }

            clientDataList.Find(x => x.clientNumber == minClientNumber).isHost = true;

            for (int i = 0; i < clientDataList.Count; i++)
            {
                clientControlList[clientDataList[i].clientNumber].UpdateClientData(clientDataList[i]);
            }

            if (clientDataList.Find(x => x.clientNumber == minClientNumber).isLocalPlayer)
            {
                connectionData.isHost = true;
                gameManager.PlayHost();
            }
        }
    }

    #endregion


    #region [ Connect ]

    [Header("[ Connect ]")]
    public string serverAddress = "127.0.0.1";
    public int serverPort = 2738;
    public string playerName = "UnknownPlayer";

    //데이터 입출력 쓰레드
    public TCPClient client;

    public static ClientData connectionData;
    public static int senderId = -1;
    
    public CharacterType characterType;

    public GameObject connectFailedObj;
    public UIButton btnServerConnect;
    public UIButton btnDisconnect;

    private bool isConnectionFailed = false;
    private bool isChangeHost = false;

    
    private void InitConnect()
    {
        EventDelegate.Add(btnServerConnect.onClick, ConnectClick);
        EventDelegate.Add(btnDisconnect.onClick, Disconnect);
    }

    private void ResetConnect()
    {
        senderId = -1;
        ResetClientDataList();
    }

    public void ConnectClick()
    {
        SavePrefabData();
        Connect();
    }

    public void Connect()
    {
        ResetConnect();
        
        try
        {
            client.StartConnect(this);
            GameManager.gameState = GameState.Lobby;
            uiManager.ShowPanel(PanelType.Lobby);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    public void ConnectFailed()
    {
        isConnectionFailed = true;
    }

    private IEnumerator ConnectFailedCoroutine()
    {
        connectFailedObj.SetActive(true);

        yield return new WaitForSeconds(2.0f);

        connectFailedObj.SetActive(false);

        uiManager.ShowPanel(PanelType.Connection);
    }

    #endregion


    #region [ Send Message ]

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

    // 데이터 확보 -> 동기화 : 클라간 데이터 전송
    public void ReceiveData(NetworkData netData)
    {
        try
        {
            switch (netData.sendType)
            {
                #region [ Game Play ]

                case SendType.READY:
                    if (clientControlList.ContainsKey(netData.senderId))
                        clientControlList[netData.senderId].PlayerReady();
                    AllReadyCheck(true);
                    break;

                case SendType.GAMESTART:
                    uiManager.SetTargetPanel(PanelType.Play);
                    gameManager.GamePlay();
                    break;

                case SendType.GAMETIMER:
                    gameManager.timerDataList.Add(netData);
                    break;

                #endregion


                #region [ Players ]

                case SendType.SYNCTRANSFORM:
                    if (clientControlList.ContainsKey(netData.senderId))
                        clientControlList[netData.senderId].netSyncTrans.SetTransform(netData.position, netData.rotation);
                    break;

                case SendType.ANIMATOR_MOVE:
                    if (clientControlList.ContainsKey(netData.senderId))
                        clientControlList[netData.senderId].netSyncAnimator.SetAnimatorMove(netData);
                    break;

                case SendType.ANIMATOR_TRIGGER:
                    if (clientControlList.ContainsKey(netData.senderId))
                        clientControlList[netData.senderId].netSyncAnimator.NetworkReceiveTrigger(netData);
                    break;

                case SendType.ATTACK:
                    gameManager.attackDataList.Add(netData);
                    break;

                case SendType.HIT:
                    if (clientControlList.ContainsKey(netData.targetId))
                        clientControlList[netData.targetId].LossHealth(netData.power, netData.senderId);
                    break;

                case SendType.DIE:
                    if (clientControlList.ContainsKey(netData.senderId))
                    {
                        clientControlList[netData.senderId].DoActionDie();
                        if (netData.targetId == senderId)
                        {
                            GameManager.KillCount++;
                            GameManager.GetInstance.AddPoint(1);
                        }
                    }
                    gameManager.CheckGameState();
                    break;

                case SendType.ADDHEALTH:
                    if (clientControlList.ContainsKey(netData.senderId))
                        clientControlList[netData.senderId].AddHealth(GlobalData.ITEM_HEALTH_HEAL_AMOUNT);
                    break;

                case SendType.EQUIPWEAPON:
                    if (clientControlList.ContainsKey(netData.senderId))
                        clientControlList[netData.senderId].SetWeapon(netData.weaponType);
                    break;

                #endregion


                #region [ Item ]

                case SendType.SPAWN_ITEM:
                    gameManager.itemDataList.Add(netData);
                    break;

                case SendType.OBJECT_SYNC_TRANSFORM:
                    if (gameManager.networkObjectList.ContainsKey(netData.targetId))
                        gameManager.networkObjectList[netData.targetId].SetTransform(netData.position, netData.rotation);
                    break;

                case SendType.DESTORY_OBJECT:
                    gameManager.removeObjectList.Add(netData);
                    break;

                #endregion


                #region [ Enemy ]

                case SendType.SPAWN_ENEMY:
                    gameManager.enemyDataList.Add(netData);
                    break;

                case SendType.ENEMY_SYNC_TRANSFORM:
                    if (gameManager.networkEnemyList.ContainsKey(netData.targetId))
                        gameManager.networkEnemyList[netData.targetId].netSyncTrans.SetTransform(netData.position, netData.rotation);
                    break;

                case SendType.ENEMY_ANIMATOR_MOVE:
                    if (gameManager.networkEnemyList.ContainsKey(netData.targetId))
                        gameManager.networkEnemyList[netData.targetId].netSyncAnimator.SetAnimatorMove(netData);
                    break;

                case SendType.ENEMY_ANIMATOR_TRIGGER:
                    if (gameManager.networkEnemyList.ContainsKey(netData.targetId))
                        gameManager.networkEnemyList[netData.targetId].netSyncAnimator.NetworkReceiveTrigger(netData);
                    break;

                case SendType.ENEMY_ATTACK:
                    gameManager.enemyAttackDataList.Add(netData);
                    break;

                case SendType.ENEMY_HIT:
                    if (gameManager.networkEnemyList.ContainsKey(netData.targetId))
                        gameManager.networkEnemyList[netData.targetId].LossHealth(netData.power);
                    break;

                case SendType.ENEMY_DIE:
                    if (gameManager.networkEnemyList.ContainsKey(netData.targetId))
                        gameManager.networkEnemyList[netData.targetId].DoActionDie();
                    break;

                #endregion
            }
        }
        catch (Exception e)
        {
            Debug.Log("Receive Error " + e.ToString());
        }
    }

    private string[] receiveSplit;

    // 데이터 확보 -> 마샬링 방식 : 서버(c)에서 직접 송출
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

                //Debug.Log("Client Data Step 0 " + clientDataList.Count + " : " + receiveSplit.Length + " => " + innerData);

                #region [ 플레이어 수가 같을 경우 ]
                if (clientDataList.Count == receiveSplit.Length - 1)
                {

                }
                #endregion
                #region [ 플레이어 수가 감소 했을 경우 ]
                else if (clientDataList.Count > receiveSplit.Length - 2)
                {
                    try
                    {
                        // 현재 접속 중인 플레이어 판별
                        List<int> connectedClientNumbers = new List<int>();
                        for (int i = 0; i < receiveSplit.Length - 1; i++)
                        {
                            string[] dataSplit = receiveSplit[i].Split("="[0]);
                            connectedClientNumbers.Add(int.Parse(dataSplit[1]));
                        }

                        bool isHostOut = false;

                        // 기존 플레이어 데이터 판별
                        for (int i = 0; i < clientDataList.Count; i++)
                        {
                            // 접속 중인 플레이어 목록에 없을 경우
                            if (!connectedClientNumbers.Contains(clientDataList[i].clientNumber))
                            {
                                // 접속 해제 처리
                                clientDataList[i].isDisconnected = true;
                                clientControlList[clientDataList[i].clientNumber].DoActionDisconnect();

                                // 호스트가 접속 해제시
                                if (clientControlList[clientDataList[i].clientNumber].clientData.isHost)
                                    isHostOut = true;

                                // 클라이언트 정보 삭제
                                clientDataList.Remove(clientDataList[i]);
                            }
                        }

                        // 호스트 접속 해제 알림
                        if (isHostOut && clientDataList.Count > 0)
                        {
                            isChangeHost = true;
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log("Server message getted failed. " + e.Message);
                        Debug.Log("Error " + innerData);
                    }
                }
                #endregion
                #region [ 플레이어 수가 증가 했을 경우 ]
                else
                {
                    try
                    {
                        // 서버에서 보내 온 클라이언트 목록 정보 확인
                        for (int i = 0; i < receiveSplit.Length - 1; i++)
                        {
                            string[] dataSplit = receiveSplit[i].Split("="[0]);

                            // 클라이언트 정보가 없을 경우 추가
                            if (clientDataList.Find(x => x.clientNumber == int.Parse(dataSplit[1])) == null)
                            {
                                // 클라이언트 데이터로 변환
                                ClientData clientData = new ClientData()
                                {
                                    isDie = false,
                                    isSpawned = false,
                                    isDisconnected = false,

                                    clientIndex = int.Parse(dataSplit[0]),
                                    isHost = int.Parse(dataSplit[0]) == 0,
                                    clientNumber = int.Parse(dataSplit[1]),
                                    isLocalPlayer = int.Parse(dataSplit[1]) == senderId,
                                    clientName = dataSplit[2],
                                    characterType = (CharacterType)(int.Parse(dataSplit[3])),
                                    isReady = dataSplit[4] == "1"
                                };

                                // 자신의 데이터인지 확인 후 connectionData로 입력
                                if (clientData.isLocalPlayer)
                                    connectionData = clientData;

                                clientDataList.Add(clientData);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log("Server message getted failed. " + e.Message);
                        Debug.Log("Error " + innerData);
                    }
                }
                #endregion

                #region [ 레디 상태 확인 ]

                try
                {
                    for (int i = 0; i < receiveSplit.Length - 1; i++)
                    {
                        string[] dataSplit = receiveSplit[i].Split("="[0]);

                        int clientNumber = int.Parse(dataSplit[1]);
                        bool isReady = dataSplit[4] == "1";

                        ClientData receiveClientData = clientDataList.Find(x => x.clientNumber == clientNumber);

                        if (isReady && !receiveClientData.isReady)
                        {
                            receiveClientData.isReady = isReady;
                            clientControlList[clientNumber].PlayerReady();
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.Log("Server message getted failed. " + e.Message);
                    Debug.Log("Error " + innerData);
                }

                #endregion

                AllReadyCheck(true);
                break;

            case ServerMessageType.Join:
                break;
        }
    }

    #endregion


    #region [ Client List ]

    public static List<ClientData> clientDataList = new List<ClientData>();
    public static Dictionary<int, PlayerControl> clientControlList = new Dictionary<int, PlayerControl>();
    public static PlayerControl myPlayerControl;

    private void ResetClientDataList()
    {
        clientDataList = new List<ClientData>();

        foreach (KeyValuePair<int, PlayerControl> keyVal in clientControlList)
        {
            if (keyVal.Value != null)
            {
                keyVal.Value.HideInfos();
                Destroy(keyVal.Value.gameObject);
            }
        }

        uiManager.RemovePlayerInfos();

        clientControlList = new Dictionary<int, PlayerControl>();
        myPlayerControl = null;
    }

    //플레이어들 레디 상태 확인
    public void AllReadyCheck(bool isChecking)
    {
        PlayerControl hostPlayer = null;
        bool isAllReady = true;

        foreach (KeyValuePair<int, PlayerControl> keyVal in clientControlList)
        {
            if (keyVal.Value.clientData.isHost)
                hostPlayer = keyVal.Value;

            if (keyVal.Value.clientData.clientNumber == senderId)
                myPlayerControl = keyVal.Value;

            if (!isChecking || !keyVal.Value.IsPlayerReady)
                isAllReady = false;
        }

        if (hostPlayer != null && clientControlList.Values.Count > 1)
            hostPlayer.AllPlayerReady(isAllReady);
    }

    #endregion


    #region [ Disconnect ]

    private void OnApplicationQuit()
    {
        Disconnect();
    }

    public void Disconnect()
    {
        ResetConnect();

        if (client != null)
            client.Disconnect();

        gameManager.ResetGame();
        uiManager.ShowPanel(PanelType.Connection);
    }

    #endregion
}

[Serializable]
public class ClientData
{
    public int clientNumber;
    public CharacterType characterType;
    public bool isReady = false;
    public bool isSpawned = false;
    public bool isDie = false;
    public bool isLocalPlayer = false;
    public bool isHost = false;
    public bool isDisconnected = false;
    public int clientIndex;
    public string clientName;
}