using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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
        /// 该block是否是从bucket里分配的
        /// </summary>
        internal bool AllocateFromBucket { get; set; }

        /// <summary>
        /// 表示该段内存的状态
        /// 暂时未用到
        /// </summary>
        internal BlockState State { get; set; }

        /// <summary>
        /// 存储该段缓存数据的字节数组的引用
        /// </summary>
        public byte[] Data { get; set; }

        ///// <summary>
        ///// 该段缓存数据再Data里的偏移量
        ///// </summary>
        //public int Offset { get; set; }

        /// <summary>
        /// 该段缓存数据的长度
        /// </summary>
        public int Size { get; set; }
    }

    internal class ByteBucket
    {
        #region 实例变量

        private int blockSize;
        private int numberOfBlock;
        private SpinLock queueLock;

        /// <summary>
        /// 当前block的数量
        /// </summary>
        private int numblk;

        private Queue<ByteBlock> blockQueue;

        #endregion

        #region 属性

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="blockSize">每个内存块的大小</param>
        /// <param name="numberOfBlock">为该内存池有多少个内存块</param>
        public ByteBucket(int blockSize, int numberOfBlock)
        {
            this.blockSize = blockSize;
            this.numberOfBlock = numberOfBlock;
            this.queueLock = new SpinLock();
            this.blockQueue = new Queue<ByteBlock>();
        }

        public ByteBlock Obtain()
        {
            bool lockTaken = false;
            this.queueLock.Enter(ref lockTaken);

            try
            {
                if (this.blockQueue.Count == 0)
                {
                    if (this.numblk < this.numberOfBlock)
                    {
                        this.numblk++;
                        ByteBlock block = new ByteBlock()
                        {
                            Data = new byte[this.blockSize],
                            Size = this.blockSize,
                            AllocateFromBucket = true,
                        };
                        return block;
                    }

                    return null;
                }

                return this.blockQueue.Dequeue();
            }
            finally
            {
                if (lockTaken)
                {
                    this.queueLock.Exit(false);
                }
            }
        }

        public void Recycle(ByteBlock bytes)
        {
            bool lockTaken = false;
            this.queueLock.Enter(ref lockTaken);

            try
            {
                this.blockQueue.Enqueue(bytes);
            }
            finally
            {
                if (lockTaken)
                {
                    this.queueLock.Exit(false);
                }
            }
        }
    }

    /// <summary>
    /// Byte数组的缓存池
    /// 实现思路：
    /// 预先分配一块超大的byte数组，然后通过数据所在byte数组的偏移量和数据大小来确定每一块数据的位置
    /// </summary>
    public class BytePool
    {
        #region 实例变量

        private ByteBucket[] buckets;

        #endregion

        #region 构造方法

        private BytePool()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxArrayLength">block的最大长度</param>
        /// <param name="maxArraysPerBucket">每个bucket里的block的数量，如果这个数量不够用的，那么就直接再开辟一段新的内存空间返回</param>
        private BytePool(int maxArrayLength, int maxArraysPerBucket)
        {
            int maxBuckets = SelectBucketIndex(maxArrayLength);

            // bucket在总缓冲区中的偏移位置
            int startOffset = 0;
            this.buckets = new ByteBucket[maxBuckets + 1];
            for (int i = 0; i < buckets.Length; i++)
            {
                this.buckets[i] = new ByteBucket(GetMaxSizeForBucket(i), maxArraysPerBucket);

                // 当前bucket里的块大小
                int blockSize = GetMaxSizeForBucket(i);

                // 当前bucket的总大小
                int bucketSize = blockSize * maxArraysPerBucket;

                startOffset += bucketSize;
            }
        }

        #endregion

        /// <summary>
        /// 创建一个新的字节缓存池
        /// </summary>        
        /// <param name="maxArrayLength">block的最大长度</param>
        /// <param name="maxArraysPerBucket">每个bucket里的block的数量，如果这个数量不够用的，那么就直接再开辟一段新的内存空间返回</param>
        /// <returns></returns>
        public static BytePool Create(int maxArrayLength, int maxArraysPerBucket)
        {
            BytePool pool = new BytePool(maxArrayLength, maxArraysPerBucket);
            return pool;
        }

        #region 实例方法

        #endregion

        #region 静态方法

        /// <summary>
        /// 根据bufferSize计算bucket的索引
        /// </summary>
        /// <param name="bufferSize"></param>
        /// <returns></returns>
        internal static int SelectBucketIndex(int bufferSize)
        {
            uint bitsRemaining = ((uint)bufferSize - 1) >> 4;

            int poolIndex = 0;
            if (bitsRemaining > 0xFFFF) { bitsRemaining >>= 16; poolIndex = 16; }
            if (bitsRemaining > 0xFF) { bitsRemaining >>= 8; poolIndex += 8; }
            if (bitsRemaining > 0xF) { bitsRemaining >>= 4; poolIndex += 4; }
            if (bitsRemaining > 0x3) { bitsRemaining >>= 2; poolIndex += 2; }
            if (bitsRemaining > 0x1) { bitsRemaining >>= 1; poolIndex += 1; }

            return poolIndex + (int)bitsRemaining;
        }

        /// <summary>
        /// 根据bucket索引计算bucket里的每个block的大小
        /// </summary>
        /// <param name="binIndex"></param>
        /// <returns></returns>
        internal static int GetMaxSizeForBucket(int binIndex)
        {
            int maxSize = 16 << binIndex;
            return maxSize;
        }

        #endregion

        #region 公开接口

        /// <summary>
        /// 从缓存池里获取一个缓存的字节数组
        /// 如果缓存里没有数据了，那么会自动扩容
        /// </summary>
        /// <returns></returns>
        public ByteBlock Obtain(int size)
        {
            int bucketIndex = SelectBucketIndex(size);

            // 先查找该大小是否有对应的bucket
            if (bucketIndex > buckets.Length - 1)
            {
                // 没有则开辟一段新的内存
                return new ByteBlock() { Data = new byte[size], Size = size, AllocateFromBucket = false };
            }

            // 从bucket里获取一个block
            ByteBucket bucket = this.buckets[bucketIndex];

            // 如果有，那么从bucket里获取一段内存
            // 如果获取的block为空，那么说明没有多余的block可供使用了
            // 那么就直接新开辟一段内存
            ByteBlock block = bucket.Obtain();
            if (block == null)
            {
                return new ByteBlock() { Data = new byte[size], Size = size, AllocateFromBucket = false };
            }

            return block;
        }

        /// <summary>
        /// 回收一个用完了的字节数组
        /// </summary>
        /// <param name="block"></param>
        public void Recycle(ByteBlock block)
        {
            if (!block.AllocateFromBucket)
            {
                return;
            }

            int bucketIndex = SelectBucketIndex(block.Size);

            if (bucketIndex > buckets.Length - 1)
            {
                return;
            }

            ByteBucket bucket = this.buckets[bucketIndex];
            bucket.Recycle(block);
        }

        #endregion
    }
}
