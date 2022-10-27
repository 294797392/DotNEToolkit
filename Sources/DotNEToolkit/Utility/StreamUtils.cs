using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace DotNEToolkit
{
    /// <summary>
    /// 封装对流的一些操作
    /// </summary>
    public static class StreamUtils
    {
        /// <summary>
        /// 一次读取1024个字节
        /// </summary>
        private const int BufferSize = 1024;

        /// <summary>
        /// 从流中读取数据，一直读满size个字节为止
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static byte[] ReadFull(this Stream stream, long size)
        {
            byte[] result = new byte[size];
            stream.ReadFull(result);
            return result;
        }

        public static int ReadFull(this Stream stream, byte[] bytes)
        {
            long left = bytes.Length;
            int readed = 0;

            while (left > 0)
            {
                int readLen = left > BufferSize ? BufferSize : (int)left;
                int size = stream.Read(bytes, readed, readLen);
                if (size == 0)
                {
                    return readed;
                }
                readed += size;
                left -= readed;
            }

            return readed;
        }

        /// <summary>
        /// 读取指定大小的数据
        /// </summary>
        /// <param name="readFunc"></param>
        /// <param name="howMuch">要读取的字节数</param>
        /// <returns></returns>
        public static byte[] ReadFull(Func<byte[], int, int, int> readFunc, int howMuch)
        {
            byte[] result = new byte[howMuch];

            int left = howMuch;
            int readed = 0;

            while (left > 0)
            {
                int readLen = left > BufferSize ? BufferSize : (int)left;
                int size = readFunc(result, readed, readLen);
                if (size == 0)
                {
                    break;
                }
                readed += size;
                left -= readed;
            }

            return result;
        }
    }
}
