using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class NetworkData
{
    [Serializable]
    public struct MessageVector
    {
        int sender;
        float x;
        float y;
        float z;
    }

    private Byte[] PacketSerialize(MessageVector packet)
    {
        MemoryStream ms = new MemoryStream();
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(ms, packet);
        return ms.ToArray();
    }

    private MemoryStream PacketDeserialize(Byte[] data)
    {
        MemoryStream ms = new MemoryStream();
        foreach (Byte wb in data)
            ms.WriteByte(wb);
        ms.Position = 0;
        return ms;
    }

    [Serializable]
    public struct MessageQuaternion
    {
        int sender;
        float x;
        float y;
        float z;
        float w;
    }
}