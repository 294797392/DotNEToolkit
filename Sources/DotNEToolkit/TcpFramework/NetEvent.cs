///////////////////////////////////////////////////////
//NSTCPFramework
//�汾��1.0.0.1
//////////////////////////////////////////////////////
using System;
using System.Net.Sockets;

namespace DotNEToolkit.TcpFramework
{
    /// <summary> 
    /// ������������¼�����,�����˼������¼���Socket���� 
    /// </summary> 
    public class NetEventArgs : EventArgs
    {
        public NetEventArgs(Socket clientSocket)
            : this(clientSocket, SocketError.Success)
        {
        }

        /// <summary> 
        /// ���캯�� 
        /// </summary> 
        /// <param name="client">�ͻ��˻Ự</param> 
        public NetEventArgs(Socket clientSocket, SocketError code)
        {
            // �����ڷ����¼�ʱClientSocket�Ѿ��������
            //if (null == clientSocket)
            //{
            //    throw (new ArgumentNullException());
            //}

            this.ClientSocket = clientSocket;
            this.ClientHandle = clientSocket != null ? clientSocket.Handle.ToInt32() : 0;
            this.SocketError = code;
        }

        /// <summary> 
        /// ��ü������¼��ĻỰ���� 
        /// </summary> 
        public Socket ClientSocket
        {
            get;
            private set;
        }

        /// <summary>
        /// ��ȡSocket��Handle
        /// </summary>
        public int ClientHandle
        {
            get;
            private set;
        }

        public SocketError SocketError
        {
            get;
            private set;
        }
    }

    /// <summary>
    /// TCP���ݽ����¼�����
    /// </summary>
    public class DataReceivedEventArgs : NetEventArgs
    {
        public DataReceivedEventArgs(Socket clientSocket, byte[] receiveBuffer, int startOffset, int dataSize, int remainSize, int packageIndex)
            : base(clientSocket, SocketError.Success)
        {
            this.Data = receiveBuffer;
            this.DataSize = dataSize;
            this.StartOffset = startOffset;

            this.RemainSize = remainSize;
            this.PackageIndex = packageIndex;
        }

        /// <summary>
        /// ���յ������ݻ�����
        /// </summary>
        public byte[] Data
        {
            get;
            private set;
        }

        /// <summary>
        /// ���յ��������ڻ���������ʼλ��
        /// </summary>
        public int StartOffset
        {
            get;
            private set;
        }

        /// <summary>
        /// ���յ������ݳ���
        /// </summary>
        public int DataSize
        {
            get;
            private set;
        }

        /// <summary>
        /// ��ǰ����ʣ���ֽ���
        /// </summary>
        public int RemainSize
        {
            get;
            private set;
        }

        /// <summary>
        /// ���message̫��һ�ν��ղ��꣬�����Խ���¼
        /// ���ո�messageʱ�������ص���Index. ��0��ʼ����
        /// </summary>
        public int PackageIndex
        {
            get;
            private set;
        }
    }

    public class DataSentEventArgs : NetEventArgs
    {
        public DataSentEventArgs(Socket clientSocket, int dataSent)
            : this(clientSocket, dataSent, SocketError.Success)
        {
        }

        public DataSentEventArgs(Socket clientSocket, int dataSent, SocketError errorCode)
            : base(clientSocket, errorCode)
        {
            this.DataSize = dataSent;
        }

        public int DataSize
        {
            get;
            private set;
        }
    }
}
