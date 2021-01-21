using ProtoBuf;
using System.Net.Sockets;

[ProtoContract]
public class MsgSecret:MsgBase
{
    /// <summary>
    /// 每一个协议类必然包含构造函数来确定当前协议类型，并且都有prototype进行序列化标记
    /// </summary>
    public MsgSecret()
    {
        protoType = ProtocolEnum.MsgSecret;
    }

    [ProtoMember(1)]
    public override ProtocolEnum protoType { get; set; }

    //密钥
    [ProtoMember(2)]
    public string secret;
}
 