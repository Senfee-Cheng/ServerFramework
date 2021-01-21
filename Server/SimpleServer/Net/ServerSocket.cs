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
        public static long m_PingInterval = 30;//心跳包间隔时间
        //端口号
        private const int m_Port = 8090;

        //服务器监听Socket
        private static Socket m_ListenSocket;

        //临时保存的 客户端Socket集合
        private static List<Socket> m_CheckReadList = new List<Socket>();

        //储存所有客户端
        public static Dictionary<Socket, ClientSocket> m_ClientDic = new Dictionary<Socket, ClientSocket>();

        public List<ClientSocket> m_TempList = new List<ClientSocket>();

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

                for (int i = m_CheckReadList.Count - 1; i >= 0; i--)
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


                //检测是否心跳包超时的计算

                long timeNow = GetTimeStamp();
                m_TempList.Clear();
                foreach (ClientSocket clientSocket in m_ClientDic.Values)
                {
                    //当前时间-上次心跳包间隔时间 > 2分钟，断开
                    if (timeNow - clientSocket.LastPingTime > m_PingInterval * 4)
                    {
                        Debug.Log("ping close" + clientSocket.Socket.RemoteEndPoint.ToString());
                        m_TempList.Add(clientSocket);//临时保存
                        
                    }
                }

                foreach (ClientSocket clientSocket in m_TempList)
                {
                    CloseClient(clientSocket);
                }
                m_TempList.Clear();
            }

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
            ByteArray readBuff = new ByteArray();
            //接收信息，根据信息解析协议，根据协议内容处理消息再下发到客户端
            int count = 0;

            //如果上一次接收数据刚好占满了1024的数组
            if (readBuff.Remain <=0)
            {
                //数据移动到index
                OnReceiveData(clientSocket);
                readBuff.CheckAndMoveBytes();
                //保证到如果数据长度大于默认长度，扩充数据长度，保证信息的正常接收
                while (readBuff.Remain <=0)
                {
                    int expandSize = readBuff.Length < ByteArray.DEFAULT_SIZE ? ByteArray.DEFAULT_SIZE : readBuff.Length;
                    readBuff.ReSize(expandSize * 2);
                }
            }

            try
            {
                count = client.Receive(readBuff.Bytes, readBuff.WriteIndex, readBuff.Remain, 0);
            }
            catch (SocketException ex)
            {
                Debug.LogError("Receive fail:" + ex);
                CloseClient(clientSocket);  //如果出错，关闭客户端
                return;

            }
            //客户端断开连接了
            if(count <= 0) {
                CloseClient(clientSocket);
                return;
            }
            //获取下次index位置
            readBuff.WriteIndex += count;
            //解析信息
            readBuff.CheckAndMoveBytes();
        }

        void OnReceiveData(ClientSocket clientSocket)
        {
            //如果信息长度不够，需要再次读取信息
            //OnReceiveData(clientSocket);


        }

        //获取时间
        public static long GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds);
        }
    }
}