using System;

namespace SimpleServer
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