using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace DotNEToolkit
{
    public static class Streams
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

            long left = size;
            int offset = 0;

            while (left > 0)
            {
                int readLen = left > 1024 ? BufferSize : (int)left;
                int actual = stream.Read(result, offset, readLen);
                offset += actual;
                left = result.Length - offset;
            }

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
    }
}
