using System;

namespace SimpleServer.Net
{
    public class Program
    {
        static void Main(string[] args)
        {
            ServerSocket.Instance.Init();
            Console.ReadLine();
        }
    }
}