using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using DotNEToolkit;
using DotNEToolkit.Modular;
using log4net.Config;
using Newtonsoft.Json;

namespace Factory.NET.Channels
{
    public enum IODriverProperies
    {
        /// <summary>
        /// 换行符
        /// </summary>
        NewLine,
    }

    /// <summary>
    /// 描述一个具有输入输出功能的设备对象
    /// 1.管理IODevice的连接
    /// 2.管理IODevice的输入和输出
    /// 3.向外部模块提供输入/输出接口
    /// </summary>
    public abstract class ChannelBase
    {
        private const string DefaultNewLine = "\r\n";
        private const int DefaultReadTimeout = 30;          // 读取数据超时时间，单位是秒
        private const int DefaultWriteTimeout = 30;         // 写入数据超时时间，单位是秒
        private const int DefaultReadBufferSize = 16384;    // 接收缓冲区大小
        private const int DefaultWriteBufferSize = 16384;    // 发送缓冲区大小

        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("AbstractIODriver");

        #endregion

        #region 实例变量

        protected ChannelStatus status;
        protected string newLine;
        protected int readTimeout;
        protected int writeTimeout;
        protected int readBufferSize;
        protected int writeBufferSize;
        protected int submitTimeout;

        #endregion

        #region 属性

        public abstract ChannelTypes Type { get; }

        /// <summary>
        /// 模块的输入参数
        /// </summary>
        internal IDictionary InputParameters { get; set; }

        #endregion

        #region 构造方法

        public ChannelBase()
        {
        }

        #endregion

        #region 公开接口

        public int Initialize(IDictionary dictionary)
        {
            this.InputParameters = dictionary;

            this.newLine = this.GetParameter<string>("NewLine", DefaultNewLine);
            this.readTimeout = this.GetParameter<int>("ReadTimeout", DefaultReadTimeout * 1000);
            this.writeTimeout = this.GetParameter<int>("WriteTimeout", DefaultWriteTimeout * 1000);
            this.readBufferSize = this.GetParameter<int>("ReadBufferSize", DefaultReadBufferSize);
            this.writeBufferSize = this.GetParameter<int>("WriteBufferSize", DefaultWriteBufferSize);
            this.submitTimeout = this.GetParameter<int>("SubmitTimeout", DefaultWriteTimeout * 1000);

            return this.OnInitialize();
        }

        public void Release()
        {
            this.OnRelease();
        }

        #endregion

        #region 实例方法

        protected T GetParameter<T>(IDictionary dictionary, string key)
        {
            Type t = typeof(T);

            if (t == typeof(string))
            {
                return dictionary.GetValue<T>(key);
            }

            if (t.IsClass)
            {
                string json = dictionary[key].ToString();
                return JsonConvert.DeserializeObject<T>(json);
            }

            return dictionary.GetValue<T>(key);
        }

        #endregion

        #region 抽象方法

        protected abstract int OnInitialize();

        protected abstract void OnRelease();

        /// <summary>
        /// 读取一次数据就返回，不关心读取了多少数据
        /// </summary>
        /// <param name="bytes">存储读取的数据缓冲区</param>
        /// <param name="offset">存储读取到的数据的偏移量</param>
        /// <param name="len">要读取的数据长度</param>
        /// <returns>读取到的字节数，有可能是0</returns>
        public abstract int ReadBytes(byte[] bytes, int offset, int len);

        /// <summary>
        /// 读取指定长度的数据，如果读取不到指定长度的数据会一直阻塞
        /// </summary>
        /// <param name="size">要读取的数据长度</param>
        /// <returns></returns>
        public abstract byte[] ReadBytesFull(int size);

        /// <summary>
        /// 从IO驱动读取一行数据
        /// 该方法不捕捉异常，需要调用者处理异常情况
        /// </summary>
        /// <returns></returns>
        public abstract string ReadLine();


        public abstract void WriteBytes(byte[] bytes);

        public abstract void WriteLine(string line);

        /// <summary>
        /// 清空接收缓冲区
        /// 防止在缓冲区满了的时候接收出问题
        /// </summary>
        public abstract void ClearExisting();

        #endregion

        #region 公开接口

        /// <summary>
        /// 读取该模块的输入参数，如果参数不存在则报异常
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T GetParameter<T>(string key)
        {
            IDictionary parameters = this.InputParameters;

            if (!parameters.Contains(key))
            {
                logger.ErrorFormat("没有找到必需的参数:{0}", key);
                throw new KeyNotFoundException();
            }

            return this.GetParameter<T>(parameters, key);
        }

        /// <summary>
        /// 读取该模块的输入参数，如果参数不存在则返回defaultValue
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T GetParameter<T>(string key, T defaultValue)
        {
            IDictionary parameters = this.InputParameters;

            if (!parameters.Contains(key))
            {
                return defaultValue;
            }

            return this.GetParameter<T>(parameters, key);
        }

        /// <summary>
        /// 一直读取数据，读取到match为止，并把读取到的数据返回。超时时间使用默认超时时间
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="match"></param>
        /// <param name="timeout"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        public int ReadMatches(string match, MatchingRules rules, out string line)
        {
            return this.ReadMatches(match, rules, this.readTimeout, out line);
        }

        public int ReadMatches(string match, MatchingRules rules, int timeout, out string line)
        {
            List<string> matches = new List<string>() { match };

            return ReadMatches(matches, rules, timeout, out line);
        }

        public int ReadMatches(IEnumerable<string> matches, MatchingRules rules, int timeout, out string line)
        {
            line = null;

            DateTime start = DateTime.Now;
            while (true)
            {
                // 如果超时时间不是无限时长，那么才去判断是否超时
                if (timeout != Timeout.Infinite)
                {
                    if ((DateTime.Now - start).TotalMilliseconds > timeout)
                    {
                        return ResponseCode.IODRV_READ_TIMEOUT;
                    }
                }

                try
                {
                    line = this.ReadLine();
                }
                catch (Exception ex)
                {
                    logger.Error("读取异常", ex);
                    return ResponseCode.FAILED;
                }

                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                logger.DebugFormat("IODriver OUT = {0}", line);

                line = line.Trim();

                switch (rules)
                {
                    case MatchingRules.Contains:
                        {
                            foreach (string match in matches)
                            {
                                if (line.Contains(match))
                                {
                                    return ResponseCode.SUCCESS;
                                }
                            }
                            continue;
                        }

                    case MatchingRules.StartWith:
                        {
                            foreach (string match in matches)
                            {
                                if (line.StartsWith(match))
                                {
                                    return ResponseCode.SUCCESS;
                                }
                            }
                            continue;
                        }

                    default:
                        throw new NotImplementedException();
                }
            }
        }

        /// <summary>
        /// 一直读取数，读取到match为止，可以自己指定超时时间，指定Timeout.Infinite表示没有超时时间
        /// </summary>
        /// <param name="match"></param>
        /// <param name="rules"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public int ReadMatches(string match, MatchingRules rules, int timeout)
        {
            string line;

            List<string> matches = new List<string>() { match };

            return this.ReadMatches(matches, rules, timeout, out line);
        }

        /// <summary>
        /// 一直读取数据，读取到match为止，超时时间使用默认超时时间
        /// </summary>
        /// <param name="match"></param>
        /// <param name="rules"></param>
        /// <returns></returns>
        public int ReadMatches(string match, MatchingRules rules)
        {
            return this.ReadMatches(match, rules, this.readTimeout);
        }

        public int SubmitLine(string line, string match1, out string matchedLine, string match2 = "")
        {
            matchedLine = string.Empty;

            logger.DebugFormat("IODriver IN = {0}", line);

            this.WriteLine(line);

            int rc = ResponseCode.SUCCESS;

            DateTime start = DateTime.Now;

            while ((DateTime.Now - start).TotalMilliseconds < this.submitTimeout)
            {
                string receivedLine = null;

                try
                {
                    receivedLine = this.ReadLine();
                }
                catch (Exception ex)
                {
                    logger.Error("读取数据异常", ex);
                    return ResponseCode.FAILED;
                }

                logger.DebugFormat("IODriver OUT = {0}", receivedLine);

                if (receivedLine.Contains(match1))
                {
                    matchedLine = receivedLine;
                    return ResponseCode.SUCCESS;
                }

                if (!string.IsNullOrEmpty(match2))
                {
                    if (receivedLine.Contains(match2))
                    {
                        matchedLine = receivedLine;
                        return ResponseCode.DUTCLI_CMD_EXEC_ERROR;
                    }
                }
            }

            return ResponseCode.DUTCLI_CMD_EXEC_TIMEOUT;
        }

        /// <summary>
        /// 发送一条数据，然后一直读取数据。读到match1判断为执行成功，读到match2判断为执行失败
        /// </summary>
        /// <param name="line"></param>
        /// <param name="match1"></param>
        /// <param name="match2"></param>
        /// <returns></returns>
        public int SubmitLine(string line, string match1, string match2 = "")
        {
            string matchedLine;
            return this.SubmitLine(line, match1, out matchedLine, match2);
        }

        /// <summary>
        /// 发送一条数据，然后再读取一条数据
        /// </summary>
        /// <param name="line">要发送的数据</param>
        /// <param name="read">读取到的数据</param>
        /// <returns></returns>
        public int SubmitLine(string line, out string read)
        {
            read = null;

            try
            {
                this.WriteLine(line);

                read = this.ReadLine();

                return ResponseCode.SUCCESS;
            }
            catch (Exception ex)
            {
                logger.Error("读取数据异常", ex);
                return ResponseCode.FAILED;
            }
        }

        public int SubmitBytes(byte[] bytes, string match1, out string matchedLine, string match2 = "")
        {
            matchedLine = string.Empty;

            this.WriteBytes(bytes);

            int rc = ResponseCode.SUCCESS;

            DateTime start = DateTime.Now;

            while ((DateTime.Now - start).TotalMilliseconds < this.submitTimeout)
            {
                string receivedLine;

                try 
                {
                    receivedLine = this.ReadLine();
                }
                catch (Exception ex)
                {
                    logger.Error("读取数据异常", ex);
                    return ResponseCode.FAILED;
                }

                logger.DebugFormat("IODriver OUT = {0}", receivedLine);

                if (receivedLine.Contains(match1))
                {
                    matchedLine = receivedLine;
                    return ResponseCode.SUCCESS;
                }

                if (!string.IsNullOrEmpty(match2))
                {
                    if (receivedLine.Contains(match2))
                    {
                        matchedLine = receivedLine;
                        return ResponseCode.DUTCLI_CMD_EXEC_ERROR;
                    }
                }
            }

            return ResponseCode.DUTCLI_CMD_EXEC_TIMEOUT;
        }

        public int SubmitBytes(byte[] bytes, string match1, string match2 = "")
        {
            string matchedLine;
            return this.SubmitBytes(bytes, match1, out matchedLine, match2);
        }

        #endregion
    }
}