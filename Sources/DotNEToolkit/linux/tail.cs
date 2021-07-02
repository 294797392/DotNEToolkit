using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace DotNEToolkit.linux
{
    /// <summary>
    /// 模拟linux的tail工具
    /// </summary>
    public class tail
    {
        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("tail");

        /// <summary>
        /// 如果打开文件失败，那么这个值指定重试打开文件的间隔时间
        /// 打开文件成功后，这个值指定每次读取文件的间隔时间
        /// 单位是毫秒
        /// </summary>
        private const int sleep_period = 1000;

        /// <summary>
        /// 每次读取的缓冲区大小
        /// </summary>
        private const int BUFSIZE = 512;

        /// <summary>
        /// 默认的换行符
        /// </summary>
        private const string NEW_LINE = "\r\n";

        #endregion

        #region 枚举

        /// <summary>
        /// tail工具的事件
        /// </summary>
        public enum tail_event_type
        {
            TAIL_DATA,

            /// <summary>
            /// 退出
            /// </summary>
            TAIL_EXIT
        }

        /// <summary>
        /// tail选项
        /// </summary>
        public enum tail_options
        {
            /// <summary>
            /// same as 'tail -f'
            /// </summary>
            follow = 1,

            /// <summary>
            /// 按行读取，换行符是\n
            /// </summary>
            readline = 2,

            /// <summary>
            /// 如果文件不存在，是否一直重试读取文件
            /// </summary>
            retry = 32768
        }

        #endregion

        #region 公开事件

        /// <summary>
        /// 第一个参数是tail对象
        /// 第二个参数是事件类型
        /// 第三个参数是事件类型所关联的数据
        /// 第四个参数是userData
        /// </summary>
        public event Action<tail, tail_event_type, object, object> callback;

        #endregion

        #region 实例变量

        private char[] remain;

        private string[] new_line_splitter;

        private bool isRunning;

        private object userData;

        #endregion

        #region 属性

        private StreamReader reader { get; set; }

        private FileStream stream { get; set; }

        public tail_options options { get; private set; }

        /// <summary>
        /// 读取文件的间隔时间
        /// </summary>
        public int period { get; set; }

        /// <summary>
        /// 每次读取文件的缓冲区大小
        /// </summary>
        public int bufsize { get; set; }

        #endregion

        #region 构造方法

        public tail()
        {
            this.period = sleep_period;
            this.new_line_splitter = new string[] { NEW_LINE };
            this.options |= tail_options.follow;
            this.options |= tail_options.readline;
            this.options |= tail_options.retry;
            this.bufsize = BUFSIZE;
        }

        #endregion

        #region 公开接口

        public void start(string path)
        {
            this.isRunning = true;
            this.tail_async(path);
        }

        public void start(string path, object userData)
        {
            this.userData = userData;
            this.start(path);
        }

        public void stop()
        {
            
        }

        public void addopt(tail_options opt)
        {
            this.options |= opt;
        }

        #endregion

        #region 实例方法

        private FileStream tail_open(string path, out StreamReader reader)
        {
            reader = null;

            try
            {
                FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);

                // 默认从文件结尾处开始读取
                stream.Seek(0, SeekOrigin.End);

                reader = new StreamReader(stream);

                return stream;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("打开文件异常, {0}, {1}", path, ex.Message);
                return null;
            }
        }

        private int tail_read(int len, char[] chars)
        {
            // 已经读取了的数据大小
            int readed = 0;

            // 剩余要读取的数据大小
            int left = len;

            while (readed != len)
            {
                int read = this.reader.ReadBlock(chars, readed, left);
                if (read == 0)
                {
                    // 没有数据了
                    return readed;
                }
                else if (read > 0)
                {
                    readed += read;
                    left -= read;
                }
                else if (read < 0)
                {
                    // 读取失败?
                    if (readed > 0)
                    {
                        // 已经读取了一些了，返回读取的大小
                        return readed;
                    }
                    return read;
                }
            }

            return readed;
        }

        private void notify_callback(tail_event_type tevent, object data)
        {
            if (this.callback != null)
            {
                this.callback(this, tevent, data, this.userData);
            }
        }

        private void close()
        {
            if (this.stream != null)
            {
                this.stream.Close();
                this.stream.Dispose();
            }

            if (this.reader != null)
            {
                this.reader.Close();
                this.reader.Dispose();
            }

            this.remain = null;
        }

        /// <summary>
        /// 如果不是完整的一行数据，那么把不完整的数据截断并保存到remain里
        /// </summary>
        /// <param name="chars">返回不完整的行的数据</param>
        private char[] trim_line(char[] chars)
        {
            List<char> remains = new List<char>();

            int len = chars.Length - 1;
            for (int i = len; i >= 0; i--)
            {
                if (chars[i] == '\n')
                {
                    break;
                }

                remains.Add(chars[i]);
                chars[i] = '\0';
            }

            remains.Reverse();
            return remains.ToArray();
        }

        private void tail_async(string path)
        {
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                bool follow = this.options.HasFlag(tail_options.follow);
                bool retry = this.options.HasFlag(tail_options.retry);
                bool readline = this.options.HasFlag(tail_options.readline);

                while (this.isRunning)
                {
                    FileStream fs = null;           // 文件流
                    StreamReader reader = null;     // 文件读取器
                    long timestamp = 0;             // 文件创建的时间戳

                    #region 打开文件

                    if (!File.Exists(path) || (fs = tail_open(path, out reader)) == null)
                    {
                        if (retry)
                        {
                            Thread.Sleep(sleep_period);
                            continue;
                        }
                        else
                        {
                            this.notify_callback(tail_event_type.TAIL_EXIT, null);
                            return;
                        }
                    }

                    // 使用文件的创建时间戳比较读取的文件是否有变化
                    timestamp = File.GetCreationTime(path).ToFileTime();
                    this.reader = reader;
                    this.stream = fs;

                    #endregion

                    do
                    {
                        Thread.Sleep(this.period);

                        // 判断当前是否还正在运行
                        if (!this.isRunning)
                        {
                            this.close();
                            break;
                        }

                        if (!File.Exists(path) ||                                   // 文件不存在
                            File.GetCreationTime(path).ToFileTime() != timestamp)   // 判断当前文件和已经打开的文件是否一致（如果用户删除了之前打开的文件，然后创建了一个同名的文件，那么就不一致）
                        {
                            // 跳出循环重新打开文件
                            this.close();
                            break;
                        }

                        // 从文件里读取固定大小的数据
                        char[] chars = new char[this.bufsize];
                        int readed = tail_read(this.bufsize, chars);
                        if (readed < 0)
                        {
                            // 读到的数据小于0，说明读取失败，此时跳出循环，重新打开文件
                            break;
                        }
                        else if (readed == 0)
                        {
                            // 没读到数据，此时可能数据被读完了，等待文件被写入
                            continue;
                        }
                        else
                        {
                            // 读到了数据

                            if (readline)
                            {
                                char[] buffer = null;

                                if (this.remain == null || this.remain.Length == 0)
                                {
                                    this.remain = this.trim_line(chars);
                                    buffer = chars;
                                }
                                else
                                {
                                    buffer = new char[this.remain.Length + readed];
                                    Array.Copy(this.remain, 0, buffer, 0, this.remain.Length);
                                    Array.Copy(chars, 0, buffer, this.remain.Length, readed);
                                    this.remain = this.trim_line(buffer);
                                }

                                string[] lines = string.Concat<char>(buffer).Trim('\0').Split(this.new_line_splitter, StringSplitOptions.RemoveEmptyEntries);

                                foreach (string line in lines)
                                {
                                    this.notify_callback(tail_event_type.TAIL_DATA, line);
                                }
                            }
                            else
                            {
                                this.notify_callback(tail_event_type.TAIL_DATA, string.Concat<char>(chars));
                            }
                        }
                    }
                    while (follow && this.isRunning); // 一直读取，读完了也继续读
                }
            });
        }

        #endregion
    }
}
