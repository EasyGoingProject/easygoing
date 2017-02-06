using UnityEngine;
using System.Collections;

public class PlayerAnimator : MonoBehaviour
{
    private bool isInit = false;

    [SerializeField]
    private Animator playerAnimator;

    public void InitAnimator()
    {
        isInit = true;
    }

    public void UpdateAnimator()
    {
        if (!isInit)
            return;

        playerAnimator.SetFloat(GlobalData.ANIMATOR_PARAM_MOVE, InputControl.isMoving ? 0.3f : 0);
   }

    public void AttackAnimation(WeaponType weaponType)
    {
        switch (weaponType)
        {
            case WeaponType.NONE:
                playerAnimator.SetTrigger(GlobalData.TRIGGER_ATTACK_NONE);
                break;

            case WeaponType.SPEAR:
                playerAnimator.SetTrigger(GlobalData.TRIGGER_ATTACK_SPEAR);
                break;

            case WeaponType.BOW:
                playerAnimator.SetTrigger(GlobalData.TRIGGER_ATTACK_BOW);
                break;

            case WeaponType.THROW:
                playerAnimator.SetTrigger(GlobalData.TRIGGER_ATTACK_THROW);
                break;
        }
    }

    public void JumpAnimation()
    {
        playerAnimator.SetTrigger(GlobalData.TRIGGER_JUMP);
    }

    public bool IsAttacking
    {
        get { return playerAnimator.GetCurrentAnimatorStateInfo(0).IsTag(GlobalData.ANIMATOR_TAG_ATTACK); }
    }
}
