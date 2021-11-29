///////////////////////////////////////////////////////
//TCP Framework
//////////////////////////////////////////////////////
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using log4net;

namespace DotNEToolkit.TcpFramework
{
    /// <summary> 
    /// 提供TCP连接服务的服务器类。 
    /// 
    /// 1. 使用异步的Socket事件作为基础，完成全双工的网络通讯功能. 
    /// 2. 该服务类每次Accept客户端连接后，将立刻启动接收过程。每次接收首先接收Message头，然后根据
    ///    报文头解析出来的剩余报文长度，安排一次或者多次的接收。由此来处理分包问题。当报文的长度
    ///    小于可以接受的缓冲区大小时，服务类将会等收到所有报文后触发数据接收事件；否则服务类将会
    ///    触发多次数据接收事件，直至报文接收完成。然后进入下一次接收状态。
    /// 3. 服务类发送时也是用异步发送过程。与发送过程相互不干扰。
    /// </summary> 
    public class TcpSvr
    {
        #region 常量字段

        /// <summary>
        /// 默认并发级别，即同一时刻访问sessionTable的并发线程数。
        /// 请参考ConcurrentDictionary concurrentLevel定义。
        /// </summary>
        public const int DefaultConcurrentLevel = 30;

        /// <summary> 
        /// 默认的服务器最大连接客户端端数据 
        /// </summary> 
        public const int DefaultMaxClient = 3000;

        /// <summary> 
        /// 缺省Header大小8K 
        /// </summary> 
        public const int DefaultHeaderSize = 256;

        /// <summary>
        /// TCP Listen backlog
        /// </summary>
        public const int DefaultBacklog = 512;

        /// <summary>
        /// 接收缓冲最大大小
        /// </summary>
        public const int DefaultBufferSize = 8192;

        /// <summary>
        /// 连续接收到空消息的次数大于该值，服务器将会关闭该连接
        /// </summary>
        public const int DefaultMaxEmptyMessage = 5;

        #endregion

        #region 私有变量

        /// <summary>
        /// 并发级别。定义为同一时刻访问sessionTable的并发线程数。
        /// 请参考ConcurrentDictionary concurrentLevel定义。
        /// </summary>
        private int concurrentLevel;

        /// <summary>
        /// 接收数据回调Callback
        /// </summary>
        private AsyncCallback dataReceivedCallback;

        /// <summary>
        /// connection Accepted callback
        /// </summary>
        public AsyncCallback connectionAcceptedCallback;

        /// <summary>
        /// data sent callback
        /// </summary>
        public AsyncCallback dataSentCallback;

        private int receiveBufferSize;

        /// <summary>
        /// 服务器程序监听的IP地址
        /// </summary>
        private IPAddress serverIP;

        /// <summary> 
        /// 服务器程序使用的端口 
        /// </summary> 
        private ushort port;

        /// <summary> 
        /// 服务器的运行状态 
        /// </summary> 
        private bool isRun;

        /// <summary> 
        /// 服务器使用的异步Socket类, 
        /// </summary> 
        private Socket listenSocket;

        /// <summary> 
        /// 保存所有客户端会话的哈希表 
        /// </summary> 
        private ConcurrentDictionary<int, Session> sessionTable;

        /// <summary>
        /// Session Manager
        /// </summary>
        private SessionManager sessionManager;

        /// <summary>
        /// TCP数据解析器
        /// </summary>
        private IDataResolver headerResolver;

#if AsyncAccept
        private ManualResetEvent acceptDone;
        private Thread listeningThread;
#endif

        /// <summary>
        /// Logger
        /// </summary>
        private static ILog logger = LogManager.GetLogger(typeof(TcpSvr));

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ipAddress">服务IP</param>
        /// <param name="port">服务器端监听的端口号</param>
        /// <param name="headerResolver">报文头解析函数</param>
        /// <param name="maxClient">服务器最大容纳客户端数</param>
        /// <param name="headerSize">报文头长度</param>
        /// <param name="maxRecvBufSize">每次接收报文的最大大小</param>
        /// <param name="concurrentLevel">服务器并发度。主要体现在Listener的backlog以及sessionMap的并发级别</param>
        public TcpSvr(IPAddress ipAddress, ushort port, IDataResolver headerResolver,
            int maxClient = DefaultMaxClient, 
            int maxRecvBufSize = DefaultBufferSize,
            int concurrentLevel = DefaultConcurrentLevel)
        {
            this.Initialize(ipAddress, port, headerResolver, maxClient, maxRecvBufSize, concurrentLevel);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ipAddress">服务IP</param>
        /// <param name="port">服务器端监听的端口号</param>
        /// <param name="headerResolver">报文头解析函数</param>
        /// <param name="maxClient">服务器最大容纳客户端数</param>
        /// <param name="headerSize">报文头长度</param>
        /// <param name="maxRecvBufSize">每次接收报文的最大大小</param>
        /// <param name="concurrentLevel">服务器并发度。主要体现在Listener的backlog以及sessionMap的并发级别</param>
        public TcpSvr(string ipAddress, ushort port, IDataResolver headerResolver,
            int maxClient = DefaultMaxClient,
            int maxRecvBufSize = DefaultBufferSize,
            int concurrentLevel = DefaultConcurrentLevel)
        {
            IPAddress ip;
            if (!IPAddress.TryParse(ipAddress, out ip))
            {
                throw new ArgumentException("invalid ip address", "ipAddress");
            }

            this.Initialize(ip, port, headerResolver, maxClient, maxRecvBufSize, concurrentLevel);
        }

        #endregion

        #region 事件定义

        /// <summary> 
        /// 客户端建立连接事件 
        /// </summary> 
        public event EventHandler<NetEventArgs> ClientConnected;

        /// <summary> 
        /// 客户端关闭事件 
        /// </summary> 
        public event EventHandler<NetEventArgs> ClientClosed;

        /// <summary> 
        /// 服务器已经满事件 
        /// </summary> 
        public event EventHandler<NetEventArgs> ServerFull;

        /// <summary> 
        /// 服务器接收到数据事件 
        /// </summary> 
        public event EventHandler<DataReceivedEventArgs> DataReceived;

        /// <summary>
        /// 数据发送结束
        /// </summary>
        public event EventHandler<DataSentEventArgs> DataSent;

        #endregion

        #region 属性

        /// <summary> 
        /// 服务器的Socket对象 
        /// </summary> 
        public Socket ServerSocket
        {
            get
            {
                return this.listenSocket;
            }
        }

        /// <summary> 
        /// 服务器可以容纳客户端的最大能力 
        /// </summary> 
        public int Capacity
        {
            get
            {
                return this.sessionManager.Capacity;
            }
        }

        /// <summary> 
        /// 当前的客户端连接数 
        /// </summary> 
        public int SessionCount
        {
            get
            {
                return this.sessionTable.Count;
            }
        }

        /// <summary> 
        /// 服务器运行状态 
        /// </summary> 
        public bool IsRun
        {
            get
            {
                return this.isRun;
            }
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
        /// 启动服务器程序,开始监听客户端请求 
        /// </summary> 
        public void Start()
        {
            if (this.isRun)
            {
                logger.Warn("TcpSvr已经在运行.");
                return;
            }

            this.sessionTable = new ConcurrentDictionary<int, Session>(this.concurrentLevel, this.sessionManager.Capacity);

            //绑定端口 
            IPEndPoint iep = new IPEndPoint(this.serverIP, this.port);

            //初始化socket 
            this.listenSocket = new Socket(iep.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            this.listenSocket.Bind(iep);

            //开始监听 
            logger.InfoFormat("开始侦听 {0}:{1}", this.serverIP, this.port);
            this.listenSocket.Listen(this.concurrentLevel);

            this.listenSocket.BeginAccept(this.connectionAcceptedCallback, this.listenSocket);

#if AsyncAccept
            this.acceptDone = new ManualResetEvent(false);
            this.listeningThread = new Thread(() =>
            {
                while (this.isRun)
                {
                    try
                    {
                        this.acceptDone.Reset();
                        logger.Debug("Accept Next");
                        this.listenSocket.BeginAccept(new AsyncCallback(this.OnConnectionAccepted), this.listenSocket);

                        // 等待信号准备继续接受客户端 
                        this.acceptDone.WaitOne();
                    }
                    catch (Exception ex)
                    {
                        logger.Error("listen异常", ex);
                    }
                }
            });
            this.listeningThread.IsBackground = true;
            this.listeningThread.Start();
#endif            
            this.isRun = true;
        }

        /// <summary> 
        /// 停止服务器程序,所有与客户端的连接将关闭 
        /// </summary> 
        public void Stop()
        {
            if (this.isRun)
            {
                this.isRun = false;

#if AsyncAccept
                try
                {
                    this.listeningThread.Abort(); // 停止侦听线程
                }
                catch (Exception ex)
                {
                    logger.Warn("停止侦听异常", ex);
                }
#endif
                //关闭侦听端口
                if (this.listenSocket.Connected)
                {
                    this.listenSocket.Shutdown(SocketShutdown.Both);
                }

                // 首先清除事件，防止出现大量的client disconnected事件
                this.ClientClosed = null;
                this.ClientConnected = null;
                this.DataReceived = null;
                this.DataSent = null;
                this.ServerFull = null;

                this.CloseAllClient();

                //清理资源 
                this.listenSocket.Close();
                this.sessionTable = null;

                this.sessionManager.Clear();
            }
        }

        /// <summary> 
        /// 关闭所有的客户端会话,与所有的客户端连接会断开。
        /// 该命令将不触发ClientClosed事件
        /// </summary> 
        public void CloseAllClient()
        {
            try
            {
                logger.Info("关闭所有session");
                List<Session> sessionList = new List<Session>(this.sessionTable.Values);
                this.sessionTable.Clear();

                foreach (Session session in sessionList)
                {
                    this.sessionManager.ReleaseSession(session);
                }
            }
            catch (Exception ex)
            {
                logger.Error("清除所有session异常", ex);
            }
        }

        /// <summary> 
        /// 关闭一个与客户端之间的会话 
        /// </summary> 
        /// <param name="clientSocket">需要关闭的客户端会话对象</param> 
        public void CloseClient(int clientHandle)
        {
            try
            {
                Session session;
                if (this.sessionTable.TryRemove(clientHandle, out session))
                {
                    logger.DebugFormat("关闭客户端{0}", session.RemoteEndPoint);
                    Socket clientSocket = session.ClientSocket;

                    //发送客户端关闭事件
                    if (this.ClientClosed != null)
                    {
                        this.ClientClosed(this, new NetEventArgs(clientSocket, SocketError.Disconnecting));
                    }

                    this.sessionManager.ReleaseSession(session);
                }
                else
                {
                    logger.WarnFormat("没有找到需删除的Handle: {0}", clientHandle);
                }
            }
            catch (Exception ex)
            {
                logger.Warn("关闭异常", ex);
            }
        }

        /// <summary> 
        /// 发送数据 
        /// </summary> 
        /// <param name="clientSession">接收数据的客户端会话</param> 
        /// <param name="datagram">数据报文</param> 
        public SocketError SendAsync(int clientHandle, byte[] data)
        {
            Session session = this.FindSession(clientHandle);
            if (session != null)
            {
                SocketError code;
                logger.InfoFormat("开始向{0}发送", session.RemoteEndPoint);
                session.ClientSocket.BeginSend(data, 0, data.Length, SocketFlags.None, out code,
                    this.dataSentCallback, session);
                return code;
            }
            else
            {
                // 给定Client不存在
                return SocketError.NotConnected;
            }
        }

        /// <summary>
        /// 获取Client Socket
        /// </summary>
        /// <param name="clientHandle"></param>
        /// <returns></returns>
        public Socket GetClientSocket(int clientHandle)
        {
            Session session = this.FindSession(clientHandle);
            return session != null ? session.ClientSocket : null;
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ipAddress">服务IP</param>
        /// <param name="port">服务器端监听的端口号</param>
        /// <param name="headerResolver">报文头解析函数</param>
        /// <param name="maxClient">服务器最大容纳客户端数</param>
        /// <param name="recvBufSize">接收缓冲的大小</param>
        /// <param name="concurrentLevel">服务器并发度。主要体现在Listener的backlog以及sessionMap的并发级别</param>
        private void Initialize(IPAddress serverIP, ushort port, IDataResolver headerResolver, 
            int maxClient, int recvBufSize, int concurrentLevel)
        {
            if (headerResolver == null)
            {
                throw new ArgumentNullException("headerResolver");
            }

            if (port == 0)
            {
                throw new ArgumentNullException("port");
            }

            this.headerResolver = headerResolver;
            this.serverIP = serverIP;
            this.port = port;
            this.receiveBufferSize = recvBufSize;
            this.concurrentLevel = concurrentLevel;

            this.sessionManager = new SessionManager(maxClient, recvBufSize);
            this.MaxEmptyMessage = DefaultMaxEmptyMessage;

            this.dataReceivedCallback = new AsyncCallback(this.OnDataReceived);
            this.connectionAcceptedCallback = new AsyncCallback(this.OnConnectionAccepted);
            this.dataSentCallback = new AsyncCallback(this.OnDataSent);
        }

        /// <summary> 
        /// 客户端连接处理函数 
        /// </summary> 
        /// <param name="iar">欲建立服务器连接的Socket对象</param> 
        private void OnConnectionAccepted(IAsyncResult iar)
        {
            if (!this.isRun)
            {
                return;
            }

            #region 接受一个客户端的连接请求 

            Socket client = null;
            try
            {
                logger.InfoFormat("接收接入请求");
                Socket listener = (Socket)iar.AsyncState;
                client = listener.EndAccept(iar);
            }
            catch (ObjectDisposedException)
            {
                // 收到该Exception说明Client Socket已被关闭。BeginReceive操作被取消，无需进行任何处理
                return;
            }
            catch (Exception ex)
            {
                // 接收客户端接入请求异常
                logger.Warn("Accept异常", ex);
                return;
            }
            finally
            {
                // 通知accept线程开始进行下一次accept
#if AsyncAccept
                this.acceptDone.Set();
#else
                this.listenSocket.BeginAccept(this.connectionAcceptedCallback, this.listenSocket);
#endif
            }

            #endregion

            #region 处理连接请求

            if (this.SessionCount >= this.sessionManager.Capacity)
            {
                logger.InfoFormat("客户数已到达最大值{0}, 关闭连接:{1}", this.sessionManager.Capacity, client.RemoteEndPoint);
                client.Close();

                if (this.ServerFull != null)
                {
                    this.ServerFull(this, new NetEventArgs(client));
                }
            }
            else
            {
                //新建一个session
                Session session = this.sessionManager.RequestSession(client);
                Debug.Assert(session != null); // 最大客户数和sessionManager容量匹配，因此session应该不为空

                if (session != null)
                {
                    if (this.sessionTable.TryAdd(session.ID, session))
                    {
                        // 发出新的客户连接事件 
                        if (this.ClientConnected != null)
                        {
                            this.ClientConnected(this, new NetEventArgs(session.ClientSocket));
                        }

                        logger.InfoFormat("启动{0}接收过程, 开始接收header", session.RemoteEndPoint);
                        this.ReceiveData(session, 0, this.headerResolver.HeaderSize, 0, 0);
                    }
                    else
                    {
                        logger.WarnFormat("添加{0}失败, 关闭连接", session.RemoteEndPoint);
                        this.sessionManager.ReleaseSession(session);
                    }
                }
            }

            #endregion
        }

        /// <summary> 
        /// 接受数据完成处理函数. 接收到报文后，server首先判断session当前的状态：
        /// 1. 接收报文头的状态（Session.Remaining==0）: 
        ///    服务将调用headerResolver解析报文头部，确定接下来的报文大小。同时创建接收缓冲。
        /// 2. 接收报文状态(Session.Remaining>0)：
        ///    收到报文。服务将触发接收报文事件
        /// 
        /// </summary> 
        /// <param name="iar">目标客户端Socket</param> 
        private void OnDataReceived(IAsyncResult iar)
        {
            Session session = iar.AsyncState as Session;

            try
            {
                SocketError socketError;
                Socket clientSocket = session.ClientSocket;
                if (clientSocket == null)
                {
                    // 说明socket为空说明该次回调是因为session已被服务器主动关闭后，
                    // 结束BeginReceive所产生的，因此不做任何处理。
                    return;
                }

                int recvSize = clientSocket.EndReceive(iar, out socketError);
                switch (socketError)
                {
                    case SocketError.Success:      
                        if (recvSize > 0)
                        {
                            session.EmptyMsgCount = 0; // 重置EmptyMsgCount
                            if (session.Remaining == 0)
                            {
                                this.ResolveHeader(session); // 处理报文头
                            }
                            else
                            {
                                this.HandleData(session, recvSize);
                            }
                        }
                        else
                        {
                            // 接收到空数据
                            session.EmptyMsgCount++;
                            if (session.EmptyMsgCount > this.MaxEmptyMessage)
                            {
                                logger.InfoFormat("{0}空数据达到{1}次，关闭通道", session.RemoteEndPoint, session.EmptyMsgCount);
                                this.CloseSession(session, SocketError.ConnectionReset);
                            }
                            else
                            {
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
                        this.CloseSession(session, socketError);
                        break;

                    default:
                        // 尝试恢复, 重试上次任务
                        this.ResumeLastReceive(session);
                        break;
                }
            }
            catch (SocketException ex)
            {
                logger.Warn("接收数据Socket异常", ex);
                this.CloseSession(session, SocketError.SocketError);
            }
            catch (ObjectDisposedException)
            {
                // 收到该Exception说明Client Socket已被关闭。BeginReceive操作被取消，无需进行任何处理
            }
            catch (Exception ex)
            {
                logger.Warn("接收数据异常", ex); // 出现未知异常，关闭session
                this.CloseSession(session, SocketError.SocketError);
            }
        }

        /// <summary> 
        /// 发送数据完成处理函数 
        /// </summary> 
        /// <param name="iar">目标客户端Socket</param> 
        private void OnDataSent(IAsyncResult iar)
        {
            Session session = iar.AsyncState as Session;

            try
            {
                SocketError socketError = SocketError.SocketError;
                Socket clientSocket = session.ClientSocket;
                if (clientSocket == null)
                {
                    // 说明socket为空说明session已被服务器主动关闭, 不做任何处理
                    return;
                }

                int sent = clientSocket.EndSend(iar, out socketError);
                if (this.DataSent != null)
                {
                    this.DataSent(this, new DataSentEventArgs(clientSocket, sent, socketError));
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
                        this.CloseSession(session, socketError);
                        return;

                    default:
                        break;
                }
            }
            catch (SocketException ex)
            {
                logger.Warn("发送数据Socket异常", ex);
                this.CloseSession(session, SocketError.SocketError);
            }
            catch (ObjectDisposedException)
            {
                // 收到该Exception说明Client Socket已被关闭。BeginReceive操作被取消，无需进行任何处理
            }
            catch (Exception ex)
            {
                logger.Warn("发送异常", ex);
                this.CloseSession(session, SocketError.SocketError);
            }
        }

        /// <summary> 
        /// 关闭一个客户端Session. 
        /// </summary> 
        /// <param name="session">目标session对象</param> 
        /// <param name="errorCode">客户端退出的SocketError Code</param> 
        private void CloseSession(Session session, SocketError errorCode)
        {
            try
            {
                Session outValue;
                if (this.sessionTable.TryRemove(session.ID, out outValue))
                {
                    Socket clientSocket = session.ClientSocket;

                    //发送客户端关闭事件
                    if (this.ClientClosed != null)
                    {
                        this.ClientClosed(this, new NetEventArgs(clientSocket, errorCode));
                    }

                    // 释放资源并关闭Socket
                    this.sessionManager.ReleaseSession(session);
                }
                else
                {
                    logger.WarnFormat("没有找到需删除的session: {0}", session.RemoteEndPoint);
                }
            }
            catch (Exception ex)
            {
                logger.Warn("关闭Session异常", ex);
            }
        }

        /// <summary> 
        /// 通过Socket对象查找ClientSocket对象 
        /// </summary> 
        /// <param name="client"></param> 
        /// <returns>找到的Session对象,如果为null,说明并不存在该回话</returns> 
        private Session FindSession(int clientHandle)
        {
            Session session;
            return this.sessionTable.TryGetValue(clientHandle, out session) ? session : null;
        }

        /// <summary>
        /// 解析报文头
        /// </summary>
        /// <param name="session"></param>
        private void ResolveHeader(Session session)
        {
            int messageSize = this.headerResolver.ResolveHeader(session.ReceiveBuffer, session.BufferOffset);
            if (messageSize > 0)
            {
                logger.DebugFormat("{0}报文头, 报文大小:{1}", session.RemoteEndPoint, messageSize);

                // 接收报文时, 如果 报文长度+HeaderSize大于缓冲区大小，则会分多次接收
                // 处理报文头的时候并不触发报文事件，而是等到报文数据全部到达或者是每一次报文包到达后触发
                this.ReceiveData(session, this.headerResolver.HeaderSize,
                    Math.Min(this.receiveBufferSize - this.headerResolver.HeaderSize, messageSize), 0, messageSize);
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
                logger.WarnFormat("{0}无效报文头, 执行关闭通道策略", session.RemoteEndPoint);
                this.CloseSession(session, SocketError.ConnectionReset);
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
                Debug.Assert(dataSize <= this.receiveBufferSize);

                if (this.DataReceived != null)
                {
                    // 触发数据到达事件
                    this.DataReceived(this, new DataReceivedEventArgs(session.ClientSocket, session.ReceiveBuffer,
                        session.BufferOffset, dataSize, session.Remaining, session.PackageIndex));
                }

                if (session.Remaining > 0)
                {
                    // 本次报文还有没收到的部分，继续接收
                    logger.DebugFormat("{0}剩余报文:{1}", session.RemoteEndPoint, session.Remaining);
                    this.ReceiveData(session, 0, Math.Min(session.Remaining, this.receiveBufferSize), session.PackageIndex + 1, session.Remaining);
                }
                else
                {
                    // 本次报文已经在该次接收中全部完成。开始下一次报文头的接收
                    logger.DebugFormat("{0}接收报文完毕", session.RemoteEndPoint);
                    this.ReceiveData(session, 0, this.headerResolver.HeaderSize, 0, 0);
                }
            }
        }

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
            session.PackageIndex = packageIndex;
            session.DataInBuffer = dataInBuffer;
            session.Expected = expectedSize;
            session.Remaining = remaining;
            session.ClientSocket.BeginReceive(session.ReceiveBuffer, session.BufferOffset + session.DataInBuffer,
                session.Expected, SocketFlags.None, this.dataReceivedCallback, session);
        }

        /// <summary>
        /// 根据当前记录重试上次的任务
        /// </summary>
        /// <param name="session"></param>
        private void ResumeLastReceive(Session session)
        {
            // 当前处于正常数据接收中（Header或Data）
            session.ClientSocket.BeginReceive(session.ReceiveBuffer, session.BufferOffset + session.DataInBuffer, session.Expected, SocketFlags.None,
                this.dataReceivedCallback, session);
        }

        #endregion

    } 
}
