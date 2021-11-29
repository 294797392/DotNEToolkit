///////////////////////////////////////////////////////
//NSTCPFramework
//版本：1.0.0.1
//////////////////////////////////////////////////////
using System;
using System.Net.Sockets;

namespace DotNEToolkit.TcpFramework
{
    /// <summary> 
    /// 服务器程序的事件参数,包含了激发该事件的Socket对象 
    /// </summary> 
    public class NetEventArgs : EventArgs
    {
        public NetEventArgs(Socket clientSocket)
            : this(clientSocket, SocketError.Success)
        {
        }

        /// <summary> 
        /// 构造函数 
        /// </summary> 
        /// <param name="client">客户端会话</param> 
        public NetEventArgs(Socket clientSocket, SocketError code)
        {
            // 可能在发送事件时ClientSocket已经被清空了
            //if (null == clientSocket)
            //{
            //    throw (new ArgumentNullException());
            //}

            this.ClientSocket = clientSocket;
            this.ClientHandle = clientSocket != null ? clientSocket.Handle.ToInt32() : 0;
            this.SocketError = code;
        }

        /// <summary> 
        /// 获得激发该事件的会话对象 
        /// </summary> 
        public Socket ClientSocket
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Socket的Handle
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
    /// TCP数据接收事件参数
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
        /// 接收到的数据缓冲区
        /// </summary>
        public byte[] Data
        {
            get;
            private set;
        }

        /// <summary>
        /// 接收到的数据在缓冲区的起始位置
        /// </summary>
        public int StartOffset
        {
            get;
            private set;
        }

        /// <summary>
        /// 接收到的数据长度
        /// </summary>
        public int DataSize
        {
            get;
            private set;
        }

        /// <summary>
        /// 当前报文剩余字节数
        /// </summary>
        public int RemainSize
        {
            get;
            private set;
        }

        /// <summary>
        /// 如果message太大一次接收不完，该属性将记录
        /// 接收该message时所触发回调的Index. 从0开始计数
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
