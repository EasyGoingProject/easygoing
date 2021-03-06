﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NetworkSyncAnimator : MonoBehaviour
{
    public ObjectType objectType = ObjectType.Player;
    public int objectNetworkId = 0;

    [SerializeField]
    private float positionLerpRate = 0.05f;
    [SerializeField]
    private float moveThreshold = 0.05f;
    private Animator playerAnimator;

    public bool isLocalPlayer;
    public NetworkData netData;
    private List<string> animTriggerStrList = new List<string>();

    private bool isInit = false;

    public void Init(Animator _animator, int _networkId, bool _isLocalPlayer)
    {
        playerAnimator = _animator;

        netData = new NetworkData();
        netData.senderId = _networkId;
        
        if (objectType == ObjectType.Player)
        {
            netData.sendType = SendType.ANIMATOR_MOVE;
        }
        else if (objectType == ObjectType.Enemy)
        {
            objectNetworkId = _networkId;
            netData.targetId = objectNetworkId;
            netData.sendType = SendType.ENEMY_ANIMATOR_MOVE;
        }

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

        if (objectType == ObjectType.Player)
        {
            IOCPManager.GetInstance.SendToServerMessage(new NetworkData()
            {
                senderId = netData.senderId,
                sendType = SendType.ANIMATOR_TRIGGER,
                animator = netAnimator
            });
        }
        else if(objectType == ObjectType.Enemy)
        {
            IOCPManager.GetInstance.SendToServerMessage(new NetworkData()
            {
                senderId = netData.senderId,
                targetId = objectNetworkId,
                sendType = SendType.ENEMY_ANIMATOR_TRIGGER,
                animator = netAnimator
            });
        }
    }

    public void JumpAnimation()
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
                die = false,
                jump = true
            }
        });
    }

    public void DieAnimation()
    {
        if (objectType == ObjectType.Player)
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
        else if(objectType == ObjectType.Enemy)
        {
            IOCPManager.GetInstance.SendToServerMessage(new NetworkData()
            {
                senderId = netData.senderId,
                sendType = SendType.ENEMY_ANIMATOR_TRIGGER,
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
}
