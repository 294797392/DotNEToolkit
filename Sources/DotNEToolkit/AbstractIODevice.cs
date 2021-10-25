using DotNEToolkit.Modular;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit
{
    public abstract class AbstractIODevice : ModuleBase
    {
        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("AbstractIODevice");

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
        /// 判断IO设备是否已连接
        /// </summary>
        /// <returns></returns>
        public abstract bool IsConnected();

        /// <summary>
        /// 连接IO设备
        /// </summary>
        /// <returns></returns>
        public abstract int Connect();

        /// <summary>
        /// 与IO设备断开连接
        /// </summary>
        /// <returns></returns>
        public abstract int Disconnect();

        protected abstract int ReadBytes(byte[] bytes, int offset, int count);

        protected abstract int WriteBytes(byte[] bytes, int offset, int count);

        public abstract int ReadLine(out string line);

        public abstract int WriteLine(string line);

        #endregion

        #region 公开接口

        /// <summary>
        /// 读取bytes大小的数据
        /// </summary>
        /// <param name="bytes">要存储读取的数据的缓冲区</param>
        /// <returns></returns>
        public int ReadBytes(byte[] bytes)
        {
            int total = bytes.Length;
            int readed = 0;
            while (readed != total)
            {
                int size = this.ReadBytes(bytes, readed, total - readed);
                if (size == -1) 
                {
                    return DotNETCode.FAILED;
                }
                readed += size;
                logger.InfoFormat("读取的字节数 = {0}", readed);
            }

            return DotNETCode.SUCCESS;
        }

        /// <summary>
        /// 写入bytes大小的数据
        /// </summary>
        /// <param name="bytes">要写入的数据</param>
        /// <returns></returns>
        public int WriteBytes(byte[] bytes)
        {
            int total = bytes.Length;
            int writed = 0;
            while (writed != total)
            {
                int size = this.WriteBytes(bytes, writed, total - writed);
                if (size == -1)
                {
                    return DotNETCode.FAILED;
                }
                writed += size;
                logger.InfoFormat("写入的字节数 = {0}", writed);
            }

            return DotNETCode.SUCCESS;
        }

        #endregion
    }

    public static class AbstractIODeviceExtension
    {
        private const int DefaultTimeout = 30000;

        public static int ReadMatches(this AbstractIODevice device, string match, int timeout, out string line)
        {
            line = null;

            DateTime start = DateTime.Now;

            while ((DateTime.Now - start).TotalMilliseconds < timeout)
            {
                string readed;
                int code = device.ReadLine(out readed);
                if (code != DotNETCode.SUCCESS)
                {
                    return code;
                }

                if (!readed.Contains(match))
                {
                    continue;
                }

                line = readed;
                return DotNETCode.SUCCESS;
            }

            return DotNETCode.TIMEOUT;
        }

        public static int ReadMatches(this AbstractIODevice device, string match, out string line)
        {
            return ReadMatches(device, match, DefaultTimeout, out line);
        }
    }
}