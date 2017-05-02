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

    public PlayerState playerState;
    public ClientData clientData;
    
    // 하위 플레이어 컴포넌트들
    private PlayerTransform playerTransform;
    private PlayerAnimator playerAnimator;
    private PlayerAttack playerAttack;
    private PlayerInfo playerInfo;
    [HideInInspector]
    public NetworkSyncTransform netSyncTrans;
    [HideInInspector]
    public NetworkSyncAnimator netSyncAnimator;


    void Awake()
    {
        // 캐릭터 데이터 할당
        characterData = characterDB.Get(characterType);

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

        // 하위 컴포넌트들 초기화
        playerTransform.InitTransform(characterData);
        playerAnimator.InitAnimator();
        playerAttack.InitAttack(characterData.power);

        playerState.currentHealth = characterData.health;
        playerState.isAttacking = false;
        playerState.isLive = true;

        playerInfo = UIManager.GetInstance.AddPlayerInfo(characterData, _clientData);

        netSyncTrans.isLocalPlayer = clientData.isLocalPlayer;
        netSyncAnimator.Init(playerAnimator.GetAnimator(), clientData.clientNumber, clientData.isLocalPlayer);

        //gameObject.layer = clientData.isLocalPlayer ? LayerMask.NameToLayer(GlobalData.LAYER_PLAYER) : LayerMask.NameToLayer(GlobalData.LAYER_ENEMY);
        //gameObject.tag = clientData.isLocalPlayer ? GlobalData.TAG_PLAYER : GlobalData.TAG_ENEMY;

        // ----- //
    }

    void Update()
    {
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

    public void LossHealth(float amount)
    {
        playerState.currentHealth = Mathf.Clamp(playerState.currentHealth - amount, 0, playerState.currentHealth);
        playerState.isLive = playerState.currentHealth > 0;

        playerInfo.SetHealth(playerState.currentHealth / characterData.health);

        if (!playerState.isLive && clientData.isLocalPlayer)
            Die();
    }

    private void Die()
    {
        playerAnimator.DieAnimation();
        netSyncAnimator.DieAnimation();
    }
}
