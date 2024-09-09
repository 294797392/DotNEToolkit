using DotNEToolkit.Modular;
using Factory.NET.IODrivers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Factory.NET
{
    /// <summary>
    /// 实现了DUT最基本的功能
    /// 1.DUT连接管理
    /// 2.DUT的输入和输出管理
    /// 3.DUT接收和发送缓冲区管理，保证缓冲区不会溢出
    /// </summary>
    public abstract class DUTClient : ModuleBase
    {
        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("DUTClient");

        #endregion

        #region 实例变量

        protected AbstractIODriver driver;

        #endregion

        #region 属性

        /// <summary>
        /// 输入/输出驱动
        /// </summary>
        public AbstractIODriver Driver { get { return this.driver; } }

        #endregion

        #region 公开接口

        /// <summary>
        ///控制DUT执行一个简单的动作
        /// </summary>
        /// <param name="action"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public abstract int Control(string cmd, object param);

        #endregion

        #region 实例方法

        protected void WriteBytes(byte[] bytes)
        {
            this.driver.WriteBytes(bytes);
        }

        /// <summary>
        /// 不知道要读取多少字节，使用重试机制来读取
        /// </summary>
        /// <param name="bytes">存储要读取的数据</param>
        /// <param name="times">一共读取多少次</param>
        /// <returns>
        /// 大于等于0：读取到的字节数
        /// 小于0：失败
        /// </returns>
        protected int ReadBytes(byte[] bytes, int times)
        {
            byte[] message = bytes;

            int read = 0;
            int left = bytes.Length;

            for (int i = 0; i < times; i++)
            {
                int rc = 0;

                try
                {
                    rc = this.driver.ReadBytes(message, read, left);
                }
                catch (Exception ex)
                {
                    logger.Error("读取数据异常", ex);
                    return -1;
                }

                if (rc == 0)
                {
                    // 读完了
                    break;
                }
                else if (rc < 0)
                {
                    // 读取失败
                    return -1;
                }
                else
                {
                    read += rc;
                    left -= rc;
                }
            }

            return read;
        }

        protected byte[] ReadBytes(int len) 
        {
            byte[] message = new byte[len];

            int left = len;
            int read = 0;

            while (left > 0)
            {
                int n = this.driver.ReadBytes(message, read, left);

                left -= n;
                read += n;
            }

            return message;
        }

        protected void WriteLine(string line)
        {
            logger.DebugFormat("write = {0}", line);
            this.driver.WriteLine(line);
        }

        protected string ReadLine()
        {
            return this.driver.ReadLine();
        }



        protected void ClearReceivingBuffer()
        {
            this.driver.ClearExisting();
        }

        protected int SubmitLine(AbstractIODriver driver, string line, string match1, out string matchedLine, string match2 = "")
        {
            return driver.SubmitLine(line, match1, out matchedLine, match2);
        }

        protected int SubmitLine(AbstractIODriver driver, string line, string match1, string match2 = "")
        {
            string matchedLine;
            return this.SubmitLine(driver, line, match1, out matchedLine, match2);
        }

        protected int SubmitLine(string line, string match1, out string matchedLine, string match2 = "")
        {
            return this.SubmitLine(this.driver, line, match1, out matchedLine, match2);
        }

        protected int SubmitLine(string line, string match1, string match2 = "")
        {
            return this.SubmitLine(this.driver, line, match1, match2);
        }

        protected int ReadMatches(string match, MatchingRules rules = MatchingRules.Contains)
        {
            return this.driver.ReadMatches(match, rules);
        }

        #endregion

        #region 事件处理器

        #endregion
    }
}
