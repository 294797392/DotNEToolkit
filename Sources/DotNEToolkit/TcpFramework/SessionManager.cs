
using System.Net.Sockets;
using System.Collections.Concurrent;
namespace DotNEToolkit.TcpFramework
{
    /// <summary>
    /// SessionManager主要用于管理和回收服务器，客户端直接通讯的Session。
    /// 为减少内存碎片，提高效率，SessionManager将预先分配一大块内存区域作为缓冲，供多个Session同时使用。
    /// </summary>
    internal sealed class SessionManager
    {
        #region 私有变量

        /// <summary>
        /// 每片message头的大小
        /// </summary>
        private int blockSize;

        /// <summary>
        /// 记录可用的bufferIndex. 如果freeSessions有Size限制，
        /// 可以考虑使用freeBufferStack来减少内存消耗
        /// </summary>
        //private int[] freeBufferStack;
        //private int stackIdx; 
        //private object stackLock = new object();
        
        private ConcurrentStack<Session> freeSessions;

        /// <summary>
        /// 缓冲区
        /// </summary>
        private byte[] buffer;

        /// <summary>
        /// 已分配出的buffer offset
        /// </summary>
        private int unusedOffset;

        #endregion

        /// <summary>
        /// 初始化函数
        /// </summary>
        /// <param name="max">最大的缓冲数量</param>
        /// <param name="blockSize">每个缓冲区的大小</param>
        public SessionManager(int max, int blockSize)
        {
            this.blockSize = blockSize;
            this.Capacity = max;

            int size = max * blockSize;
            this.buffer = new byte[size];

            // 初始化空闲buffer堆栈
            this.freeSessions = new ConcurrentStack<Session>();
            this.unusedOffset = 0;
        }

        /// <summary>
        /// 报文头接收缓冲
        /// </summary>
        public byte[] Buffer
        {
            get { return this.buffer; }
        }

        /// <summary>
        /// 每片缓冲长度
        /// </summary>
        public int BlockSize
        {
            get { return this.blockSize; }
        }

        /// <summary>
        /// 可以支持的session最大数量
        /// </summary>
        public int Capacity
        {
            get;
            private set;
        }

        /// <summary>
        /// 申请一个会话对象
        /// </summary>
        /// <param name="cliSock">通讯Socket</param>
        /// <returns></returns>
        public Session RequestSession(Socket cliSock)
        {
            Session session;
            if (!this.freeSessions.TryPop(out session))
            {
                if (this.unusedOffset < this.buffer.Length)
                {
                    // 缓冲池还有空余缓冲空间，创建一个新回话对象
                    session = new Session(this.buffer, this.unusedOffset);
                    this.unusedOffset += this.blockSize;
                }
                else
                {
                    // 没有足够的缓冲了
                    return null;
                }
            }

            // 设置Socket
            session.SetSocket(cliSock);
            return session;
        }

        /// <summary>
        /// 回收会话对象
        /// </summary>
        /// <param name="session"></param>
        public void ReleaseSession(Session session)
        {
            session.Close(); // 关闭会话
            this.freeSessions.Push(session);// 回收缓冲对象以备下次使用
        }

        /// <summary>
        /// 重置BufferManager
        /// </summary>
        public void Clear()
        {
            this.freeSessions.Clear();
            this.unusedOffset = 0;
        }
    }
}
