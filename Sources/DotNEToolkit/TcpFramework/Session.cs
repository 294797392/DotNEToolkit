using System;
using System.Net.Sockets;

namespace DotNEToolkit.TcpFramework
{
    /// <summary> 
    /// 客户端与服务器之间通讯的会话类。该会话类主要管理某个Socket接收事务，
    /// 包括分包管理，缓冲区等等。
    /// </summary> 
    internal sealed class Session
    {
        /// <summary>
        /// 构造接收通讯会话。Session中记录了TCP Message接收时需要保存的一些中间变量，如TCP Message的
        /// 头，接收Message使用的缓冲区，剩余message的长度等等。为了减少内存碎片，Session在接收TCP 
        /// Message头时直接使用BufferManager预先分配的大块内存。然后再根据TCP Message头中定义的message
        /// 长度，来动态决定下一次需要接收的数据大小，Socket接收缓冲区。接收完毕后，直接将缓冲区传递给上层应用，可以避免
        /// 内存的复制。
        /// </summary>
        /// <param name="BufferOffset">session使用的缓冲区偏移</param>
        /// <param name="buffer">接收缓冲区</param>
        internal Session(byte[] buffer, int bufferOffset)
        {
            this.ReceiveBuffer = buffer;
            this.BufferOffset = bufferOffset;

            this.Expected = 0;
            this.DataInBuffer = 0;
            this.Remaining = 0;
            this.PackageIndex = 0;
            this.EmptyMsgCount = 0;
        }

        /// <summary> 
        /// 返回会话的ID. 小于0 则为未连接
        /// </summary>
        public int ID
        {
            get;
            private set;
        }

        /// <summary>
        /// 远端接入点，用于调试或显示
        /// </summary>
        public string RemoteEndPoint
        {
            get;
            private set;
        }

        /// <summary> 
        /// 获得与客户端会话关联的Socket对象 
        /// </summary> 
        public Socket ClientSocket
        {
            get;
            private set;
        }

        /// <summary>
        /// 本次处理的报文接收中剩余的字节数
        /// </summary>
        public int Remaining
        {
            get;
            set;
        }

        /// <summary>
        /// 下一次接收期望的数据大小
        /// </summary>
        public int Expected
        {
            get;
            set;
        }

        /// <summary>
        /// 当前buffer里保存的数据长度
        /// </summary>
        public int DataInBuffer
        {
            get;
            set;
        }

        /// <summary>
        /// 如果一个message过大，一次接收不完。
        /// 该字段记录已经接收的该message并触发回调的次数
        /// </summary>
        public int PackageIndex
        {
            get;
            set;
        }

        /// <summary>
        /// Message缓冲区
        /// </summary>
        public byte[] ReceiveBuffer
        {
            get;
            private set;
        }

        /// <summary>
        /// 在接收下一次数据时，应该从该偏移位置保存接收到的数据。
        /// </summary>
        public int BufferOffset
        {
            get;
            private set;
        }

        /// <summary>
        /// 记录连续收到的空消息事件记录. 在很多情况下，当远端socket断开时，
        /// 异步接收函数不会收到ConnectionReset错误。而是直接返回空消息。如果服务器连续n次接收到空消息，
        /// 那么可以认为远端Socket已经断开。
        /// </summary>
        public int EmptyMsgCount
        {
            get;
            set;
        }

        /// <summary>
        /// 设置Session关联的Socket
        /// </summary>
        /// <param name="cliSocket"></param>
        public void SetSocket(Socket cliSocket)
        {
            this.ClientSocket = cliSocket;
            this.ID = cliSocket.Handle.ToInt32();
            this.RemoteEndPoint = cliSocket.RemoteEndPoint.ToString();
        }

        /// <summary> 
        /// 关闭会话 
        /// </summary> 
        public void Close()
        {
            if (this.ClientSocket != null)
            {
                try
                {
                    //关闭数据的接受和发送 
                    this.ClientSocket.Shutdown(SocketShutdown.Both);

                    //清理资源 
                    this.ClientSocket.Close();
                }
                catch 
                {

                }
                finally
                {
                    this.ClientSocket = null;

                    this.ID = 0;
                    this.RemoteEndPoint = string.Empty;

                    //this.ReceiveBuffer = null;
                    //this.BufferOffset = 0;
                    this.Expected = 0;
                    this.Remaining = 0;

                    this.EmptyMsgCount = 0;                    
                }
            }
        }

        #region override

        /// <summary> 
        /// 使用Socket对象的Handle值作为HashCode,它具有良好的线性特征. 
        /// </summary> 
        /// <returns></returns> 
        public override int GetHashCode()
        {
            return this.ID;
        }

        /// <summary> 
        /// 返回两个Session是否代表同一个客户端 
        /// </summary> 
        /// <param name="obj"></param> 
        /// <returns></returns> 
        public override bool Equals(object obj)
        {
            Session rightObj = (Session)obj;

            return this.ID == (int)rightObj.ID;

        }

        /// <summary> 
        /// 重载ToString()方法,返回Session对象的特征 
        /// </summary> 
        /// <returns></returns> 
        public override string ToString()
        {
            return string.Format("Session{0}({1})",
                this.ID, this.ClientSocket.RemoteEndPoint);
        }

        #endregion
    }
}