using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
public struct NetworkData
{
    public int senderId;
    public CharacterType characterType;
    public DataType dataType;
    public string message;
    public float life;
    public NetworkVector position;
    public NetworkQuaternion rotation;
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

public enum DataType
{
    RESPONSE = 0,
    JOIN = 1,
    SYNCTRANSFORM = 2,
    MESSAGE = 3
}

