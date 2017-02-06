using UnityEngine;
using System.Collections;

public class PlayerControl : MonoBehaviour
{
    public CharacterType characterType;
    public CharacterDatabase characterDB;

    public bool isAttacking = false;

    private CharacterData characterData;

    
    private PlayerTransform playerTransform;
    private PlayerAnimator playerAnimator;
    private PlayerAttack playerAttack;

    void Awake()
    {
        characterData = characterDB.Get(characterType);

        playerTransform = GetComponent<PlayerTransform>();
        playerAnimator = GetComponent<PlayerAnimator>();
        playerAttack = GetComponent<PlayerAttack>();
    }

    void Start()
    {
        playerTransform.InitTransform(characterData);
        playerAnimator.InitAnimator();
        playerAttack.InitWeapon();
    }

    void Update()
    {
        if(Input.GetButton(GlobalData.BUTTON_FIRE) 
            && playerAttack.CanAttack)
        {
            playerAttack.Attack();
            playerAnimator.AttackAnimation(playerAttack.GetWeaponType());
        }

        if(Input.GetButtonDown(GlobalData.BUTTON_JUMP) 
            && playerTransform.IsGround)
        {
            playerTransform.Jump();
            playerAnimator.JumpAnimation();
        }

        if (!playerAnimator.IsAttacking)
            playerTransform.UpdateTransform();
        playerAnimator.UpdateAnimator();
    }
}
