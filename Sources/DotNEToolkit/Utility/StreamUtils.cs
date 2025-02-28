using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace DotNEToolkit
{
    /// <summary>
    /// 封装对流的一些操作
    /// </summary>
    public static class StreamUtils
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger("StreamUtils");

        /// <summary>
        /// 从流中读取数据，一直读满size个字节为止
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="bytes">存储读取道的数据的缓冲区</param>
        /// <param name="offset">要读取的数据偏移量</param>
        /// <param name="size">要读取的字节数，如果为0，那么读取bytes.Length个字节</param>
        /// <returns>如果读取到的数据是0，那么返回false</returns>
        public static bool ReadFull(this Stream stream, byte[] bytes, int offset, int size)
        {
            // 剩余要收的数据长度
            int left = size == 0 ? bytes.Length : size;

            // 已经接收到的数据长度
            int read = 0;

            while (left > 0)
            {
                int readed = stream.Read(bytes, read + offset, left);
                read += readed;
                left -= readed;

                if (readed == 0) 
                {
                    return false;
                }
            }

            return true;
        }

        public static byte[] ReadFull(this Stream stream, int size)
        {
            byte[] bytes = new byte[size];
            if (!ReadFull(stream, bytes, 0, size))
            {
                return null;
            }
            return bytes;
        }

        public static void ReadFull(this StreamReader reader, byte[] bytes)
        {
            ReadFull(reader.BaseStream, bytes, 0, bytes.Length);
        }
    }
}
