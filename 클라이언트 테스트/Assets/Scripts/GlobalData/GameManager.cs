using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.Utility;

public class GameManager : Singleton<GameManager>
{
    private void Awake()
    {
        Application.runInBackground = true;

        PhysicsLayerSetting();
    }


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

    private List<ClientData> nonRespawnedClientList;
    public List<NetworkData> attackDataList;

    private void FixedUpdate()
    {
        if (IOCPManager.GetInstance.client != null
            && IOCPManager.GetInstance.client.connectState == TCPClient.ConnectionState.Connected)
        {
            if (IOCPManager.clientDataList.FindAll(x => !x.isSpawned).Count > 0)
            {
                nonRespawnedClientList = IOCPManager.clientDataList.FindAll(x => !x.isSpawned);
                RespawnCharacter(nonRespawnedClientList[0]);
            }

            if(attackDataList.Count > 0)
            {
                for(int i = 0; i < attackDataList.Count; i++)
                    CreateAttack(attackDataList[i]);

                attackDataList = new List<NetworkData>();
            }
        }
    }

    #region [ Character Spawn ]

    [Header("[ Character Spawn ]")]
    public CharacterDatabase characterDB;
    public Transform[] respawnPoints;
    public GameObject playerObjPrefab;
    public SmoothFollow followCamera;

    public void RespawnCharacter(ClientData clientData)
    {
        GameObject playerObj = Instantiate(
            playerObjPrefab,
            respawnPoints[clientData.clientIndex].position,
            respawnPoints[clientData.clientIndex].rotation) as GameObject;

        PlayerControl pControl = playerObj.GetComponent<PlayerControl>();
        pControl.InitCharacter(clientData);
        pControl.netSyncTrans.SetTransform(
            respawnPoints[clientData.clientIndex].position,
            respawnPoints[clientData.clientIndex].eulerAngles);

        IOCPManager.clientControlList.Add(clientData.clientNumber, pControl);

        if (clientData.isLocalPlayer)
            followCamera.target = playerObj.transform;

        clientData.isDie = false;
        clientData.isSpawned = true;
    }

    #endregion


    #region [ Attack Object Spawn ]

    [Header("[ Character Spawn ]")]
    public WeaponDatabase weaponDB;


    private void CreateAttack(NetworkData attackData)
    {
        WeaponData weaponData = weaponDB.Get(attackData.weaponType);

        GameObject attackObj = Instantiate(weaponData.attackObject) as GameObject;
        attackObj.transform.position = new Vector3(attackData.position.x, attackData.position.y, attackData.position.z);
        attackObj.transform.rotation = Quaternion.Euler(new Vector3(attackData.rotation.x, attackData.rotation.y, attackData.rotation.z));

        attackObj.GetComponent<PlayerAttackObject>().SetAttack(
            attackData.senderId,
            weaponData.attackActiveDuration,
            weaponData.attackObjectSpeed,
            weaponData.damage * attackData.power);
    }

    #endregion
}
