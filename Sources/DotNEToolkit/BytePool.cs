using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit
{
    internal enum BlockState
    {
        /// <summary>
        /// 使用中..
        /// </summary>
        Used,

        /// <summary>
        /// 未使用..
        /// </summary>
        Unused,
    }

    /// <summary>
    /// 存储一段缓存的字节数组
    /// </summary>
    public class ByteBlock
    {
        /// <summary>
        /// 表示该段内存的状态
        /// 暂时未用到
        /// </summary>
        internal BlockState State { get; set; }

        /// <summary>
        /// 存储该段缓存数据的字节数组的引用
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// 该段缓存数据再Data里的偏移量
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// 该段缓存数据的长度
        /// </summary>
        public int Size { get; set; }
    }

    /// <summary>
    /// Byte数组的缓存池
    /// 实现思路：
    /// 预先分配一块超大的byte数组，然后通过数据所在byte数组的偏移量和数据大小来确定每一块数据的位置
    /// </summary>
    public class BytePool
    {
        #region 实例变量

        private byte[] baseBytes;

        private Queue<ByteBlock> blockQueue;
        private object queueLock;

        /// <summary>
        /// 字节块的数量
        /// </summary>
        private int numblk;

        #endregion

        #region 构造方法

        private BytePool(int count, int size)
        {
            // TODO：做字节对齐
            this.baseBytes = new byte[count * size];
            this.blockQueue = new Queue<ByteBlock>(count);
            this.queueLock = new object();
            this.numblk = count;

            // 先初始化baseBytes，baseBytes里的每个元素的值不为0，才会占用内存空间
            for (int i = 0; i < this.baseBytes.Length; i++)
            {
                this.baseBytes[i] = 1;
            }

            // 预先把blockQueue分配好
            for (int i = 0; i < count; i++)
            {
                ByteBlock block = new ByteBlock()
                {
                    Data = this.baseBytes,
                    Offset = i * size,
                    Size = size,
                    State = BlockState.Unused
                };
                this.blockQueue.Enqueue(block);
            }
        }

        #endregion

        /// <summary>
        /// 创建一个新的字节缓存池
        /// </summary>
        /// <param name="numblk">字节块数量</param>
        /// <param name="blksize">每个字节块的大小</param>
        /// <returns></returns>
        public static BytePool Create(int numblk, int blksize)
        {
            BytePool pool = new BytePool(numblk, blksize);
            return pool;
        }

        #region 公开接口

        /// <summary>
        /// 从缓存池里获取一个缓存的字节数组
        /// 如果缓存里没有数据了，那么会自动扩容
        /// </summary>
        /// <returns></returns>
        public ByteBlock Obtain()
        {
            lock (this.queueLock)
            {
                if (this.blockQueue.Count == 0)
                {

                }
                else
                {
                    return this.blockQueue.Dequeue();
                }
            }
        }

        /// <summary>
        /// 回收一个用完了的字节数组
        /// </summary>
        /// <param name="bytes"></param>
        public void Recycle(ByteBlock bytes)
        {
            lock (this.queueLock)
            {
                this.blockQueue.Enqueue(bytes);
            }
        }

        #endregion
    }
}
