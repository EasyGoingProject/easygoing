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
    public ItemType itemType;
    public EnemyType enemyType;
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
    ADDHEALTH = 12,
    DIE = 7,

    READY = 8,
    GAMESTART = 9,

    MESSAGE = 10,

    EQUIPWEAPON = 11,

    OBJECT_SYNC_TRANSFORM = 14,
    DESTORY_OBJECT = 15,

    SPAWN_ITEM = 13,
    SPAWN_ENEMY = 16,

    ENEMY_SYNC_TRANSFORM = 17,
    ENEMY_ANIMATOR_MOVE = 18,
    ENEMY_ANIMATOR_TRIGGER = 19,
    ENEMY_ATTACK = 20,
    ENEMY_HIT = 21,
    ENEMY_DIE = 22,

    GAMETIMER = 23,
    ALARM = 24,
    DEACTIVATEAREA = 25,

    ADDKILL = 26,
    SPAWN_WINNERPOINT = 27,
    INWINNERPOINT = 28
}

