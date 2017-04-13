using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class ConverterTools
{
    public static System.Object ConvertBytesToOjbect(byte[] b)
    {
        MemoryStream memStream = new MemoryStream();
        BinaryFormatter binForm = new BinaryFormatter();
        memStream.Write(b, 0, b.Length);
        memStream.Seek(0, SeekOrigin.Begin);
        object obj = (object)binForm.Deserialize(memStream);
        return obj;
    }

    public static byte[] ConvertObjectToBytes(object o)
    {
        MemoryStream memStream = new MemoryStream();
        BinaryFormatter binForm = new BinaryFormatter();
        binForm.Serialize(memStream, o);
        return memStream.ToArray();
    }

    public static byte[] ConvertObjectToBytesForIOS(object o)
    {
        byte[] result;
        BinaryFormatter binForm = new BinaryFormatter();
        using (MemoryStream memStream = new MemoryStream())
        {
            binForm.Serialize(memStream, 0);
            result = memStream.GetBuffer();
        }
        return result;
    }

    public static byte[] WrapMessage(byte[] mes)
    {
        byte[] lengPre = BitConverter.GetBytes(mes.Length);
        byte[] r = new byte[lengPre.Length + mes.Length];
        lengPre.CopyTo(r, 0);
        mes.CopyTo(r, lengPre.Length);
        return r;
    }
}
