// 플레이어 루트에 부착될 컴포넌트
// 플레이어 컴포넌트 전체를 관리 - 최상의 컨트롤

using UnityEngine;
using System.Collections;

public class PlayerControl : MonoBehaviour
{
    [Header("[ Character Type ]")]
    // 캐릭터 데이터베이스 : Assets/Data/CharacterDB
    public CharacterDatabase characterDB;
    // 캐릭터 데이터베이스에서 가져올 타입
    public CharacterType characterType;
    // 캐릭터 데이터베이스에서 가져올 Data를 할당할 변수
    private CharacterData characterData;

    public GameObject[] characterObjs;
    private GameObject currentCharacterObj;
    private PlayerItemSet itemSet;

    public PlayerState playerState;
    public ClientData clientData;
    public Transform headTrans;
    
    // 하위 플레이어 컴포넌트들
    private PlayerTransform playerTransform;
    private PlayerAnimator playerAnimator;
    private PlayerAttack playerAttack;
    private PlayerInfo playerInfo;
    private PlayerLobbyInfo playerLobbyInfo;
    [HideInInspector]
    public NetworkSyncTransform netSyncTrans;
    [HideInInspector]
    public NetworkSyncAnimator netSyncAnimator;

    public bool isActionDie = false;
    public bool isActionDisconnect = false;


    void Awake()
    {
        // 하위 컴포넌트들 할당
        playerTransform = GetComponent<PlayerTransform>();
        playerAnimator = GetComponent<PlayerAnimator>();
        playerAttack = GetComponent<PlayerAttack>();
        netSyncTrans = GetComponent<NetworkSyncTransform>();
        netSyncAnimator = GetComponent<NetworkSyncAnimator>();
        // ----- //
    }

    public void InitCharacter(ClientData _clientData)
    {
        clientData = _clientData;

        characterType = clientData.characterType;
        for(int i = 0; i < characterObjs.Length; i++)
        {
            if (i == (int)characterType)
            {
                characterObjs[i].SetActive(true);
                currentCharacterObj = characterObjs[i];
            }
            else
                characterObjs[i].SetActive(false);
        }
        Animator characterAnim = currentCharacterObj.GetComponent<Animator>();
        itemSet = currentCharacterObj.GetComponent<PlayerItemSet>();
        itemSet.ActiveWeapon(WeaponType.HAND);

        // 캐릭터 데이터 할당
        characterData = characterDB.Get(characterType);

        // 하위 컴포넌트들 초기화
        playerTransform.InitTransform(characterData);
        playerAnimator.InitAnimator(characterAnim);
        playerAttack.InitAttack(characterData.power);

        playerState.maxHealth = characterData.health;
        playerState.currentHealth = characterData.health;
        playerState.isAttacking = false;
        playerState.isLive = true;

        playerInfo = UIManager.GetInstance.AddPlayerInfo(characterData, clientData);
        playerLobbyInfo = UIManager.GetInstance.AddPlayerLobbyInfo(clientData);

        netSyncTrans.isLocalPlayer = clientData.isLocalPlayer;
        netSyncAnimator.Init(playerAnimator.GetAnimator(), clientData.clientNumber, clientData.isLocalPlayer);

        isActionDie = false;

        //gameObject.layer = clientData.isLocalPlayer ? LayerMask.NameToLayer(GlobalData.LAYER_PLAYER) : LayerMask.NameToLayer(GlobalData.LAYER_ENEMY);
        //gameObject.tag = clientData.isLocalPlayer ? GlobalData.TAG_PLAYER : GlobalData.TAG_ENEMY;

        // ----- //
    }

    public void UpdateClientData(ClientData _clientData)
    {
        playerInfo.UpdatePlayerInfo(_clientData);
        playerLobbyInfo.UpdateLobbyInfo(_clientData);
    }

    void Update()
    {
        if (isActionDie)
        {
            isActionDie = false;
            Die();
        }

        if (isActionDisconnect)
        {
            isActionDisconnect = false;
            Disconnect();
        }

        if (GameManager.gameState != GameState.Playing)
            return;

        if (!playerState.isLive || !clientData.isLocalPlayer)
            return;

        // 공격 가능시
        if(Input.GetButton(GlobalData.BUTTON_FIRE) 
            && playerAttack.CanAttack)
        {
            // 플레이어 공격 컴포넌트에 공격 전달
            StartCoroutine(playerAttack.Attack());
            // 플레이어 애니메이터 컴포넌트에 공격 전달
            playerAnimator.AttackAnimation(playerAttack.GetWeaponType());
            netSyncAnimator.AttackAnimation(playerAttack.GetWeaponType());
        }

        // 점프 가능시
        if(Input.GetButtonDown(GlobalData.BUTTON_JUMP) 
            && playerTransform.IsGround)
        {
            // 플레이어 이동 컴포넌트에 점프 전달
            playerTransform.Jump();
            // 플레이어 애니메이터 컴포넌트에 점프 전달
            playerAnimator.JumpAnimation();
            netSyncAnimator.JumpAnimation();
        }

        // 공격 중이 아닐때
        if (!playerAnimator.IsAttacking)
        {
            // 플레이어 이동 컴포넌트 업데이트
            playerTransform.UpdateTransform();
        }
        // 플레이어 애니메이터 컴포넌트 업데이트
        playerAnimator.UpdateAnimator();
    }

    public void LossHealth(float amount, int attackerId)
    {
        if (!playerState.isLive)
            return;

        playerState.currentHealth = Mathf.Clamp(playerState.currentHealth - amount, 0, playerState.maxHealth);
        playerState.isLive = playerState.currentHealth > 0;

        playerInfo.SetHealth(playerState.currentHealth / characterData.health);

        if (!playerState.isLive && clientData.isLocalPlayer)
        {
            IOCPManager.GetInstance.SendToServerMessage(new NetworkData()
            {
                senderId = clientData.clientNumber,
                targetId = attackerId,
                sendType = SendType.DIE
            });
        }
    }

    public void SetWeapon(WeaponType getWeaponType)
    {
        playerAttack.GetWeapon(getWeaponType);
        itemSet.ActiveWeapon(getWeaponType);
    }

    public void AddHealth(float amount)
    {
        playerState.currentHealth = Mathf.Clamp(playerState.currentHealth + amount, 0, playerState.maxHealth);
        playerInfo.SetHealth(playerState.currentHealth / characterData.health);
    }

    public void DoActionDie()
    {
        isActionDie = true;
    }

    private void Die()
    {
        playerInfo.SetDie(); 

        playerAnimator.DieAnimation();
        netSyncAnimator.DieAnimation();
    }

    public bool IsPlayerReady
    {
        get { return playerLobbyInfo.clientData.isReady; }
    }

    public void PlayerReady()
    {
        playerLobbyInfo.ClientReady();
    }

    public void AllPlayerReady(bool isAllReady)
    {
        if (clientData.isLocalPlayer)
            playerLobbyInfo.AllReady(isAllReady);
    }

    public void DoActionDisconnect()
    {
        isActionDisconnect = true;
    }

    public void HideInfos()
    {
        UIManager.GetInstance.RemovePlayerLobbyInfo(clientData);
    }

    private void Disconnect()
    {
        HideInfos();

        if (GameManager.gameState == GameState.Playing)
        {
            playerState.isLive = false;
            playerInfo.HideHost();

            IOCPManager.GetInstance.SendToServerMessage(new NetworkData()
            {
                senderId = clientData.clientNumber,
                sendType = SendType.DIE
            });
        }
        else if(GameManager.gameState == GameState.Lobby || GameManager.gameState == GameState.NoConnect)
        {
            Destroy(playerInfo.gameObject);
            Destroy(gameObject);
        }
    }
}
