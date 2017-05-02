using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NetworkSyncAnimator : MonoBehaviour
{
    [SerializeField]
    private float positionLerpRate = 0.05f;
    [SerializeField]
    private float moveThreshold = 0.05f;
    private Animator playerAnimator;

    public bool isLocalPlayer;
    public NetworkData netData;
    private List<string> animTriggerStrList = new List<string>();

    private bool isInit = false;

    public void Init(Animator _animator, int _senderId, bool _isLocalPlayer)
    {
        playerAnimator = _animator;

        netData = new NetworkData();
        netData.senderId = _senderId;
        netData.sendType = SendType.ANIMATOR_MOVE;

        isLocalPlayer = _isLocalPlayer;

        isInit = true;
    }

    public void SetAnimatorMove(NetworkData _netData)
    {
        netData.animator.move = _netData.animator.move;
    }

    void Update()
    {
        if (!isInit)
            return;

        if (isLocalPlayer)
            return;

        playerAnimator.SetFloat(GlobalData.ANIMATOR_PARAM_MOVE, netData.animator.move);

        if (animTriggerStrList.Count > 0)
        {
            playerAnimator.SetTrigger(animTriggerStrList[0]);
            animTriggerStrList.RemoveAt(0);
        }
    }

    void FixedUpdate()
    {
        if (!isInit)
            return;

        if (!isLocalPlayer)
            return;

        if (IsMoveAmount())
        {
            netData.animator.move = playerAnimator.GetFloat(GlobalData.ANIMATOR_PARAM_MOVE);
            IOCPManager.GetInstance.SendToServerMessage(netData);
        }
    }

    private bool IsMoveAmount()
    {
        return Mathf.Abs(playerAnimator.GetFloat(GlobalData.ANIMATOR_PARAM_MOVE) - netData.animator.move) > moveThreshold;
    }

    public void NetworkReceiveTrigger(NetworkData _netData)
    {
        if (_netData.animator.attackNormal)
            animTriggerStrList.Add(GlobalData.TRIGGER_ATTACK_HAND);

        if (_netData.animator.attackSpear)
            animTriggerStrList.Add(GlobalData.TRIGGER_ATTACK_SPEAR);

        if (_netData.animator.attackBow)
            animTriggerStrList.Add(GlobalData.TRIGGER_ATTACK_BOW);

        if (_netData.animator.attackThrow)
            animTriggerStrList.Add(GlobalData.TRIGGER_ATTACK_THROW);

        if (_netData.animator.jump)
            animTriggerStrList.Add(GlobalData.TRIGGER_JUMP);

        if (_netData.animator.die)
            animTriggerStrList.Add(GlobalData.TRIGGER_DIE);
    }


    public void AttackAnimation(WeaponType weaponType)
    {
        NetworkAnimator netAnimator = new NetworkAnimator()
        {
            attackBow = false,
            attackNormal = false,
            attackSpear = false,
            attackThrow = false,
            die = false,
            jump = false
        };

        switch (weaponType)
        {
            case WeaponType.HAND:
                netAnimator.attackNormal = true;
                break;

            case WeaponType.SPEAR:
                netAnimator.attackSpear = true;
                break;

            case WeaponType.BOW:
                netAnimator.attackBow = true;
                break;

            case WeaponType.THROW:
                netAnimator.attackThrow = true;
                break;
        }

        IOCPManager.GetInstance.SendToServerMessage(new NetworkData()
        {
            senderId = netData.senderId,
            sendType = SendType.ANIMATOR_TRIGGER,
            animator = netAnimator
        });
    }

    public void JumpAnimation()
    {
        NetworkData netAnimTrigger = new NetworkData()
        {
            senderId = netData.senderId,
            sendType = SendType.ANIMATOR_TRIGGER,
            animator = new NetworkAnimator()
            {
                attackBow = false,
                attackNormal = false,
                attackSpear = false,
                attackThrow = false,
                die = false,
                jump = true
            }
        };

        IOCPManager.GetInstance.SendToServerMessage(netAnimTrigger);
    }

    public void DieAnimation()
    {
        IOCPManager.GetInstance.SendToServerMessage(new NetworkData()
        {
            senderId = netData.senderId,
            sendType = SendType.ANIMATOR_TRIGGER,
            animator = new NetworkAnimator()
            {
                attackBow = false,
                attackNormal = false,
                attackSpear = false,
                attackThrow = false,
                die = true,
                jump = false
            }
        });
    }
}
