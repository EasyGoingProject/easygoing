using System;
using System.IO;
using System.Runtime.InteropServices;
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

    public static byte[] StructToBytes(object o)
    {
        int datasize = Marshal.SizeOf(o);
        IntPtr buff = Marshal.AllocHGlobal(datasize);
        Marshal.StructureToPtr(o, buff, false);
        byte[] data = new byte[datasize];
        Marshal.Copy(buff, data, 0, datasize);
        Marshal.FreeHGlobal(buff);
        return data;
    }

    public static object BytesToStruct(byte[] data, Type type)
    {
        IntPtr buff = Marshal.AllocHGlobal(data.Length);
        Marshal.Copy(data, 0, buff, data.Length);
        object obj = Marshal.PtrToStructure(buff, type);
        Marshal.FreeHGlobal(buff);

        if (Marshal.SizeOf(obj) != data.Length)
            return null;
        return obj;
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
