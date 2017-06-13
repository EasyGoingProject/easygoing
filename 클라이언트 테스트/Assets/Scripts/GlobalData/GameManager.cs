using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.Utility;

public class GameManager : Singleton<GameManager>
{
    public static GameState gameState = GameState.NoConnect;
    public static ResultState resultState = ResultState.Win;
    public static int KillCount = 0;
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

        ResetGame();
    }

    private void Update()
    {
        UpdateGameStart();
    }

    public void ResetGame()
    {
        gameState = GameState.NoConnect;

        gameEndTrigger = false;

        ResetDataStack();
        ResetHost();
        ResetEnemies();
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
    public List<NetworkData> attackDataList;
    public List<NetworkData> itemDataList;
    public List<NetworkData> removeObjectList;
    public List<NetworkData> enemyDataList;
    public List<NetworkData> enemyAttackDataList;

    private void ResetDataStack()
    {
        nonRespawnedClientList = new List<ClientData>();
        attackDataList = new List<NetworkData>();
        itemDataList = new List<NetworkData>();
        removeObjectList = new List<NetworkData>();
        enemyDataList = new List<NetworkData>();
        enemyAttackDataList = new List<NetworkData>();
    }

    private void FixedUpdate()
    {
        if (IOCPManager.GetInstance.client != null
            && IOCPManager.GetInstance.client.connectState == TCPClient.ConnectionState.Connected)
        {
            if (gameEndTrigger)
            {
                gameEndTrigger = false;
                UIManager.GetInstance.ShowPanel(PanelType.Result);
            }

            if (IOCPManager.clientDataList.FindAll(x => !x.isSpawned).Count > 0)
            {
                nonRespawnedClientList = IOCPManager.clientDataList.FindAll(x => !x.isSpawned);

                for (int i = 0; i < nonRespawnedClientList.Count; i++)
                    RespawnCharacter(nonRespawnedClientList[i]);
            }

            if(attackDataList.Count > 0)
            {
                for(int i = 0; i < attackDataList.Count; i++)
                    CreateAttack(attackDataList[i]);

                attackDataList = new List<NetworkData>();
            }

            if(itemDataList.Count > 0)
            {
                for (int i = 0; i < itemDataList.Count; i++)
                    CreateItem(itemDataList[i]);

                itemDataList = new List<NetworkData>();
            }

            if (enemyDataList.Count > 0)
            {
                for (int i = 0; i < enemyDataList.Count; i++)
                    CreateEnemy(enemyDataList[i]);

                enemyDataList = new List<NetworkData>();
            }

            if (enemyAttackDataList.Count > 0)
            {
                for (int i = 0; i < enemyAttackDataList.Count; i++)
                    CreateEnemyAttack(enemyAttackDataList[i]);

                enemyAttackDataList = new List<NetworkData>();
            }

            if (removeObjectList.Count > 0)
            {
                for (int i = 0; i < removeObjectList.Count; i++)
                    RemoveNetworkObject(removeObjectList[i]);

                removeObjectList = new List<NetworkData>();
            }
        }
    }

    #endregion 


    #region [ Play Host ]

    private bool isGameStartEvent = false;
    private int syncObjectID = 100;
    public Dictionary<int, NetworkSyncTransform> networkObjectList = new Dictionary<int, NetworkSyncTransform>();

    private IEnumerator spanwItemCoroutine;
    private IEnumerator spawnEnemyCoroutine;

    private void InitHost()
    {
        spanwItemCoroutine = HostSpawnItem();
        spawnEnemyCoroutine = HostSpawnEnemy();
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

        foreach(KeyValuePair<int, NetworkSyncTransform> keyval in networkObjectList)
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
        }
    }

    #endregion


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
            followCamera.target = playerObj.transform;

        clientData.isDie = false;
        clientData.isSpawned = true;
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


    #region [ Item Spawn ]

    [Header("[ Item Spawn ]")]
    public GameObject[] itemObjPrefab;

    private IEnumerator HostSpawnItem()
    {
        float createItemTime = Random.Range(5.0f, 20.0f);

        while (gameState == GameState.Playing)
        {
            yield return new WaitForSeconds(createItemTime);

            createItemTime = Random.Range(5.0f, 20.0f);

            CreateRandomItem();
        }
    }

    private void CreateRandomItem()
    {
        syncObjectID++;

        int randItemType = Random.Range(0, itemObjPrefab.Length);
        ItemType itemType = itemObjPrefab[randItemType].GetComponent<ItemControl>().itemType;
        Vector3 randPos = new Vector3(Random.Range(-itemZoneSize, itemZoneSize),
                                      itemZoneHeight,
                                      Random.Range(-itemZoneSize, itemZoneSize));
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

    private IEnumerator HostSpawnEnemy()
    {
        float enemyItemTime = Random.Range(0.0f, 10.0f);

        while (gameState == GameState.Playing)
        {
            yield return new WaitForSeconds(enemyItemTime);
            enemyItemTime = Random.Range(20.0f, 30.0f);

            while (networkEnemyValueList.FindAll(x => x.isLive).Count > 4)
                yield return null;

            CreateRandomEnemy();
        }
    }

    private void CreateRandomEnemy()
    {
        syncObjectID++;

        int randEnemyType = Random.Range(0, enemyObjPrefab.Length);
        EnemyType enemyType = enemyObjPrefab[randEnemyType].GetComponent<EnemyControl>().enemyType;
        Vector3 randPos = new Vector3(Random.Range(-enemyZoneSize, enemyZoneSize),
                                      enemyZoneHeight,
                                      Random.Range(-enemyZoneSize, enemyZoneSize));
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
        if((int)IOCPManager.GetInstance.characterType == 0)
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


    #region [ Game End ]

    [Header("[ Game End ]")]
    public UIButton btnGameEnd;

    private void InitGameEnd()
    {
        EventDelegate.Add(btnGameEnd.onClick, IOCPManager.GetInstance.Disconnect);
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
