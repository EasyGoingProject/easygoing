// 플레이어의 애니메이터 정보 컴포넌트

using UnityEngine;
using System.Collections;

public class PlayerAnimator : MonoBehaviour
{
    // 초기화 상태 확인
    private bool isInit = false;

    // 애니메이터 할당
    [SerializeField]
    private Animator playerAnimator;


    #region [ Init ]

    // 애니메이터 초기화
    public void InitAnimator()
    {
        isInit = true;
    }

    public Animator GetAnimator()
    {
        return playerAnimator;
    }

    #endregion


    #region [ Update ]

    // 애니메이터 업데이트
    public void UpdateAnimator()
    {
        if (!isInit)
            return;

        // 이동중인지 확인 후 이동중일 때 뛰는 모션 플레이
        playerAnimator.SetFloat(GlobalData.ANIMATOR_PARAM_MOVE, InputControl.isMoving ? 0.3f : 0);
    }

    // 재생중인 애니메이션이 공격 애니메이션인지 확인
    public bool IsAttacking
    {
        get { return playerAnimator.GetCurrentAnimatorStateInfo(0).IsTag(GlobalData.ANIMATOR_TAG_ATTACK); }
    }

    #endregion


    #region [ Animate ]

    // 공격 애니메이션 재생
    // 무기마다 애니메이션이 다르기 때문에 무기타입 파라미터로 가져옴
    public void AttackAnimation(WeaponType weaponType)
    {
        switch (weaponType)
        {
            case WeaponType.HAND:
                playerAnimator.SetTrigger(GlobalData.TRIGGER_ATTACK_HAND);
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

    // 점프 애니메이션 재생
    public void JumpAnimation()
    {
        playerAnimator.SetTrigger(GlobalData.TRIGGER_JUMP);
    }

    // 사망 애니메이션 재생
    public void DieAnimation()
    {
        playerAnimator.SetTrigger(GlobalData.TRIGGER_DIE);
    }

    #endregion
}
