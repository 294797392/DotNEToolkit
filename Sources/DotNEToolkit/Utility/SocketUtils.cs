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
        /// <param name="timeout">超时时间，如果读取数据的时候超过此时间还没读到，那么就返回false</param>
        /// <returns></returns>
        public static bool ReceiveFull(this Socket socket, byte[] data, int timeout)
        {
            int timeout_ms = timeout * 1000;

            // 剩余要收的数据长度
            int left = data.Length;

            // 已经接收到的数据长度
            int read = 0;

            // 接收到空数据的次数
            int empties = 0;

            while (left > 0)
            {
                // 以下几种情况SocketRead会返回true：
                // 1. 有连接接入的时候
                // 2. 有数据可以被读取的时候
                // 3. 连接被关闭，重置，中止的时候（此时读取到的数据可能是0）
                bool result = socket.Poll(timeout_ms, SelectMode.SelectRead);
                if (!result)
                {
                    logger.ErrorFormat("从Socket读取数据超时, 超时时间:{0}ms", timeout);
                    return false;
                }

                SocketError error;
                int size = socket.Receive(data, read, left, SocketFlags.None, out error);
                switch (error)
                {
                    case SocketError.Success:
                        {
                            if (size == 0)
                            {
                                // 按理说应该不会是0
                                // 如果还是0，有可能是Socket已经断开连接了
                                if (++empties == 5)
                                {
                                    logger.ErrorFormat("连续收到5次空数据，判断连接已断开");
                                    return false;
                                }
                                continue;
                            }
                            else
                            {
                                empties = 0;
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
        public static bool SendFull(this Socket socket, byte[] data, int offset, int timeout)
        {
            // 收数据的耗时
            int elapsed = 0;

            // 剩余要发送的字节数
            int left = data.Length;

            // 已经发送的字节数
            int sent = 0;

            while (left > 0)
            {
                SocketError error;
                int size = socket.Send(data, sent + offset, left, SocketFlags.None, out error);
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
                                    logger.ErrorFormat("写超时, 等待50毫秒");
                                    elapsed += 50;
                                    Thread.Sleep(50);
                                }
                            }
                            else
                            {
                                sent += size;
                                left -= size;
                            }
                            break;
                        }

                    default:
                        {
                            logger.ErrorFormat("写入数据失败, SocketError = {0}", error);
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
        public static bool SendFull(this Socket socket, byte[] data, int timeout)
        {
            return SendFull(socket, data, 0, timeout);

            //// 收数据的耗时
            //int elapsed = 0;

            //int left = data.Length;     // 剩余要发送的字节数
            //int sent = 0;               // 已经发送的字节数

            //while (left > 0)
            //{
            //    SocketError error;
            //    int size = socket.Send(data, sent, left, SocketFlags.None, out error);
            //    switch (error)
            //    {
            //        case SocketError.Success:
            //            {
            //                if (size == 0)
            //                {
            //                    if (elapsed >= timeout)
            //                    {
            //                        return false;
            //                    }
            //                    else
            //                    {
            //                        logger.ErrorFormat("写超时, 等待50毫秒");
            //                        elapsed += 50;
            //                        Thread.Sleep(50);
            //                    }
            //                }
            //                else
            //                {
            //                    sent += size;
            //                    left -= size;
            //                }
            //                break;
            //            }

            //        default:
            //            {
            //                logger.ErrorFormat("写入数据失败, SocketError = {0}", error);
            //                return false;
            //            }
            //    }
            //}

            //return true;
        }
    }
}
