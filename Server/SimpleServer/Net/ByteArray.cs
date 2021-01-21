using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleServer.Net
{
    public class ByteArray
    {
        //默认大小
        public const int DEFAULT_SIZE = 1024;

        //初始大小
        private int m_InitSize = 0;

        //缓冲区
        public byte[] Bytes;

        //读写位置
        public int ReadIndex = 0;
        public int WriteIndex = 0;

        //容量
        private int Capacity = 0;

        //剩余空间
        public int Remain { get { return Capacity - WriteIndex; } }

        //数据长度
        public int Length { get { return WriteIndex - ReadIndex; } }

        //构造器
        public ByteArray()
        {
            Bytes = new byte[DEFAULT_SIZE];
            Capacity = DEFAULT_SIZE;
            m_InitSize = DEFAULT_SIZE;
            ReadIndex = 0;
            WriteIndex = 0;
        }
        //检查并移动数据
        public void CheckAndMoveBytes()
        {
            if(Length < 8)
            {
                MoveBytes();
            }
        }

        /// <summary>
        /// 移动数据
        /// </summary>
        public void MoveBytes()
        {
            if(ReadIndex < 0)
            {
                return;
            }
            Array.Copy(Bytes, ReadIndex, Bytes, 0, Length);
            WriteIndex = Length;
            ReadIndex = 0;
        }

        /// <summary>
        /// 重设尺寸
        /// </summary>
        /// <param name="size"></param>
        public void ReSize(int size)
        {
            if (ReadIndex < 0) return;          //
            if (size < Length) return;          // <数据长度
            if (size < m_InitSize) return;      // <初始尺寸

            int n = 1024;
            if (n < size) n *= 2;
            while (n < size) n *= 2;
            byte[] newBytes = new byte[Capacity];
            Array.Copy(Bytes, ReadIndex, newBytes, 0, Length);  //扩大后拷贝
            Bytes = newBytes;
            WriteIndex = Length;
            ReadIndex = 0;
            
        }

    }
}
