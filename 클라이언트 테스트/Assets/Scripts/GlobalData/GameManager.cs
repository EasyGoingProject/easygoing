using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.Utility;

public class GameManager : Singleton<GameManager>
{
    public static GameState gameState = GameState.NoConnect;
    public static ResultState resultState = ResultState.Win;
    public static int KillCount = 0;
    public static int Point = 0;
    public static TimeSpan gameTimer;
    private bool gameEndTrigger = false;

    private float itemZoneSize = 8.0f;
    private float itemZoneHeight = 20.0f;
    private float enemyZoneSize = 7.0f;
    private float enemyZoneHeight = 4.0f;

    private void Awake()
    {
        Application.runInBackground = true;

        PhysicsLayerSetting();

        InitGameEnd();
        InitSelectCharacter();
        InitHost();
        InitPotion();

        ResetGame();
    }

    private void Update()
    {
        UpdateGameStart();

        if (gameState == GameState.Playing && Input.GetKeyDown(KeyCode.E))
            UsePotion();

        #region [ Test ]
        //if (Input.GetKeyDown(KeyCode.KeypadPlus))
        //    AddPoint(1);
        #endregion
    }

    public void ResetGame()
    {
        gameState = GameState.NoConnect;

        gameEndTrigger = false;
        KillCount = 0;
        gameTimer = TimeSpan.FromSeconds(0);
        SetTimerString();

        InitHost();
        ResetDataStack();
        ResetHost();
        ResetEnemies();
        ResetArea();
    }

    #region [ Physic ]

    private void PhysicsLayerSetting()
    {
        #region [ Attack Layer]

        Physics.IgnoreLayerCollision(LayerMask.NameToLayer(GlobalData.LAYER_ENEMYATTACK),
                                     LayerMask.NameToLayer(GlobalData.LAYER_ENEMYATTACK), true);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer(GlobalData.LAYER_ENEMYATTACK),
                                     LayerMask.NameToLayer(GlobalData.LAYER_ENEMY), true);

        Physics.IgnoreLayerCollision(LayerMask.NameToLayer(GlobalData.LAYER_PLAYERATTACK),
                                     LayerMask.NameToLayer(GlobalData.LAYER_PLAYERATTACK), true);
        //Physics.IgnoreLayerCollision(LayerMask.NameToLayer(GlobalData.LAYER_PLAYERATTACK),
        //                             LayerMask.NameToLayer(GlobalData.LAYER_PLAYER), true);

        Physics.IgnoreLayerCollision(LayerMask.NameToLayer(GlobalData.LAYER_ENEMYATTACK),
                                     LayerMask.NameToLayer(GlobalData.LAYER_PLAYERATTACK), true);

        #endregion
    }

    #endregion


    #region [ Server Message Stack ]

    private List<ClientData> nonRespawnedClientList;
    public List<NetworkData> netDataList;

    private void ResetDataStack()
    {
        nonRespawnedClientList = new List<ClientData>();
        netDataList = new List<NetworkData>();
    }

    private void FixedUpdate()
    {
        if (IOCPManager.GetInstance.client != null
            && IOCPManager.GetInstance.client.connectState == TCPClient.ConnectionState.Connected)
        {
            if (gameEndTrigger)
            {
                gameEndTrigger = false;
                if (resultState == ResultState.Win)
                    AddPoint(3);
                else
                    AddPoint(1);

                UIManager.GetInstance.ShowPanel(PanelType.Result);
            }

            if (IOCPManager.clientDataList.FindAll(x => !x.isSpawned).Count > 0)
            {
                nonRespawnedClientList = IOCPManager.clientDataList.FindAll(x => !x.isSpawned);

                for (int i = 0; i < nonRespawnedClientList.Count; i++)
                    RespawnCharacter(nonRespawnedClientList[i]);
            }

            if(netDataList.Count > 0)
            {
                for(int i= 0; i < netDataList.Count; i++)
                {
                    switch (netDataList[i].sendType)
                    {
                        case SendType.ATTACK:
                            CreateAttack(netDataList[i]);
                            break;

                        case SendType.SPAWN_ITEM:
                            CreateItem(netDataList[i]);
                            break;

                        case SendType.SPAWN_ENEMY:
                            CreateEnemy(netDataList[i]);
                            break;

                        case SendType.ENEMY_ATTACK:
                            CreateEnemyAttack(netDataList[i]);
                            break;

                        case SendType.GAMETIMER:
                            SetTimer(netDataList[i]);
                            break;

                        case SendType.ALARM:
                            AreaDeactiveAlarm(netDataList[i]);
                            break;

                        case SendType.DESTORY_OBJECT:
                            RemoveNetworkObject(netDataList[i]);
                            break;

                        case SendType.DEACTIVATEAREA:
                            DeactivateArea(netDataList[i]);
                            break;

                        case SendType.ADDKILL:
                            KillOther((int)netDataList[i].power);
                            break;

                        case SendType.SPAWN_WINNERPOINT:
                            CreateWinnerPoint(netDataList[i]);
                            break;

                        case SendType.INWINNERPOINT:
                            CheckGameState(netDataList[i].senderId);
                            break;
                    }
                }

                netDataList = new List<NetworkData>();
            }
        }
    }

    #endregion


    // -- GAME PLAY----

    #region [ Select Character ]

    [Header("[Select Character]")]
    public UITexture texCharacter;
    public UIButton btnCharacterBefore;
    public UIButton btnCharacterNext;
    private int maxCharacterIndex;

    private void InitSelectCharacter()
    {
        EventDelegate.Add(btnCharacterBefore.onClick, OnSelectBefore);
        EventDelegate.Add(btnCharacterNext.onClick, OnSelectNext);

        maxCharacterIndex = System.Enum.GetNames(typeof(CharacterType)).Length - 1;

        SelectCharacter(CharacterType.Character001);
    }

    private void OnSelectBefore()
    {
        if ((int)IOCPManager.GetInstance.characterType == 0)
            SelectCharacter((CharacterType)maxCharacterIndex);
        else
            SelectCharacter((CharacterType)((int)IOCPManager.GetInstance.characterType - 1));
    }

    private void OnSelectNext()
    {
        if ((int)IOCPManager.GetInstance.characterType == maxCharacterIndex)
            SelectCharacter((CharacterType)0);
        else
            SelectCharacter((CharacterType)((int)IOCPManager.GetInstance.characterType + 1));
    }

    private void SelectCharacter(CharacterType charType)
    {
        IOCPManager.GetInstance.characterType = charType;
        texCharacter.mainTexture = characterDB.Get(charType).texCharacter;
    }


    #endregion


    #region [ Player Connection Control ]

    public void CheckGameState()
    {
        if (gameState != GameState.Playing)
            return;

        int liver = 0;

        foreach(KeyValuePair<int, PlayerControl> keyval in IOCPManager.clientControlList)
        {
            if (!keyval.Value.isActionDie && keyval.Value.playerState.isLive)
                liver++;
        }

        if (!IOCPManager.myPlayerControl.playerState.isLive)
        {
            gameState = GameState.Result;
            resultState = ResultState.Lose;
            gameEndTrigger = true;
        }
        else if(liver < 2)
        {
            gameState = GameState.Result;
            resultState = ResultState.Win;
            gameEndTrigger = true;
        }
    }

    public void CheckGameState(int winnerId)
    {
        if(winnerId == IOCPManager.senderId)
        {
            gameState = GameState.Result;
            resultState = ResultState.Win;
            gameEndTrigger = true;
        }
        else
        {
            gameState = GameState.Result;
            resultState = ResultState.Lose;
            gameEndTrigger = true;
        }
    }

    #endregion


    #region [ Game End ]

    [Header("[ Game End ]")]
    public UIButton btnGameEnd;

    private void InitGameEnd()
    {
        EventDelegate.Add(btnGameEnd.onClick, Disconnect);
    }

    private void Disconnect()
    {
        ResetGame();
        IOCPManager.GetInstance.Disconnect();
    }

    #endregion


    #region [ Point ]

    [Header("[ Point ]")]
    public UILabel lbPoint;

    public void SetPoint(int initPoint)
    {
        Point = initPoint;
        lbPoint.text = Point.ToString("#,##0");
    }

    public void AddPoint(int addPoint)
    {
        Point += addPoint;
        lbPoint.text = Point.ToString("#,##0");

        IOCPManager.GetInstance.SavePrefabData();
    }

    private void KillOther(int point)
    {
        KillCount++;
        GetInstance.AddPoint(point);
    }
    #endregion


    #region [ Potion ]

    [Header("[ Potion ]")]
    public static int PotionCount = 0;
    public const int PotionPrice = 3;
    public UILabel lbPotionAmount;
    public UIButton btnAddPotion;
    public UIButton btnUsePotion;

    private void InitPotion()
    {
        EventDelegate.Add(btnAddPotion.onClick, AddPotion);
        EventDelegate.Add(btnUsePotion.onClick, UsePotion);
    }

    public void SetPotion(int amount)
    {
        PotionCount = amount;
        UpdatePotionCount();
    }

    private void AddPotion()
    {
        if (Point < PotionPrice)
            return;

        AddPoint(-PotionPrice);
        PotionCount++;
        UpdatePotionCount();
    }

    private void UsePotion()
    {
        if (gameState != GameState.Playing)
            return;

        PotionCount--;
        UpdatePotionCount();

        IOCPManager.GetInstance.SendToServerMessage(new NetworkData()
        {
            senderId = IOCPManager.senderId,
            sendType = SendType.ADDHEALTH,
        });
    }

    private void UpdatePotionCount()
    {
        lbPotionAmount.text = PotionCount.ToString();
        IOCPManager.GetInstance.SavePrefabData();
    }

    #endregion


    // -- HOST---------

    #region [ Play Host ]

    private bool isGameStartEvent = false;
    private int syncObjectID = 100;
    public Dictionary<int, NetworkSyncTransform> networkObjectList = new Dictionary<int, NetworkSyncTransform>();

    private IEnumerator spanwItemCoroutine;
    private IEnumerator spawnEnemyCoroutine;
    private IEnumerator timerCoroutine;

    private void InitHost()
    {
        spanwItemCoroutine = HostSpawnItem();
        spawnEnemyCoroutine = HostSpawnEnemy();
        timerCoroutine = HostTimer();
    }

    public void GamePlay()
    {
        gameState = GameState.Playing;
        isGameStartEvent = true;
    }

    private void ResetHost()
    {
        StopCoroutine(spanwItemCoroutine);
        StopCoroutine(spawnEnemyCoroutine);
        StopCoroutine(timerCoroutine);

        foreach (KeyValuePair<int, NetworkSyncTransform> keyval in networkObjectList)
        {
            if (keyval.Value != null)
                Destroy(keyval.Value.gameObject);
        }

        networkObjectList = new Dictionary<int, NetworkSyncTransform>();
    }

    private void UpdateGameStart()
    {
        if (isGameStartEvent)
        {
            isGameStartEvent = false;

            PlayHost();
        }
    }

    public void PlayHost()
    {
        if (IOCPManager.connectionData.isHost)
        {
            StartCoroutine(spanwItemCoroutine);
            StartCoroutine(spawnEnemyCoroutine);
            StartCoroutine(timerCoroutine);
        }
    }

    private IEnumerator HostSpawnItem()
    {
        float createItemTime = UnityEngine.Random.Range(5.0f, 20.0f);

        while (gameState == GameState.Playing)
        {
            yield return new WaitForSeconds(createItemTime);

            createItemTime = UnityEngine.Random.Range(5.0f, 20.0f);

            CreateRandomItem();
        }
    }

    private IEnumerator HostSpawnEnemy()
    {
        float enemyItemTime = UnityEngine.Random.Range(0.0f, 10.0f);

        while (gameState == GameState.Playing)
        {
            yield return new WaitForSeconds(enemyItemTime);
            enemyItemTime = UnityEngine.Random.Range(20.0f, 30.0f);

            while (networkEnemyValueList.FindAll(x => x.isLive).Count > 4)
                yield return null;

            CreateRandomEnemy();
        }
    }

    private IEnumerator HostTimer()
    {
        while (gameState == GameState.Playing)
        {
            yield return new WaitForSeconds(1.0f);

            gameTimer = gameTimer.Add(TimeSpan.FromSeconds(1));
            SetTimerString();
            SendTimer(gameTimer);
            UpdateArea();
        }
    }

    #endregion


    // -- SPAWN -------

    #region [ Character Spawn ]

    [Header("[ Character Spawn ]")]
    public CharacterDatabase characterDB;
    public Transform[] respawnPoints;
    public GameObject playerObjPrefab;
    public SmoothFollow followCamera;

    public void RespawnCharacter(ClientData clientData)
    {
        if (IOCPManager.clientControlList.ContainsKey(clientData.clientNumber))
            return;

        GameObject playerObj = Instantiate(
            playerObjPrefab,
            respawnPoints[clientData.clientIndex].position,
            respawnPoints[clientData.clientIndex].rotation) as GameObject;

        PlayerControl pControl = playerObj.GetComponent<PlayerControl>();
        pControl.InitCharacter(clientData);
        pControl.netSyncTrans.SetTransform(
            respawnPoints[clientData.clientIndex].position,
            respawnPoints[clientData.clientIndex].eulerAngles);

        if (clientData.isReady)
            pControl.PlayerReady();

        IOCPManager.clientControlList.Add(clientData.clientNumber, pControl);

        if (clientData.isLocalPlayer)
            followCamera.target = pControl.headTrans;

        clientData.isDie = false;
        clientData.isSpawned = true;
    }

    #endregion


    #region [ Enemy Spawn ]

    [Header("[ Enemy Spawn ]")]
    public EnemyDatabase enemyDB;
    public GameObject[] enemyObjPrefab;
    public Dictionary<int, EnemyControl> networkEnemyList = new Dictionary<int, EnemyControl>();
    private List<EnemyControl> networkEnemyValueList = new List<EnemyControl>();

    private void ResetEnemies()
    {
        for (int i = 0; i < networkEnemyValueList.Count; i++)
        {
            if (networkEnemyValueList[i])
                Destroy(networkEnemyValueList[i].gameObject);
        }

        networkEnemyList = new Dictionary<int, EnemyControl>();
        networkEnemyValueList = new List<EnemyControl>();
    }

    private void CreateRandomEnemy()
    {
        syncObjectID++;

        int randEnemyType = UnityEngine.Random.Range(0, enemyObjPrefab.Length);
        EnemyType enemyType = enemyObjPrefab[randEnemyType].GetComponent<EnemyControl>().enemyType;
        Vector3 randPos = new Vector3(UnityEngine.Random.Range(-enemyZoneSize, enemyZoneSize),
                                      enemyZoneHeight,
                                      UnityEngine.Random.Range(-enemyZoneSize, enemyZoneSize));
        Vector3 randRot = Vector3.zero;

        IOCPManager.GetInstance.SendToServerMessage(new NetworkData()
        {
            senderId = IOCPManager.senderId,
            targetId = syncObjectID,
            sendType = SendType.SPAWN_ENEMY,
            enemyType = enemyType,
            position = new NetworkVector()
            {
                x = randPos.x,
                y = randPos.y,
                z = randPos.z
            },
            rotation = new NetworkVector()
            {
                x = randRot.x,
                y = randRot.y,
                z = randRot.z
            }
        });
    }

    private void CreateEnemy(NetworkData enemyData)
    {
        if (syncObjectID < enemyData.targetId)
            syncObjectID = enemyData.targetId + 1;

        GameObject enemyObj = Instantiate(enemyObjPrefab[(int)enemyData.enemyType]) as GameObject;
        enemyObj.transform.position = new Vector3(enemyData.position.x, enemyData.position.y, enemyData.position.z);
        enemyObj.transform.rotation = Quaternion.Euler(new Vector3(enemyData.rotation.x, enemyData.rotation.y, enemyData.rotation.z));

        enemyObj.GetComponent<EnemyControl>().Init(IOCPManager.connectionData.isHost, enemyData.targetId);
        enemyObj.GetComponent<NetworkSyncTransform>().SetObject(enemyData.targetId, IOCPManager.connectionData.isHost);

        networkEnemyList.Add(enemyData.targetId, enemyObj.GetComponent<EnemyControl>());
        networkEnemyValueList.Add(enemyObj.GetComponent<EnemyControl>());
    }

    #endregion


    #region [ Attack Object Spawn ]

    [Header("[ Character Spawn ]")]
    public WeaponDatabase weaponDB;

    public void SendAttack(WeaponType weaponType, Transform attackPoint, float power)
    {
        syncObjectID++;

        IOCPManager.GetInstance.SendToServerMessage(new NetworkData()
        {
            senderId = IOCPManager.senderId,
            targetId = syncObjectID,
            sendType = SendType.ATTACK,
            weaponType = weaponType,
            position = new NetworkVector()
            {
                x = attackPoint.position.x,
                y = attackPoint.position.y,
                z = attackPoint.position.z
            },
            rotation = new NetworkVector()
            {
                x = attackPoint.eulerAngles.x,
                y = attackPoint.eulerAngles.y,
                z = attackPoint.eulerAngles.z
            },
            power = power
        });
    }

    private void CreateAttack(NetworkData attackData)
    {
        if (syncObjectID < attackData.targetId)
            syncObjectID = attackData.targetId + 1;

        if (networkObjectList.ContainsKey(attackData.targetId))
            return;

        WeaponData weaponData = weaponDB.Get(attackData.weaponType);

        GameObject attackObj = Instantiate(weaponData.attackObject) as GameObject;
        attackObj.transform.position = new Vector3(attackData.position.x, attackData.position.y, attackData.position.z);
        attackObj.transform.rotation = Quaternion.Euler(new Vector3(attackData.rotation.x, attackData.rotation.y, attackData.rotation.z));

        attackObj.GetComponent<NetworkSyncTransform>().SetObject(attackData.targetId, IOCPManager.connectionData.isHost);

        attackObj.GetComponent<PlayerAttackObject>().SetAttack(
            attackData.senderId,
            weaponData.attackActiveDuration,
            weaponData.attackObjectSpeed,
            weaponData.damage * attackData.power,
            weaponData.attackUpAmount);

        networkObjectList.Add(attackData.targetId, attackObj.GetComponent<NetworkSyncTransform>());
    }

    public void RemoveNetworkObject(NetworkData netData)
    {
        if (networkObjectList[netData.targetId] != null
            && networkObjectList[netData.targetId].gameObject != null)
            Destroy(networkObjectList[netData.targetId].gameObject);
    }

    #endregion


    #region [ Enemy Attack ]

    public void SendEnemyAttack(EnemyType enemyType, Transform attackPoint)
    {
        if (!IOCPManager.connectionData.isHost)
            return;

        syncObjectID++;

        IOCPManager.GetInstance.SendToServerMessage(new NetworkData()
        {
            senderId = IOCPManager.senderId,
            targetId = syncObjectID,
            sendType = SendType.ENEMY_ATTACK,
            enemyType = enemyType,
            weaponType = WeaponType.HAND,
            position = new NetworkVector()
            {
                x = attackPoint.position.x,
                y = attackPoint.position.y,
                z = attackPoint.position.z
            },
            rotation = new NetworkVector()
            {
                x = attackPoint.eulerAngles.x,
                y = attackPoint.eulerAngles.y,
                z = attackPoint.eulerAngles.z
            },
        });
    }

    private void CreateEnemyAttack(NetworkData attackData)
    {
        if (syncObjectID < attackData.targetId)
            syncObjectID = attackData.targetId + 1;

        EnemyData enemyData = enemyDB.Get(attackData.enemyType);

        GameObject attackObj = Instantiate(enemyData.attackObject) as GameObject;
        attackObj.transform.position = new Vector3(attackData.position.x, attackData.position.y, attackData.position.z);
        attackObj.transform.rotation = Quaternion.Euler(new Vector3(attackData.rotation.x, attackData.rotation.y, attackData.rotation.z));

        attackObj.GetComponent<NetworkSyncTransform>().SetObject(attackData.targetId, IOCPManager.connectionData.isHost);

        attackObj.GetComponent<EnemyAttackObject>().SetAttack(
            enemyData.attackActiveDuration,
            enemyData.attackObjectSpeed,
            enemyData.power);

        networkObjectList.Add(attackData.targetId, attackObj.GetComponent<NetworkSyncTransform>());
    }

    #endregion


    #region [ Item Spawn ]

    [Header("[ Item Spawn ]")]
    public GameObject[] itemObjPrefab;

    private void CreateRandomItem()
    {
        syncObjectID++;

        int randItemType = UnityEngine.Random.Range(0, itemObjPrefab.Length);
        ItemType itemType = itemObjPrefab[randItemType].GetComponent<ItemControl>().itemType;
        Vector3 randPos = new Vector3(UnityEngine.Random.Range(-itemZoneSize, itemZoneSize),
                                      itemZoneHeight,
                                      UnityEngine.Random.Range(-itemZoneSize, itemZoneSize));
        Vector3 randRot = Vector3.zero;

        IOCPManager.GetInstance.SendToServerMessage(new NetworkData()
        {
            senderId = IOCPManager.senderId,
            targetId = syncObjectID,
            sendType = SendType.SPAWN_ITEM,
            itemType = itemType,
            position = new NetworkVector()
            {
                x = randPos.x,
                y = randPos.y,
                z = randPos.z
            },
            rotation = new NetworkVector()
            {
                x = randRot.x,
                y = randRot.y,
                z = randRot.z
            }
        });
    }

    private void CreateItem(NetworkData itemData)
    {
        if (syncObjectID < itemData.targetId)
            syncObjectID = itemData.targetId + 1;

        GameObject itemObj = Instantiate(itemObjPrefab[(int)itemData.itemType]) as GameObject;
        itemObj.transform.position = new Vector3(itemData.position.x, itemData.position.y, itemData.position.z);
        itemObj.transform.rotation = Quaternion.Euler(new Vector3(itemData.rotation.x, itemData.rotation.y, itemData.rotation.z));

        itemObj.GetComponent<NetworkSyncTransform>().SetObject(itemData.targetId, IOCPManager.connectionData.isHost);

        networkObjectList.Add(itemData.targetId, itemObj.GetComponent<NetworkSyncTransform>());
    }

    #endregion


    // -- AREA -------

    #region [ Timer Update ]

    [Header("[ Timer Update ]")]
    public UILabel lbTimer;

    private void SendTimer(TimeSpan timer)
    {
        IOCPManager.GetInstance.SendToServerMessage(new NetworkData()
        {
            senderId = IOCPManager.senderId,
            sendType = SendType.GAMETIMER,
            power = (float)timer.TotalSeconds
        });
    }

    private void SetTimer(NetworkData netData)
    {
        gameTimer = TimeSpan.FromSeconds(netData.power);
        SetTimerString();
    }

    private void SetTimerString()
    {
        lbTimer.text = string.Concat(
            gameTimer.Minutes.ToString("00"),
            ":",
            gameTimer.Seconds.ToString("00"));
    }

    #endregion


    #region [ Deactive Area ]

    [Header("[ Deactive Area ]")]
    public Area[] areas;
    public UILabel lbAlarm;
    private Area nextDeactivateArea;

    public GameObject winnerPointPrefab;
    private GameObject winnerPointInstance;

    private void ResetArea()
    {
        for (int i = 0; i < areas.Length; i++)
            areas[i].Reset();
    }

    private void UpdateArea()
    {
        if (gameTimer.TotalSeconds >=       //20 Minute
            //80)
            1200)  
        {
            if (Array.FindAll(areas, x=> !x.isActivate).Length < 4)
                SendAreaDeactive();
        }
        else if (gameTimer.TotalSeconds >=  //18 Minute - Alarm
            //70)
            1080) 
        {
            if (Array.FindAll(areas, x => !x.isAlarmed).Length < 4)
            {
                SendAreaDeactiveAlarm();
                SendCreateWinnerPoint();
            }
        }
        else if (gameTimer.TotalSeconds >=  //15 Minute
            //60)
            900) 
        {
            if (Array.FindAll(areas, x => !x.isActivate).Length < 3)
                SendAreaDeactive();
        }
        else if (gameTimer.TotalSeconds >=  //13 Minute - Alarm
            //50)
            780) 
        {
            if (Array.FindAll(areas, x => x.isAlarmed).Length < 3)
                SendAreaDeactiveAlarm();
        }
        else if (gameTimer.TotalSeconds >=  //10 Minute
            //40)
            600) 
        {
            if (Array.FindAll(areas, x => !x.isActivate).Length < 2)
                SendAreaDeactive();
        }
        else if (gameTimer.TotalSeconds >=  //8 Minute - Alarm
            //30)
            480) 
        {
            if (Array.FindAll(areas, x => x.isAlarmed).Length < 2)
                SendAreaDeactiveAlarm();
        }
        else if (gameTimer.TotalSeconds >=  //5 Minute
            //20) 
            300) 
        {
            if (Array.FindAll(areas, x => !x.isActivate).Length < 1)
                SendAreaDeactive();
        }
        else if (gameTimer.TotalSeconds >=  //3 Minute - Alarm
            //10)
            180) 
        {
            if (Array.FindAll(areas, x => x.isAlarmed).Length < 1)
                SendAreaDeactiveAlarm();
        }
    }

    private void SendAreaDeactiveAlarm()
    {
        Area[] activeAreas = Array.FindAll(areas, x => x.isActivate);
        nextDeactivateArea = activeAreas[UnityEngine.Random.Range(0, activeAreas.Length)];
        nextDeactivateArea.Alarm();

        IOCPManager.GetInstance.SendToServerMessage(new NetworkData()
        {
            senderId = IOCPManager.senderId,
            message = nextDeactivateArea.areaNumber.ToString(),
            sendType = SendType.ALARM
        });
    }

    private void AreaDeactiveAlarm(NetworkData netData)
    {
        nextDeactivateArea = Array.Find(areas, x => x.areaNumber == int.Parse(netData.message));
        nextDeactivateArea.Alarm();
        lbAlarm.text = string.Concat("경고! 2분 후 ", nextDeactivateArea.areaNumber, "구역이 닫힙니다");
        StartCoroutine(CloseAreaDeactiveAlarm());
    }

    private IEnumerator CloseAreaDeactiveAlarm()
    {
        yield return new WaitForSeconds(5.0f);
        lbAlarm.text = "";
    }

    private void SendAreaDeactive()
    {
        nextDeactivateArea.Deactivate();
        IOCPManager.GetInstance.SendToServerMessage(new NetworkData()
        {
            senderId = IOCPManager.senderId,
            message = nextDeactivateArea.areaNumber.ToString(),
            sendType = SendType.DEACTIVATEAREA
        });
    }

    private void DeactivateArea(NetworkData netData)
    {
        Array.Find(areas, x => x.areaNumber == int.Parse(netData.message)).Deactivate();
    }

    private void SendCreateWinnerPoint()
    {
        Transform randWinnerPointTrans = nextDeactivateArea.winnerPoints[UnityEngine.Random.Range(0, nextDeactivateArea.winnerPoints.Length)];

        IOCPManager.GetInstance.SendToServerMessage(new NetworkData()
        {
            senderId = IOCPManager.senderId,
            sendType = SendType.SPAWN_WINNERPOINT,
            position = new NetworkVector()
            {
                x = randWinnerPointTrans.position.x,
                y = randWinnerPointTrans.position.y,
                z = randWinnerPointTrans.position.z
            }
        });
    }

    private void CreateWinnerPoint(NetworkData netData)
    {
        winnerPointInstance = Instantiate(winnerPointPrefab) as GameObject;
        winnerPointInstance.transform.position = 
            new Vector3(netData.position.x, netData.position.y, netData.position.z);
    }

    #endregion
}

public enum GameState
{
    NoConnect,
    Lobby,
    Playing,
    Result
}


public enum ResultState
{
    Win = 0,
    Lose
}
