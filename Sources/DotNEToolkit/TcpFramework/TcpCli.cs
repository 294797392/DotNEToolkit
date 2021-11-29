///////////////////////////////////////////////////////
//NSTCPFramework
//版本：1.0.0.1
//////////////////////////////////////////////////////
using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Diagnostics;
using System.Collections;
using System.IO;
using System.Reflection;
using log4net;
using System.Collections.Generic;

namespace DotNEToolkit.TcpFramework
{
    /// <summary> 
    /// 提供Tcp网络连接服务的客户端类 
    /// 1.使用异步的Socket通讯函数BeginXXX作为基础，完成网络通讯功能。
    ///   接收与发送过程相互不干扰，实现了TCP全双工。
    /// 2.客户端连接服务成功后，将立刻启动接收过程。每次接收首先接收Message头，然后根据
    ///   报文头解析出来的剩余报文长度，安排一次或者多次的接收。由此来处理分包问题。当报文的长度
    ///   小于可以接受的缓冲区大小时，服务类将会等收到所有报文后触发数据接收事件；否则服务类将会
    /// </summary> 
    public class TcpCli
    {
        #region 常量字段

        /// <summary> 
        /// 缺省接收数据缓冲区大小8K 
        /// </summary> 
        public const int DefaultBufferSize = 8096;

        /// <summary>
        /// 连续接收到空消息的次数大于该值，服务器将会关闭该连接
        /// </summary>
        public const int DefaultMaxEmptyMessage = 5;

        #endregion

        #region 私有变量

        /// <summary> 
        /// 接收数据会话 
        /// </summary> 
        private Session session;

        /// <summary>
        /// receive buffer
        /// </summary>
        private byte[] receiveBuffer;

        /// <summary>
        /// 头部解析
        /// </summary>
        private IDataResolver headerResolver;

        /// <summary>
        /// 接收数据回调Callback
        /// </summary>
        private AsyncCallback dataReceivedCallback;

        /// <summary>
        /// data sent callback
        /// </summary>
        public AsyncCallback dataSentCallback;

        /// <summary>
        /// Logger
        /// </summary>
        private static ILog logger = LogManager.GetLogger(typeof(TcpCli));

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="headerSize">约定的TCP Message头大小</param>
        /// <param name="headerResolver">报文头解析函数</param>
        public TcpCli(IDataResolver headerResolver, int receiveBufferSize = DefaultBufferSize)
        {
            if (headerResolver == null)
            {
                throw new ArgumentNullException("headerResolver");
            }

            this.MaxEmptyMessage = DefaultMaxEmptyMessage;

            this.receiveBuffer = new byte[receiveBufferSize];
            this.headerResolver = headerResolver;

            this.dataReceivedCallback = new AsyncCallback(this.OnDataReceived);
            this.dataSentCallback = new AsyncCallback(this.OnDataSent);
        }

        #endregion

        #region 事件定义

        /// <summary> 
        /// 已连接事件 
        /// </summary> 
        public event EventHandler<NetEventArgs> Connected;

        /// <summary> 
        /// 连接断开事件 
        /// </summary> 
        public event EventHandler<NetEventArgs> Disconnected;

        /// <summary> 
        /// 接收到数据报文事件 
        /// </summary> 
        public event EventHandler<DataReceivedEventArgs> DataReceived;

        /// <summary>
        /// 发送结束
        /// </summary>
        public event EventHandler<DataSentEventArgs> DataSent;

        #endregion

        #region 属性

        /// <summary> 
        /// 返回客户端与服务器之间的连接状态 
        /// </summary> 
        public bool IsConnected
        {
            get;
            private set;
        }

        /// <summary>
        /// 如果某个clientSocket                                     
        /// 连续接收到空消息的次数大于该值，服务器将会关闭该连接
        /// </summary>
        public int MaxEmptyMessage
        {
            get;
            set;
        }

        #endregion

        #region 公有方法

        /// <summary> 
        /// 连接服务器 
        /// </summary> 
        /// <param name="ip">服务器IP地址</param> 
        /// <param name="port">服务器端口</param> 
        public bool BeginConnect(string host, int port)
        {
            if (this.IsConnected)
            {
                // 关闭后重新连接 
                this.Disconnect();
            }

            logger.InfoFormat("开始连接{0}:{1}", host, port);
            Socket newsock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            newsock.BeginConnect(host, port, new AsyncCallback(this.OnConnected), newsock);
            return true;
        }

        /// <summary> 
        /// 发送数据报文 
        /// </summary> 
        /// <param name="datagram"></param> 
        public SocketError SendAsync(byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                return SocketError.NoData;
            }

            if (!this.IsConnected)
            {
                return SocketError.NotConnected;
            }

            SocketError code;
            this.session.ClientSocket.BeginSend(data, 0, data.Length, SocketFlags.None, out code,
                this.dataSentCallback, this.session);

            return code;
        }

        /// <summary> 
        /// 关闭连接 
        /// </summary> 
        public virtual void Disconnect()
        {
            if (!this.IsConnected)
            {
                return;
            }

            this.session.Close();
            this.IsConnected = false;
        }

        #endregion

        #region 受保护方法

        /// <summary>
        /// 内部调用。会触发断开事件
        /// </summary>
        protected virtual void Close(SocketError code)
        {
            try
            {
                Socket clientSocket = this.session.ClientSocket;
                if (this.Disconnected != null)
                {
                    this.Disconnected(this, new NetEventArgs(clientSocket, code));
                }
            }
            catch
            {
            }
            finally
            {
                this.session.Close();
                this.IsConnected = false;
            }
        }

        /// <summary> 
        /// 数据发送完成处理函数 
        /// </summary> 
        /// <param name="iar"></param> 
        protected virtual void OnDataSent(IAsyncResult iar)
        {
            Session session = iar.AsyncState as Session;

            try
            {
                SocketError socketError;
                Socket socket = session.ClientSocket;
                if (socket == null)
                {
                    // 说明socket为空说明session已被服务器主动关闭, 不做任何处理
                    return;
                }

                int sent = socket.EndSend(iar, out socketError);
                if (this.DataSent != null)
                {
                    this.DataSent(this, new DataSentEventArgs(socket, sent, socketError));
                }

                // 处理SocketError
                switch (socketError)
                {
                    case SocketError.Success:
                        logger.DebugFormat("向{0}发送{1}字节完成", session.RemoteEndPoint, sent);
                        break;

                    case SocketError.ConnectionAborted:
                    case SocketError.ConnectionReset:
                    case SocketError.ConnectionRefused:
                    case SocketError.HostDown:
                    case SocketError.Disconnecting:
                    case SocketError.Fault:
                    case SocketError.NetworkDown:
                    case SocketError.NetworkReset:
                    case SocketError.NetworkUnreachable:
                    case SocketError.SocketError: // 未知错误
                        logger.InfoFormat("{0}发送错误, 关闭端口:{1}", session.RemoteEndPoint, socketError);
                        this.Close(socketError);
                        return;

                    default:
                        break;
                }
            }
            catch (SocketException ex)
            {
                logger.Warn("发送数据Socket异常", ex);
                this.Close(ex.SocketErrorCode);
            }
            catch (ObjectDisposedException)
            {
                // 收到该Exception说明Client Socket已被关闭。BeginReceive操作被取消，无需进行任何处理
            }
            catch (Exception ex)
            {
                logger.Warn("发送异常", ex);
                this.Close(SocketError.SocketError);
            }
        }

        /// <summary> 
        /// 建立Tcp连接后处理过程 
        /// </summary> 
        /// <param name="iar">异步Socket</param> 
        protected virtual void OnConnected(IAsyncResult iar)
        {
            Socket socket = (Socket)iar.AsyncState;

            try
            {
                socket.EndConnect(iar);

                //创建新会话 
                this.session = new Session(this.receiveBuffer, 0);
                this.session.SetSocket(socket);

                this.IsConnected = true;

                // 触发连接建立事件 
                if (this.Connected != null)
                {
                    this.Connected(this, new NetEventArgs(socket));
                }

                // 建立新会话后立即开始接收数据 
                logger.InfoFormat("启动{0}接收过程", this.session.RemoteEndPoint);
                this.ReceiveData(session, 0, this.headerResolver.HeaderSize, 0, 0);
            }
            catch (SocketException ex)
            {
                logger.WarnFormat("建立连接错误:{0}", ex.SocketErrorCode);

                if (socket != null)
                {
                    socket.Close();
                    if (this.Connected != null)
                    {
                        this.Connected(this, new NetEventArgs(socket, ex.SocketErrorCode));
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("连接建立出现异常", ex);

                // 关闭socket
                if (socket != null)
                {
                    socket.Close();

                    // 触发连接建立事件
                    if (this.Connected != null)
                    {
                        this.Connected(this, new NetEventArgs(socket, SocketError.SocketError));
                    }
                }
            }
        }

        /// <summary> 
        /// 数据接收处理函数 
        /// </summary> 
        /// <param name="iar">异步Socket</param> 
        protected virtual void OnDataReceived(IAsyncResult iar)
        {
            Session session = iar.AsyncState as Session;

            try
            {
                SocketError socketError;
                Socket socket = session.ClientSocket;
                if (socket == null)
                {
                    // 说明socket为空说明该次回调是因为session已被主动关闭后，
                    // 结束BeginReceive所产生的，因此不做任何处理。
                    return;
                }

                int receiveSize = socket.EndReceive(iar, out socketError);
                switch (socketError)
                {
                    case SocketError.Success:
                        if (receiveSize > 0)
                        {
                            session.EmptyMsgCount = 0; 
                            if (session.Remaining == 0)
                            {
                                this.ResolveHeader(session); // 处理报文头
                            }
                            else
                            {
                                this.HandleData(session, receiveSize);
                            }
                        }
                        else
                        {
                            // 接收到空数据
                            session.EmptyMsgCount++;
                            if (session.EmptyMsgCount > this.MaxEmptyMessage)
                            {
                                logger.InfoFormat("{0}空数据达到{1}次，关闭通道", session.RemoteEndPoint, session.EmptyMsgCount);
                                this.Close(SocketError.ConnectionReset);
                                return;
                            }
                            else 
                            {
                                // 重复上一次的接收Message任务
                                logger.InfoFormat("{0}收到空数据{1}次，重试上次接收", session.RemoteEndPoint, session.EmptyMsgCount);
                                this.ResumeLastReceive(session);
                            }
                        }

                        break;

                    case SocketError.ConnectionAborted:
                    case SocketError.ConnectionReset:
                    case SocketError.ConnectionRefused:
                    case SocketError.HostDown:
                    case SocketError.Disconnecting:
                    case SocketError.Fault:
                    case SocketError.NetworkDown:
                    case SocketError.NetworkReset:
                    case SocketError.NetworkUnreachable:
                    case SocketError.SocketError: // 未知错误
                        logger.DebugFormat("{0}接收错误, 关闭端口:{1}", session.RemoteEndPoint, socketError);
                        this.Close(socketError);
                        return;

                    default:
                        // 尝试恢复, 重试上次任务
                        this.ResumeLastReceive(session);
                        break;
                }
            }
            catch (SocketException ex)
            {
                logger.Warn("接收数据Socket异常", ex);
                this.Close(ex.SocketErrorCode);
            }
            catch (ObjectDisposedException)
            {
                // 收到该Exception说明Client Socket已被关闭。BeginReceive操作被取消，无需进行任何处理
            }
            catch (Exception ex)
            {
                // 出现异常，关闭socket
                logger.Warn("接收数据异常", ex);
                this.Close(SocketError.SocketError);
            }
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="session"></param>
        /// <param name="dataInBuffer">当前buffer已有的数据长度</param>
        /// <param name="expectedSize">期望接收的数据大小</param>
        /// <param name="packageIndex">下一次接收的包</param>
        /// <param name="remaining">当前会话剩余的字节数</param>
        private void ReceiveData(Session session, int dataInBuffer, int expectedSize, int packageIndex, int remaining)
        {
            // 在上一次ResolveHeader或者handle data时，可能会被外部程序在DataReceived事件中调用close。事件回调结束后还会执行ReceiveData
            // 而此时，ClientSocket可能会为空
            if (session.ClientSocket != null)
            {
                session.PackageIndex = packageIndex;
                session.DataInBuffer = dataInBuffer;
                session.Expected = expectedSize;
                session.Remaining = remaining;
                session.ClientSocket.BeginReceive(session.ReceiveBuffer, session.DataInBuffer,
                    session.Expected, SocketFlags.None, this.dataReceivedCallback, session);
            }
        }

        /// <summary>
        /// 根据当前记录重试上次的任务
        /// </summary>
        /// <param name="session"></param>
        private void ResumeLastReceive(Session session)
        {
            session.ClientSocket.BeginReceive(this.receiveBuffer, session.DataInBuffer, session.Expected, SocketFlags.None,
                this.dataReceivedCallback, session);
        }

        /// <summary>
        /// 解析报文头
        /// </summary>
        /// <param name="session"></param>
        private void ResolveHeader(Session session)
        {
            try
            {
                int messageSize = this.headerResolver.ResolveHeader(this.receiveBuffer, 0);
                if (messageSize < 0)
                {
                    // 该次报文头有问题, 采用断开客户端连接的方式处理策略
                    logger.WarnFormat("{0}无效报文头, 执行关闭通道策略", session.RemoteEndPoint);
                    this.Close(SocketError.ConnectionReset);
                }
                else if (messageSize == 0)
                {
                    // 本次报文数据为0，说明只包含报文头。那么本次数据接收已经全部完成。开始下一次报文头的接收
                    logger.DebugFormat("{0}接收报文完毕", session.RemoteEndPoint);

                    // 触发数据到达事件: 数据内容只包含报文头
                    if (this.DataReceived != null)
                    {
                        this.DataReceived(this, new DataReceivedEventArgs(session.ClientSocket, session.ReceiveBuffer,
                            session.BufferOffset, this.headerResolver.HeaderSize, 0, 0));
                    }

                    this.ReceiveData(session, 0, this.headerResolver.HeaderSize, 0, 0);
                }
                else
                {
                    logger.DebugFormat("{0}报文头, 报文大小:{1}", session.RemoteEndPoint, messageSize);

                    // 接收报文时, 如果 报文长度+HeaderSize大于缓冲区大小，则会分多次接收
                    // 处理报文头的时候并不触发报文事件，而是等到报文数据全部到达或者是每一次报文包到达后触发
                    this.ReceiveData(session, this.headerResolver.HeaderSize,
                        Math.Min(this.receiveBuffer.Length - this.headerResolver.HeaderSize, messageSize), 0, messageSize);
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// 处理收到的报文数据
        /// </summary>
        /// <param name="session"></param>
        /// <param name="receivedSize"></param>
        private void HandleData(Session session, int receivedSize)
        {
            Debug.Assert(receivedSize <= session.Expected); // recvSize不可能大于session.Expected
            try
            {
                session.Remaining -= receivedSize;
                if (receivedSize < session.Expected)
                {
                    // 接收到的数据比预期的少, 也许是发送方发送分包的问题。
                    // 此时继续等待发送方发送数据直到整个消息到达后再触发数据达到事件
                    logger.InfoFormat("{0}预期大小不一致{1}/{2}", session.RemoteEndPoint, session.Expected, receivedSize);
                    this.ReceiveData(session, session.DataInBuffer + receivedSize, session.Expected - receivedSize, session.PackageIndex, session.Remaining);
                }
                else if (receivedSize == session.Expected)
                {
                    int dataSize = session.DataInBuffer + receivedSize;
                    Debug.Assert(dataSize <= this.receiveBuffer.Length);

                    if (this.DataReceived != null)
                    {
                        // 触发数据到达事件
                        this.DataReceived(this, new DataReceivedEventArgs(session.ClientSocket, this.receiveBuffer, session.BufferOffset, dataSize, session.Remaining, session.PackageIndex));
                    }

                    if (session.Remaining > 0)
                    {
                        // 本次报文还有没收到的部分，继续接收
                        logger.DebugFormat("{0}剩余报文:{1}", session.RemoteEndPoint, session.Remaining);
                        this.ReceiveData(session, 0, Math.Min(session.Remaining, this.receiveBuffer.Length), session.PackageIndex + 1, session.Remaining);
                    }
                    else
                    {
                        // 本次报文已经在该次接收中全部完成。开始下一次报文头的接收
                        logger.DebugFormat("{0}接收报文完毕", session.RemoteEndPoint);
                        this.ReceiveData(session, 0, this.headerResolver.HeaderSize, 0, 0);
                    }
                }
            }
            catch
            {
            }
        }

        #endregion
    }
}
