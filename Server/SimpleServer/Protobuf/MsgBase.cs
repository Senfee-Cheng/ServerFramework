using ProtoBuf;
using ServerBase;
using SimpleServer.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MsgBase
{
    public virtual ProtocolEnum protoType { get; set; }   

    public static byte[] EncodeName(MsgBase msgBase)
    {
        return null;
    }

    /// <summary>
    /// 解码协议名
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static ProtocolEnum DecodeName(byte[] bytes)
    {
        return ProtocolEnum.None;
    }

    /// <summary>
    /// 协议序列化及加密
    /// </summary>
    /// <param name="msgBase"></param>
    /// <returns></returns>
    public static byte[] Encode(MsgBase msgBase)
    {
        using(var memory = new MemoryStream())
        {
            Serializer.Serialize(memory, msgBase);
            byte[] bytes = memory.ToArray();
            string secret = ServerSocket.Secretkey;
            //对数组进行加密
            if (msgBase is MsgSecret)
            {
                secret = ServerSocket.PublicKey;
            }
            bytes = AES.AESEncrypt(bytes, secret);
            return bytes;
        }
    }

    public static MsgBase Decode(ProtocolEnum protocol,byte[] bytes,int offset,int count)
    {
        if (count <=0)
        {
            Debug.LogError("协议解密出错，数据长度为0");
            return null;
        }

        try
        {
            byte[] newBytes = new byte[count];
            Array.Copy(bytes, offset, newBytes, 0, count);
            string secret = ServerSocket.Secretkey;
            if (protocol == ProtocolEnum.MsgSecret)
            {
                secret = ServerSocket.PublicKey;
            }
            newBytes = AES.AESDecrypt(newBytes, secret);

            using(var memory = new MemoryStream(newBytes, 0, newBytes.Length))
            {
                Type t = System.Type.GetType(protocol.ToString());
                return (MsgBase)Serializer.NonGeneric.Deserialize(t, memory);
            }

        }
        catch (Exception ex)
        {
            Debug.LogError("协议解密出错:" + ex);
            return null;
        }
    }
}
