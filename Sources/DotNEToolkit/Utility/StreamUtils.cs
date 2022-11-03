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
        /// 从流中读取数据，一直读满size个字节为止
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static void ReadFull(this Stream stream, byte[] bytes)
        {
            // 剩余要收的数据长度
            int left = bytes.Length;

            // 已经接收到的数据长度
            int read = 0;

            while (left > 0)
            {
                int size = stream.Read(bytes, read, left);
                read += size;
                left -= size;
            }
        }

        public static byte[] ReadFull(this Stream stream, long size)
        {
            byte[] bytes = new byte[size];
            ReadFull(stream, bytes);
            return bytes;
        }

        public static void ReadFull(this StreamReader reader, byte[] bytes)
        {
            ReadFull(reader.BaseStream, bytes);
        }
    }
}
