using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace DotNEToolkit.Extentions
{
    public static class SocketExtentions
    {
        /// <summary>
        /// 读取一段完整的数据
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool ReceiveFull(this Socket socket, byte[] data)
        {
            int left = data.Length;
            int read = 0;

            while (left > 0)
            {
                int size = socket.Receive(data, read, left, SocketFlags.None);
                read += size;
                left -= size;
            }

            return true;
        }

        /// <summary>
        /// 发送一段完整的数据
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool SendFull(this Socket socket, byte[] data)
        {
            int left = data.Length;     // 剩余要发送的字节数
            int sent = 0;               // 已经发送的字节数

            while (left > 0)
            {
                int size = socket.Send(data, sent, left, SocketFlags.None);
                sent += size;
                left -= size;
            }

            return true;
        }
    }
}
