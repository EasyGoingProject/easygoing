using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
public struct NetworkData
{
    public int senderId;
    public int targetId;
    public CharacterType characterType;
    public SendType sendType;
    public string message;
    public float life;
    public float power;
    public WeaponType weaponType;
    public NetworkVector position;
    public NetworkVector rotation;
    public NetworkAnimator animator;
}

[Serializable]
public struct NetworkVector
{
    public float x;
    public float y;
    public float z;
}

[Serializable]
public struct NetworkQuaternion
{
    public float x;
    public float y;
    public float z;
    public float w;
}

[Serializable]
public struct NetworkAnimator
{
    public float move;
    public bool attackNormal;
    public bool attackSpear;
    public bool attackBow;
    public bool attackThrow;
    public bool jump;
    public bool die;
}

public enum SendType
{
    RESPONSE = 0,
    JOIN = 1,
    SYNCTRANSFORM = 2,
    ANIMATOR_MOVE = 3,
    ANIMATOR_TRIGGER = 4,
    ATTACK = 5,
    HIT = 6,
    DIE = 7,
    MESSAGE = 10
}

