///////////////////////////////////////////////////////
//NSTCPFramework
//�汾��1.0.0.1
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
    /// �ṩTcp�������ӷ���Ŀͻ����� 
    /// 1.ʹ���첽��SocketͨѶ����BeginXXX��Ϊ�������������ͨѶ���ܡ�
    ///   �����뷢�͹����໥�����ţ�ʵ����TCPȫ˫����
    /// 2.�ͻ������ӷ���ɹ��󣬽������������չ��̡�ÿ�ν������Ƚ���Messageͷ��Ȼ�����
    ///   ����ͷ����������ʣ�౨�ĳ��ȣ�����һ�λ��߶�εĽ��ա��ɴ�������ְ����⡣�����ĵĳ���
    ///   С�ڿ��Խ��ܵĻ�������Сʱ�������ཫ����յ����б��ĺ󴥷����ݽ����¼�����������ཫ��
    /// </summary> 
    public class TcpCli
    {
        #region �����ֶ�

        /// <summary> 
        /// ȱʡ�������ݻ�������С8K 
        /// </summary> 
        public const int DefaultBufferSize = 8096;

        /// <summary>
        /// �������յ�����Ϣ�Ĵ������ڸ�ֵ������������رո�����
        /// </summary>
        public const int DefaultMaxEmptyMessage = 5;

        #endregion

        #region ˽�б���

        /// <summary> 
        /// �������ݻỰ 
        /// </summary> 
        private Session session;

        /// <summary>
        /// receive buffer
        /// </summary>
        private byte[] receiveBuffer;

        /// <summary>
        /// ͷ������
        /// </summary>
        private IDataResolver headerResolver;

        /// <summary>
        /// �������ݻص�Callback
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

        #region ���캯��

        /// <summary>
        /// ���캯��
        /// </summary>
        /// <param name="headerSize">Լ����TCP Messageͷ��С</param>
        /// <param name="headerResolver">����ͷ��������</param>
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

        #region �¼�����

        /// <summary> 
        /// �������¼� 
        /// </summary> 
        public event EventHandler<NetEventArgs> Connected;

        /// <summary> 
        /// ���ӶϿ��¼� 
        /// </summary> 
        public event EventHandler<NetEventArgs> Disconnected;

        /// <summary> 
        /// ���յ����ݱ����¼� 
        /// </summary> 
        public event EventHandler<DataReceivedEventArgs> DataReceived;

        /// <summary>
        /// ���ͽ���
        /// </summary>
        public event EventHandler<DataSentEventArgs> DataSent;

        #endregion

        #region ����

        /// <summary> 
        /// ���ؿͻ����������֮�������״̬ 
        /// </summary> 
        public bool IsConnected
        {
            get;
            private set;
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
        /// ���ӷ����� 
        /// </summary> 
        /// <param name="ip">������IP��ַ</param> 
        /// <param name="port">�������˿�</param> 
        public bool BeginConnect(string host, int port)
        {
            if (this.IsConnected)
            {
                // �رպ��������� 
                this.Disconnect();
            }

            logger.InfoFormat("��ʼ����{0}:{1}", host, port);
            Socket newsock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            newsock.BeginConnect(host, port, new AsyncCallback(this.OnConnected), newsock);
            return true;
        }

        /// <summary> 
        /// �������ݱ��� 
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
        /// �ر����� 
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

        #region �ܱ�������

        /// <summary>
        /// �ڲ����á��ᴥ���Ͽ��¼�
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
        /// ���ݷ�����ɴ����� 
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
                    // ˵��socketΪ��˵��session�ѱ������������ر�, �����κδ���
                    return;
                }

                int sent = socket.EndSend(iar, out socketError);
                if (this.DataSent != null)
                {
                    this.DataSent(this, new DataSentEventArgs(socket, sent, socketError));
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
                        this.Close(socketError);
                        return;

                    default:
                        break;
                }
            }
            catch (SocketException ex)
            {
                logger.Warn("��������Socket�쳣", ex);
                this.Close(ex.SocketErrorCode);
            }
            catch (ObjectDisposedException)
            {
                // �յ���Exception˵��Client Socket�ѱ��رա�BeginReceive������ȡ������������κδ���
            }
            catch (Exception ex)
            {
                logger.Warn("�����쳣", ex);
                this.Close(SocketError.SocketError);
            }
        }

        /// <summary> 
        /// ����Tcp���Ӻ������ 
        /// </summary> 
        /// <param name="iar">�첽Socket</param> 
        protected virtual void OnConnected(IAsyncResult iar)
        {
            Socket socket = (Socket)iar.AsyncState;

            try
            {
                socket.EndConnect(iar);

                //�����»Ự 
                this.session = new Session(this.receiveBuffer, 0);
                this.session.SetSocket(socket);

                this.IsConnected = true;

                // �������ӽ����¼� 
                if (this.Connected != null)
                {
                    this.Connected(this, new NetEventArgs(socket));
                }

                // �����»Ự��������ʼ�������� 
                logger.InfoFormat("����{0}���չ���", this.session.RemoteEndPoint);
                this.ReceiveData(session, 0, this.headerResolver.HeaderSize, 0, 0);
            }
            catch (SocketException ex)
            {
                logger.WarnFormat("�������Ӵ���:{0}", ex.SocketErrorCode);

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
                logger.Error("���ӽ��������쳣", ex);

                // �ر�socket
                if (socket != null)
                {
                    socket.Close();

                    // �������ӽ����¼�
                    if (this.Connected != null)
                    {
                        this.Connected(this, new NetEventArgs(socket, SocketError.SocketError));
                    }
                }
            }
        }

        /// <summary> 
        /// ���ݽ��մ����� 
        /// </summary> 
        /// <param name="iar">�첽Socket</param> 
        protected virtual void OnDataReceived(IAsyncResult iar)
        {
            Session session = iar.AsyncState as Session;

            try
            {
                SocketError socketError;
                Socket socket = session.ClientSocket;
                if (socket == null)
                {
                    // ˵��socketΪ��˵���ôλص�����Ϊsession�ѱ������رպ�
                    // ����BeginReceive�������ģ���˲����κδ���
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
                                this.ResolveHeader(session); // ������ͷ
                            }
                            else
                            {
                                this.HandleData(session, receiveSize);
                            }
                        }
                        else
                        {
                            // ���յ�������
                            session.EmptyMsgCount++;
                            if (session.EmptyMsgCount > this.MaxEmptyMessage)
                            {
                                logger.InfoFormat("{0}�����ݴﵽ{1}�Σ��ر�ͨ��", session.RemoteEndPoint, session.EmptyMsgCount);
                                this.Close(SocketError.ConnectionReset);
                                return;
                            }
                            else 
                            {
                                // �ظ���һ�εĽ���Message����
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
                        this.Close(socketError);
                        return;

                    default:
                        // ���Իָ�, �����ϴ�����
                        this.ResumeLastReceive(session);
                        break;
                }
            }
            catch (SocketException ex)
            {
                logger.Warn("��������Socket�쳣", ex);
                this.Close(ex.SocketErrorCode);
            }
            catch (ObjectDisposedException)
            {
                // �յ���Exception˵��Client Socket�ѱ��رա�BeginReceive������ȡ������������κδ���
            }
            catch (Exception ex)
            {
                // �����쳣���ر�socket
                logger.Warn("���������쳣", ex);
                this.Close(SocketError.SocketError);
            }
        }

        #endregion

        #region ˽�з���

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
            // ����һ��ResolveHeader����handle dataʱ�����ܻᱻ�ⲿ������DataReceived�¼��е���close���¼��ص������󻹻�ִ��ReceiveData
            // ����ʱ��ClientSocket���ܻ�Ϊ��
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
        /// ���ݵ�ǰ��¼�����ϴε�����
        /// </summary>
        /// <param name="session"></param>
        private void ResumeLastReceive(Session session)
        {
            session.ClientSocket.BeginReceive(this.receiveBuffer, session.DataInBuffer, session.Expected, SocketFlags.None,
                this.dataReceivedCallback, session);
        }

        /// <summary>
        /// ��������ͷ
        /// </summary>
        /// <param name="session"></param>
        private void ResolveHeader(Session session)
        {
            try
            {
                int messageSize = this.headerResolver.ResolveHeader(this.receiveBuffer, 0);
                if (messageSize < 0)
                {
                    // �ôα���ͷ������, ���öϿ��ͻ������ӵķ�ʽ�������
                    logger.WarnFormat("{0}��Ч����ͷ, ִ�йر�ͨ������", session.RemoteEndPoint);
                    this.Close(SocketError.ConnectionReset);
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
                    logger.DebugFormat("{0}����ͷ, ���Ĵ�С:{1}", session.RemoteEndPoint, messageSize);

                    // ���ձ���ʱ, ��� ���ĳ���+HeaderSize���ڻ�������С�����ֶ�ν���
                    // ������ͷ��ʱ�򲢲����������¼������ǵȵ���������ȫ�����������ÿһ�α��İ�����󴥷�
                    this.ReceiveData(session, this.headerResolver.HeaderSize,
                        Math.Min(this.receiveBuffer.Length - this.headerResolver.HeaderSize, messageSize), 0, messageSize);
                }
            }
            catch
            {
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
            try
            {
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
                    Debug.Assert(dataSize <= this.receiveBuffer.Length);

                    if (this.DataReceived != null)
                    {
                        // �������ݵ����¼�
                        this.DataReceived(this, new DataReceivedEventArgs(session.ClientSocket, this.receiveBuffer, session.BufferOffset, dataSize, session.Remaining, session.PackageIndex));
                    }

                    if (session.Remaining > 0)
                    {
                        // ���α��Ļ���û�յ��Ĳ��֣���������
                        logger.DebugFormat("{0}ʣ�౨��:{1}", session.RemoteEndPoint, session.Remaining);
                        this.ReceiveData(session, 0, Math.Min(session.Remaining, this.receiveBuffer.Length), session.PackageIndex + 1, session.Remaining);
                    }
                    else
                    {
                        // ���α����Ѿ��ڸôν�����ȫ����ɡ���ʼ��һ�α���ͷ�Ľ���
                        logger.DebugFormat("{0}���ձ������", session.RemoteEndPoint);
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
