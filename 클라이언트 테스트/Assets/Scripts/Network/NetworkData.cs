using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
public class NetworkData
{
    private int senderId;
    private DataType dataType;
    private NetworkVector position;
    private NetworkQuaternion rotation;

    public NetworkData(int _senderID, DataType _dataType)
    {
        senderId = _senderID;
        dataType = _dataType;
    }

    public NetworkData(int _senderID, DataType _dataType, NetworkVector _position, NetworkQuaternion _rotation)
    {
        senderId = _senderID;
        dataType = _dataType;
        position = _position;
        rotation = _rotation;
    }

    public int SenderId { get { return senderId; } }
    public DataType DataType { get { return dataType; } }
    public NetworkVector Position { get { return position; } }
    public NetworkQuaternion Rotation { get { return Rotation; } }

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
}

public enum DataType
{
    RESPONSE = 0,
    JOIN = 1,
    SYNCTRANSFORM = 2,
}

