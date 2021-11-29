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
    /// �ṩTCP���ӷ���ķ������ࡣ 
    /// 
    /// 1. ʹ���첽��Socket�¼���Ϊ���������ȫ˫��������ͨѶ����. 
    /// 2. �÷�����ÿ��Accept�ͻ������Ӻ󣬽������������չ��̡�ÿ�ν������Ƚ���Messageͷ��Ȼ�����
    ///    ����ͷ����������ʣ�౨�ĳ��ȣ�����һ�λ��߶�εĽ��ա��ɴ�������ְ����⡣�����ĵĳ���
    ///    С�ڿ��Խ��ܵĻ�������Сʱ�������ཫ����յ����б��ĺ󴥷����ݽ����¼�����������ཫ��
    ///    ����������ݽ����¼���ֱ�����Ľ�����ɡ�Ȼ�������һ�ν���״̬��
    /// 3. �����෢��ʱҲ�����첽���͹��̡��뷢�͹����໥�����š�
    /// </summary> 
    public class TcpSvr
    {
        #region �����ֶ�

        /// <summary>
        /// Ĭ�ϲ������𣬼�ͬһʱ�̷���sessionTable�Ĳ����߳�����
        /// ��ο�ConcurrentDictionary concurrentLevel���塣
        /// </summary>
        public const int DefaultConcurrentLevel = 30;

        /// <summary> 
        /// Ĭ�ϵķ�����������ӿͻ��˶����� 
        /// </summary> 
        public const int DefaultMaxClient = 3000;

        /// <summary> 
        /// ȱʡHeader��С8K 
        /// </summary> 
        public const int DefaultHeaderSize = 256;

        /// <summary>
        /// TCP Listen backlog
        /// </summary>
        public const int DefaultBacklog = 512;

        /// <summary>
        /// ���ջ�������С
        /// </summary>
        public const int DefaultBufferSize = 8192;

        /// <summary>
        /// �������յ�����Ϣ�Ĵ������ڸ�ֵ������������رո�����
        /// </summary>
        public const int DefaultMaxEmptyMessage = 5;

        #endregion

        #region ˽�б���

        /// <summary>
        /// �������𡣶���Ϊͬһʱ�̷���sessionTable�Ĳ����߳�����
        /// ��ο�ConcurrentDictionary concurrentLevel���塣
        /// </summary>
        private int concurrentLevel;

        /// <summary>
        /// �������ݻص�Callback
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
        /// ���������������IP��ַ
        /// </summary>
        private IPAddress serverIP;

        /// <summary> 
        /// ����������ʹ�õĶ˿� 
        /// </summary> 
        private ushort port;

        /// <summary> 
        /// ������������״̬ 
        /// </summary> 
        private bool isRun;

        /// <summary> 
        /// ������ʹ�õ��첽Socket��, 
        /// </summary> 
        private Socket listenSocket;

        /// <summary> 
        /// �������пͻ��˻Ự�Ĺ�ϣ�� 
        /// </summary> 
        private ConcurrentDictionary<int, Session> sessionTable;

        /// <summary>
        /// Session Manager
        /// </summary>
        private SessionManager sessionManager;

        /// <summary>
        /// TCP���ݽ�����
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

        #region ���캯��

        /// <summary>
        /// ���캯��
        /// </summary>
        /// <param name="ipAddress">����IP</param>
        /// <param name="port">�������˼����Ķ˿ں�</param>
        /// <param name="headerResolver">����ͷ��������</param>
        /// <param name="maxClient">������������ɿͻ�����</param>
        /// <param name="headerSize">����ͷ����</param>
        /// <param name="maxRecvBufSize">ÿ�ν��ձ��ĵ�����С</param>
        /// <param name="concurrentLevel">�����������ȡ���Ҫ������Listener��backlog�Լ�sessionMap�Ĳ�������</param>
        public TcpSvr(IPAddress ipAddress, ushort port, IDataResolver headerResolver,
            int maxClient = DefaultMaxClient, 
            int maxRecvBufSize = DefaultBufferSize,
            int concurrentLevel = DefaultConcurrentLevel)
        {
            this.Initialize(ipAddress, port, headerResolver, maxClient, maxRecvBufSize, concurrentLevel);
        }

        /// <summary>
        /// ���캯��
        /// </summary>
        /// <param name="ipAddress">����IP</param>
        /// <param name="port">�������˼����Ķ˿ں�</param>
        /// <param name="headerResolver">����ͷ��������</param>
        /// <param name="maxClient">������������ɿͻ�����</param>
        /// <param name="headerSize">����ͷ����</param>
        /// <param name="maxRecvBufSize">ÿ�ν��ձ��ĵ�����С</param>
        /// <param name="concurrentLevel">�����������ȡ���Ҫ������Listener��backlog�Լ�sessionMap�Ĳ�������</param>
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

        #region �¼�����

        /// <summary> 
        /// �ͻ��˽��������¼� 
        /// </summary> 
        public event EventHandler<NetEventArgs> ClientConnected;

        /// <summary> 
        /// �ͻ��˹ر��¼� 
        /// </summary> 
        public event EventHandler<NetEventArgs> ClientClosed;

        /// <summary> 
        /// �������Ѿ����¼� 
        /// </summary> 
        public event EventHandler<NetEventArgs> ServerFull;

        /// <summary> 
        /// ���������յ������¼� 
        /// </summary> 
        public event EventHandler<DataReceivedEventArgs> DataReceived;

        /// <summary>
        /// ���ݷ��ͽ���
        /// </summary>
        public event EventHandler<DataSentEventArgs> DataSent;

        #endregion

        #region ����

        /// <summary> 
        /// ��������Socket���� 
        /// </summary> 
        public Socket ServerSocket
        {
            get
            {
                return this.listenSocket;
            }
        }

        /// <summary> 
        /// �������������ɿͻ��˵�������� 
        /// </summary> 
        public int Capacity
        {
            get
            {
                return this.sessionManager.Capacity;
            }
        }

        /// <summary> 
        /// ��ǰ�Ŀͻ��������� 
        /// </summary> 
        public int SessionCount
        {
            get
            {
                return this.sessionTable.Count;
            }
        }

        /// <summary> 
        /// ����������״̬ 
        /// </summary> 
        public bool IsRun
        {
            get
            {
                return this.isRun;
            }
        }

        /// <summary>
        /// ���ĳ��clientSocket                                     
        /// �������յ�����Ϣ�Ĵ������ڸ�ֵ������������رո�����
        /// </summary>
        public int MaxEmptyMessage
        {
            get;
            set;
        }

        #endregion

        #region ���з���

        /// <summary> 
        /// ��������������,��ʼ�����ͻ������� 
        /// </summary> 
        public void Start()
        {
            if (this.isRun)
            {
                logger.Warn("TcpSvr�Ѿ�������.");
                return;
            }

            this.sessionTable = new ConcurrentDictionary<int, Session>(this.concurrentLevel, this.sessionManager.Capacity);

            //�󶨶˿� 
            IPEndPoint iep = new IPEndPoint(this.serverIP, this.port);

            //��ʼ��socket 
            this.listenSocket = new Socket(iep.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            this.listenSocket.Bind(iep);

            //��ʼ���� 
            logger.InfoFormat("��ʼ���� {0}:{1}", this.serverIP, this.port);
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

                        // �ȴ��ź�׼���������ܿͻ��� 
                        this.acceptDone.WaitOne();
                    }
                    catch (Exception ex)
                    {
                        logger.Error("listen�쳣", ex);
                    }
                }
            });
            this.listeningThread.IsBackground = true;
            this.listeningThread.Start();
#endif            
            this.isRun = true;
        }

        /// <summary> 
        /// ֹͣ����������,������ͻ��˵����ӽ��ر� 
        /// </summary> 
        public void Stop()
        {
            if (this.isRun)
            {
                this.isRun = false;

#if AsyncAccept
                try
                {
                    this.listeningThread.Abort(); // ֹͣ�����߳�
                }
                catch (Exception ex)
                {
                    logger.Warn("ֹͣ�����쳣", ex);
                }
#endif
                //�ر������˿�
                if (this.listenSocket.Connected)
                {
                    this.listenSocket.Shutdown(SocketShutdown.Both);
                }

                // ��������¼�����ֹ���ִ�����client disconnected�¼�
                this.ClientClosed = null;
                this.ClientConnected = null;
                this.DataReceived = null;
                this.DataSent = null;
                this.ServerFull = null;

                this.CloseAllClient();

                //������Դ 
                this.listenSocket.Close();
                this.sessionTable = null;

                this.sessionManager.Clear();
            }
        }

        /// <summary> 
        /// �ر����еĿͻ��˻Ự,�����еĿͻ������ӻ�Ͽ���
        /// �����������ClientClosed�¼�
        /// </summary> 
        public void CloseAllClient()
        {
            try
            {
                logger.Info("�ر�����session");
                List<Session> sessionList = new List<Session>(this.sessionTable.Values);
                this.sessionTable.Clear();

                foreach (Session session in sessionList)
                {
                    this.sessionManager.ReleaseSession(session);
                }
            }
            catch (Exception ex)
            {
                logger.Error("�������session�쳣", ex);
            }
        }

        /// <summary> 
        /// �ر�һ����ͻ���֮��ĻỰ 
        /// </summary> 
        /// <param name="clientSocket">��Ҫ�رյĿͻ��˻Ự����</param> 
        public void CloseClient(int clientHandle)
        {
            try
            {
                Session session;
                if (this.sessionTable.TryRemove(clientHandle, out session))
                {
                    logger.DebugFormat("�رտͻ���{0}", session.RemoteEndPoint);
                    Socket clientSocket = session.ClientSocket;

                    //���Ϳͻ��˹ر��¼�
                    if (this.ClientClosed != null)
                    {
                        this.ClientClosed(this, new NetEventArgs(clientSocket, SocketError.Disconnecting));
                    }

                    this.sessionManager.ReleaseSession(session);
                }
                else
                {
                    logger.WarnFormat("û���ҵ���ɾ����Handle: {0}", clientHandle);
                }
            }
            catch (Exception ex)
            {
                logger.Warn("�ر��쳣", ex);
            }
        }

        /// <summary> 
        /// �������� 
        /// </summary> 
        /// <param name="clientSession">�������ݵĿͻ��˻Ự</param> 
        /// <param name="datagram">���ݱ���</param> 
        public SocketError SendAsync(int clientHandle, byte[] data)
        {
            Session session = this.FindSession(clientHandle);
            if (session != null)
            {
                SocketError code;
                logger.InfoFormat("��ʼ��{0}����", session.RemoteEndPoint);
                session.ClientSocket.BeginSend(data, 0, data.Length, SocketFlags.None, out code,
                    this.dataSentCallback, session);
                return code;
            }
            else
            {
                // ����Client������
                return SocketError.NotConnected;
            }
        }

        /// <summary>
        /// ��ȡClient Socket
        /// </summary>
        /// <param name="clientHandle"></param>
        /// <returns></returns>
        public Socket GetClientSocket(int clientHandle)
        {
            Session session = this.FindSession(clientHandle);
            return session != null ? session.ClientSocket : null;
        }

        #endregion

        #region ˽�з���

        /// <summary>
        /// ���캯��
        /// </summary>
        /// <param name="ipAddress">����IP</param>
        /// <param name="port">�������˼����Ķ˿ں�</param>
        /// <param name="headerResolver">����ͷ��������</param>
        /// <param name="maxClient">������������ɿͻ�����</param>
        /// <param name="recvBufSize">���ջ���Ĵ�С</param>
        /// <param name="concurrentLevel">�����������ȡ���Ҫ������Listener��backlog�Լ�sessionMap�Ĳ�������</param>
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
        /// �ͻ������Ӵ����� 
        /// </summary> 
        /// <param name="iar">���������������ӵ�Socket����</param> 
        private void OnConnectionAccepted(IAsyncResult iar)
        {
            if (!this.isRun)
            {
                return;
            }

            #region ����һ���ͻ��˵��������� 

            Socket client = null;
            try
            {
                logger.InfoFormat("���ս�������");
                Socket listener = (Socket)iar.AsyncState;
                client = listener.EndAccept(iar);
            }
            catch (ObjectDisposedException)
            {
                // �յ���Exception˵��Client Socket�ѱ��رա�BeginReceive������ȡ������������κδ���
                return;
            }
            catch (Exception ex)
            {
                // ���տͻ��˽��������쳣
                logger.Warn("Accept�쳣", ex);
                return;
            }
            finally
            {
                // ֪ͨaccept�߳̿�ʼ������һ��accept
#if AsyncAccept
                this.acceptDone.Set();
#else
                this.listenSocket.BeginAccept(this.connectionAcceptedCallback, this.listenSocket);
#endif
            }

            #endregion

            #region ������������

            if (this.SessionCount >= this.sessionManager.Capacity)
            {
                logger.InfoFormat("�ͻ����ѵ������ֵ{0}, �ر�����:{1}", this.sessionManager.Capacity, client.RemoteEndPoint);
                client.Close();

                if (this.ServerFull != null)
                {
                    this.ServerFull(this, new NetEventArgs(client));
                }
            }
            else
            {
                //�½�һ��session
                Session session = this.sessionManager.RequestSession(client);
                Debug.Assert(session != null); // ���ͻ�����sessionManager����ƥ�䣬���sessionӦ�ò�Ϊ��

                if (session != null)
                {
                    if (this.sessionTable.TryAdd(session.ID, session))
                    {
                        // �����µĿͻ������¼� 
                        if (this.ClientConnected != null)
                        {
                            this.ClientConnected(this, new NetEventArgs(session.ClientSocket));
                        }

                        logger.InfoFormat("����{0}���չ���, ��ʼ����header", session.RemoteEndPoint);
                        this.ReceiveData(session, 0, this.headerResolver.HeaderSize, 0, 0);
                    }
                    else
                    {
                        logger.WarnFormat("���{0}ʧ��, �ر�����", session.RemoteEndPoint);
                        this.sessionManager.ReleaseSession(session);
                    }
                }
            }

            #endregion
        }

        /// <summary> 
        /// ����������ɴ�����. ���յ����ĺ�server�����ж�session��ǰ��״̬��
        /// 1. ���ձ���ͷ��״̬��Session.Remaining==0��: 
        ///    ���񽫵���headerResolver��������ͷ����ȷ���������ı��Ĵ�С��ͬʱ�������ջ��塣
        /// 2. ���ձ���״̬(Session.Remaining>0)��
        ///    �յ����ġ����񽫴������ձ����¼�
        /// 
        /// </summary> 
        /// <param name="iar">Ŀ��ͻ���Socket</param> 
        private void OnDataReceived(IAsyncResult iar)
        {
            Session session = iar.AsyncState as Session;

            try
            {
                SocketError socketError;
                Socket clientSocket = session.ClientSocket;
                if (clientSocket == null)
                {
                    // ˵��socketΪ��˵���ôλص�����Ϊsession�ѱ������������رպ�
                    // ����BeginReceive�������ģ���˲����κδ���
                    return;
                }

                int recvSize = clientSocket.EndReceive(iar, out socketError);
                switch (socketError)
                {
                    case SocketError.Success:      
                        if (recvSize > 0)
                        {
                            session.EmptyMsgCount = 0; // ����EmptyMsgCount
                            if (session.Remaining == 0)
                            {
                                this.ResolveHeader(session); // ������ͷ
                            }
                            else
                            {
                                this.HandleData(session, recvSize);
                            }
                        }
                        else
                        {
                            // ���յ�������
                            session.EmptyMsgCount++;
                            if (session.EmptyMsgCount > this.MaxEmptyMessage)
                            {
                                logger.InfoFormat("{0}�����ݴﵽ{1}�Σ��ر�ͨ��", session.RemoteEndPoint, session.EmptyMsgCount);
                                this.CloseSession(session, SocketError.ConnectionReset);
                            }
                            else
                            {
                                logger.InfoFormat("{0}�յ�������{1}�Σ������ϴν���", session.RemoteEndPoint, session.EmptyMsgCount);
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
                    case SocketError.SocketError: // δ֪����
                        logger.DebugFormat("{0}���մ���, �رն˿�:{1}", session.RemoteEndPoint, socketError);
                        this.CloseSession(session, socketError);
                        break;

                    default:
                        // ���Իָ�, �����ϴ�����
                        this.ResumeLastReceive(session);
                        break;
                }
            }
            catch (SocketException ex)
            {
                logger.Warn("��������Socket�쳣", ex);
                this.CloseSession(session, SocketError.SocketError);
            }
            catch (ObjectDisposedException)
            {
                // �յ���Exception˵��Client Socket�ѱ��رա�BeginReceive������ȡ������������κδ���
            }
            catch (Exception ex)
            {
                logger.Warn("���������쳣", ex); // ����δ֪�쳣���ر�session
                this.CloseSession(session, SocketError.SocketError);
            }
        }

        /// <summary> 
        /// ����������ɴ����� 
        /// </summary> 
        /// <param name="iar">Ŀ��ͻ���Socket</param> 
        private void OnDataSent(IAsyncResult iar)
        {
            Session session = iar.AsyncState as Session;

            try
            {
                SocketError socketError = SocketError.SocketError;
                Socket clientSocket = session.ClientSocket;
                if (clientSocket == null)
                {
                    // ˵��socketΪ��˵��session�ѱ������������ر�, �����κδ���
                    return;
                }

                int sent = clientSocket.EndSend(iar, out socketError);
                if (this.DataSent != null)
                {
                    this.DataSent(this, new DataSentEventArgs(clientSocket, sent, socketError));
                }

                // ����SocketError
                switch (socketError)
                {
                    case SocketError.Success:
                        logger.DebugFormat("��{0}����{1}�ֽ����", session.RemoteEndPoint, sent);
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
                    case SocketError.SocketError: // δ֪����
                        logger.InfoFormat("{0}���ʹ���, �رն˿�:{1}", session.RemoteEndPoint, socketError);
                        this.CloseSession(session, socketError);
                        return;

                    default:
                        break;
                }
            }
            catch (SocketException ex)
            {
                logger.Warn("��������Socket�쳣", ex);
                this.CloseSession(session, SocketError.SocketError);
            }
            catch (ObjectDisposedException)
            {
                // �յ���Exception˵��Client Socket�ѱ��رա�BeginReceive������ȡ������������κδ���
            }
            catch (Exception ex)
            {
                logger.Warn("�����쳣", ex);
                this.CloseSession(session, SocketError.SocketError);
            }
        }

        /// <summary> 
        /// �ر�һ���ͻ���Session. 
        /// </summary> 
        /// <param name="session">Ŀ��session����</param> 
        /// <param name="errorCode">�ͻ����˳���SocketError Code</param> 
        private void CloseSession(Session session, SocketError errorCode)
        {
            try
            {
                Session outValue;
                if (this.sessionTable.TryRemove(session.ID, out outValue))
                {
                    Socket clientSocket = session.ClientSocket;

                    //���Ϳͻ��˹ر��¼�
                    if (this.ClientClosed != null)
                    {
                        this.ClientClosed(this, new NetEventArgs(clientSocket, errorCode));
                    }

                    // �ͷ���Դ���ر�Socket
                    this.sessionManager.ReleaseSession(session);
                }
                else
                {
                    logger.WarnFormat("û���ҵ���ɾ����session: {0}", session.RemoteEndPoint);
                }
            }
            catch (Exception ex)
            {
                logger.Warn("�ر�Session�쳣", ex);
            }
        }

        /// <summary> 
        /// ͨ��Socket�������ClientSocket���� 
        /// </summary> 
        /// <param name="client"></param> 
        /// <returns>�ҵ���Session����,���Ϊnull,˵���������ڸûػ�</returns> 
        private Session FindSession(int clientHandle)
        {
            Session session;
            return this.sessionTable.TryGetValue(clientHandle, out session) ? session : null;
        }

        /// <summary>
        /// ��������ͷ
        /// </summary>
        /// <param name="session"></param>
        private void ResolveHeader(Session session)
        {
            int messageSize = this.headerResolver.ResolveHeader(session.ReceiveBuffer, session.BufferOffset);
            if (messageSize > 0)
            {
                logger.DebugFormat("{0}����ͷ, ���Ĵ�С:{1}", session.RemoteEndPoint, messageSize);

                // ���ձ���ʱ, ��� ���ĳ���+HeaderSize���ڻ�������С�����ֶ�ν���
                // ������ͷ��ʱ�򲢲����������¼������ǵȵ���������ȫ�����������ÿһ�α��İ�����󴥷�
                this.ReceiveData(session, this.headerResolver.HeaderSize,
                    Math.Min(this.receiveBufferSize - this.headerResolver.HeaderSize, messageSize), 0, messageSize);
            }
            else if (messageSize == 0)
            {
                // ���α�������Ϊ0��˵��ֻ��������ͷ����ô�������ݽ����Ѿ�ȫ����ɡ���ʼ��һ�α���ͷ�Ľ���
                logger.DebugFormat("{0}���ձ������", session.RemoteEndPoint);

                // �������ݵ����¼�: ��������ֻ��������ͷ
                if (this.DataReceived != null)
                {
                    this.DataReceived(this, new DataReceivedEventArgs(session.ClientSocket, session.ReceiveBuffer,
                        session.BufferOffset, this.headerResolver.HeaderSize, 0, 0));
                }

                this.ReceiveData(session, 0, this.headerResolver.HeaderSize, 0, 0);
            }
            else
            {
                logger.WarnFormat("{0}��Ч����ͷ, ִ�йر�ͨ������", session.RemoteEndPoint);
                this.CloseSession(session, SocketError.ConnectionReset);
            }
        }

        /// <summary>
        /// �����յ��ı�������
        /// </summary>
        /// <param name="session"></param>
        /// <param name="receivedSize"></param>
        private void HandleData(Session session, int receivedSize)
        {
            Debug.Assert(receivedSize <= session.Expected); // recvSize�����ܴ���session.Expected

            session.Remaining -= receivedSize;
            if (receivedSize < session.Expected)
            {
                // ���յ������ݱ�Ԥ�ڵ���, Ҳ���Ƿ��ͷ����ͷְ������⡣
                // ��ʱ�����ȴ����ͷ���������ֱ��������Ϣ������ٴ������ݴﵽ�¼�
                logger.InfoFormat("{0}Ԥ�ڴ�С��һ��{1}/{2}", session.RemoteEndPoint, session.Expected, receivedSize);
                this.ReceiveData(session, session.DataInBuffer + receivedSize, session.Expected - receivedSize, session.PackageIndex, session.Remaining);
            }
            else if (receivedSize == session.Expected)
            {
                int dataSize = session.DataInBuffer + receivedSize;
                Debug.Assert(dataSize <= this.receiveBufferSize);

                if (this.DataReceived != null)
                {
                    // �������ݵ����¼�
                    this.DataReceived(this, new DataReceivedEventArgs(session.ClientSocket, session.ReceiveBuffer,
                        session.BufferOffset, dataSize, session.Remaining, session.PackageIndex));
                }

                if (session.Remaining > 0)
                {
                    // ���α��Ļ���û�յ��Ĳ��֣���������
                    logger.DebugFormat("{0}ʣ�౨��:{1}", session.RemoteEndPoint, session.Remaining);
                    this.ReceiveData(session, 0, Math.Min(session.Remaining, this.receiveBufferSize), session.PackageIndex + 1, session.Remaining);
                }
                else
                {
                    // ���α����Ѿ��ڸôν�����ȫ����ɡ���ʼ��һ�α���ͷ�Ľ���
                    logger.DebugFormat("{0}���ձ������", session.RemoteEndPoint);
                    this.ReceiveData(session, 0, this.headerResolver.HeaderSize, 0, 0);
                }
            }
        }

        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="session"></param>
        /// <param name="dataInBuffer">��ǰbuffer���е����ݳ���</param>
        /// <param name="expectedSize">�������յ����ݴ�С</param>
        /// <param name="packageIndex">��һ�ν��յİ�</param>
        /// <param name="remaining">��ǰ�Ựʣ����ֽ���</param>
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
        /// ���ݵ�ǰ��¼�����ϴε�����
        /// </summary>
        /// <param name="session"></param>
        private void ResumeLastReceive(Session session)
        {
            // ��ǰ�����������ݽ����У�Header��Data��
            session.ClientSocket.BeginReceive(session.ReceiveBuffer, session.BufferOffset + session.DataInBuffer, session.Expected, SocketFlags.None,
                this.dataReceivedCallback, session);
        }

        #endregion

    } 
}
