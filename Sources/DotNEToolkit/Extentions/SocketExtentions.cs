using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace DotNEToolkit.Extentions
{
    public static class SocketExtentions
    {
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
