using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DotNEToolkit
{
    /// <summary>
    /// 封装可以快速操作Socket的方法
    /// </summary>
    public static class SocketUtils
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger("SocketUtils");

        /// <summary>
        /// 读取一段完整的数据
        /// 注意当该函数返回false的时候，应该关闭Socket
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="data"></param>
        /// <param name="timeout">超时时间，如果超过此时间还没有收到完整的数据，那么就返回false</param>
        /// <returns></returns>
        public static bool ReceiveFull(this Socket socket, byte[] data, int timeout)
        {
            // 收数据的耗时
            int elapsed = 0;

            // 剩余要收的数据长度
            int left = data.Length;

            // 已经接收到的数据长度
            int read = 0;

            while (left > 0)
            {
                SocketError error;
                int size = socket.Receive(data, read, left, SocketFlags.None, out error);
                switch (error)
                {
                    case SocketError.Success:
                        {
                            if (size == 0)
                            {
                                if (elapsed >= timeout)
                                {
                                    return false;
                                }
                                else
                                {
                                    elapsed += 50;
                                    Thread.Sleep(50);
                                }
                            }
                            else
                            {
                                read += size;
                                left -= size;
                            }
                            break;
                        }

                    default:
                        {
                            logger.ErrorFormat("接收数据失败, SocketError = {0}", error);
                            return false;
                        }
                }
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
