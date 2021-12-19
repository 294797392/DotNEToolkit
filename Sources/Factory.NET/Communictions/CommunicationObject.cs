using DotNEToolkit.Modular;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Factory.NET.Communictions
{
    /// <summary>
    /// 通信对象模型
    /// </summary>
    public abstract class CommunicationObject : ModuleBase
    {
        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("CommunicationObject");

        #endregion

        #region ModuleBase

        public override int Initialize(IDictionary parameters)
        {
            return base.Initialize(parameters);
        }

        public override void Release()
        {
            base.Release();
        }

        #endregion

        #region 抽象接口

        /// <summary>
        /// 判断IO设备是否已经打开
        /// </summary>
        /// <returns></returns>
        public abstract bool IsOpened();

        /// <summary>
        /// 打开一个IO设备
        /// </summary>
        /// <returns></returns>
        public abstract int Open();

        /// <summary>
        /// 关闭IO设备
        /// </summary>
        /// <returns></returns>
        public abstract int Close();

        public abstract int ReadLine(out string line);

        public abstract int WriteLine(string line);

        protected abstract int ReadBytes(byte[] bytes, int offset, int count);

        protected abstract int WriteBytes(byte[] bytes, int offset, int count);

        #endregion

        #region 公开接口

        /// <summary>
        /// 读取bytes大小的数据
        /// </summary>
        /// <param name="bytes">要存储读取的数据的缓冲区</param>
        /// <returns></returns>
        public bool ReadBytes(byte[] bytes)
        {
            int total = bytes.Length;
            int readed = 0;
            while (readed != total)
            {
                int size = this.ReadBytes(bytes, readed, total - readed);
                if (size == -1)
                {
                    return false;
                }
                readed += size;
                logger.DebugFormat("读取的字节数 = {0}", readed);
            }

            return true;
        }

        /// <summary>
        /// 写入bytes大小的数据
        /// </summary>
        /// <param name="bytes">要写入的数据</param>
        /// <returns></returns>
        public bool WriteBytes(byte[] bytes)
        {
            int total = bytes.Length;
            int writed = 0;
            while (writed != total)
            {
                int size = this.WriteBytes(bytes, writed, total - writed);
                if (size == -1)
                {
                    return false;
                }
                writed += size;
                logger.DebugFormat("写入的字节数 = {0}", writed);
            }

            return true;
        }

        #endregion
    }
}
