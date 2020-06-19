using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using ServerBase;

namespace SimpleServer.Net
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

        //临时保存的 客户端Socket集合
        private static List<Socket> m_CheckReadList = new List<Socket>();

        //储存所有客户端
        public static Dictionary<Socket, ClientSocket> m_ClientDic = new Dictionary<Socket, ClientSocket>();

        public void Init()
        {
            IPAddress ip = IPAddress.Parse(m_IPStr);
            IPEndPoint ipEndPoint = new IPEndPoint(ip, m_Port);
            m_ListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_ListenSocket.Bind(ipEndPoint);
            m_ListenSocket.Listen(50000);
            Debug.LogInfo("服务器启动监听{0}成功", m_ListenSocket.LocalEndPoint.ToString());

            while (true)
            {
                //检查是否有读取的Socket

                //处理查找所有Socket
                ResetCheckRead();

                try
                {
                    //最后等待时间单位是微秒
                    Socket.Select(m_CheckReadList, null, null, 1000);
                }
                catch (Exception e)
                {

                    Debug.LogError(e);
                }

                for (int i = m_CheckReadList.Count-1; i >=0 ; i--)
                {
                    Socket s = m_CheckReadList[i];
                    if (s == m_ListenSocket)
                    {
                        //说明有客户端连接到服务器，服务器Socket可读
                        ReadListen(s);
                    }
                    else
                    {
                        //说明连接的客户端可读，证明有信息传上来了
                        ReadClient(s);
                    }
                }
            }

            //检测是否心跳包超时的计算
        }

        public void ResetCheckRead()
        {
            m_CheckReadList.Clear();
            m_CheckReadList.Add(m_ListenSocket);
            foreach (Socket socket in m_ClientDic.Keys)
            {
                m_CheckReadList.Add(socket);
            }
        }

        void ReadListen(Socket listen)
        {
            try
            {
                Socket client = listen.Accept();
                ClientSocket clientSocket = new ClientSocket();
                clientSocket.Socket = client;
                clientSocket.LastPingTime = GetTimeStamp();
                m_ClientDic.Add(client, clientSocket);
                Debug.Log("一个客户端连接：{0}，当前{1}个客户端在线", client.LocalEndPoint.ToString(), m_ClientDic.Count);
            }
            catch (SocketException e)
            {

                Debug.LogError("Accept fail" + e);
            }
        }

        //关闭客户端
        public void CloseClient(ClientSocket client)
        {
            client.Socket.Close();
            m_ClientDic.Remove(client.Socket);
            Debug.Log("一个客户端断开连接 ，当前总连接数：{0}", m_ClientDic.Count);
        }

        //拿到消息，进行接收，处理消息后发回客户端
        void ReadClient(Socket client)
        {
            ClientSocket clientSocket = m_ClientDic[client];
            //接收信息，根据信息解析协议，根据协议内容处理消息再下发到客户端
        }

        //获取时间
        public static long GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds);
        }
    }
}