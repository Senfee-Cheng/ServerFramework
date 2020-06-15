using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using ServerBase;

namespace SimpleServer
{
    public class ServerSocket : Singleton<ServerSocket>
    {
        //公钥
        public static string PublicKey = "Coffee";

        //密钥
        public static string Secretkey = "#Coffee_Sweet#";
#if DEBUG
        private string m_IPStr = "127.0.0.1";
#else
    private string m_IPStr = "172.45.756.54";
#endif
        //端口号
        private const int m_Port = 8090;
        //服务器监听Socket
        private static Socket m_ListenSocket;
        //客户端Socket集合
        private static List<Socket> m_CheckReadList = new List<Socket>( );

        public void Init()
        {
            IPAddress ip = IPAddress.Parse(m_IPStr);
            IPEndPoint ipEndPoint = new IPEndPoint(ip,m_Port);
            m_ListenSocket = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
            m_ListenSocket.Bind(ipEndPoint);
            m_ListenSocket.Listen(50000);
            Debug.LogInfo("服务器启动监听{0}成功",m_ListenSocket.LocalEndPoint.ToString());
        }
    }
}