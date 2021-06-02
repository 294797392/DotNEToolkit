using DotNEToolkit.Extentions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DotNEToolkit
{
    /// <summary>
    /// 指定如何处理INI文件里的空白字符
    /// </summary>
    public enum BlankCharacterOptions
    {
        /// <summary>
        /// 遇到空白字符不做处理
        /// </summary>
        None,

        /// <summary>
        /// 删除两端的空白字符
        /// </summary>
        Delete,

        /// <summary>
        /// 只删除开头的空白字符
        /// </summary>
        OnlyDeleteFirst,

        /// <summary>
        /// 只删除结尾的空白字符
        /// </summary>
        OnlyDeleteLast,

        /// <summary>
        /// 删除所有空白字符
        /// </summary>
        DeleteAll
    }

    /// <summary>
    /// INI文件读写器
    /// </summary>
    public class INIReader
    {
        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("INIReader");

        private static char[] Splitter = { '=' };

        #endregion

        #region 实例变量

        /// <summary>
        /// section -> values
        /// </summary>
        private Dictionary<string, Dictionary<string, string>> values;

        #endregion

        #region 构造方法

        private INIReader()
        {
            this.values = new Dictionary<string, Dictionary<string, string>>();
        }

        #endregion

        #region 类方法

        public static INIReader Open(string path, BlankCharacterOptions options = BlankCharacterOptions.OnlyDeleteLast)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            var values = InitializeFile(path, options);
            if (values == null)
            {
                return null;
            }

            return new INIReader()
            {
                values = values
            };
        }

        #endregion

        #region 公开接口

        public string Read(string section, string key, string defaultValue)
        {
            Dictionary<string, string> kvs;
            if (!this.values.TryGetValue(section, out kvs))
            {
                return defaultValue;
            }

            string value;
            if (!kvs.TryGetValue(key, out value))
            {
                return defaultValue;
            }

            return value;
        }

        public int ReadInt(string section, string key, int defaultValue)
        {
            return Convert.ToInt32(this.Read(section, key, defaultValue.ToString()));
        }

        public double ReadDouble(string section, string key, double defaultValue)
        {
            return Convert.ToDouble(this.Read(section, key, defaultValue.ToString()));
        }

        #endregion

        #region 实例方法

        private static string ProcessValue(string value, BlankCharacterOptions options)
        {
            string result = value;

            switch (options)
            {
                case BlankCharacterOptions.None:
                    {
                        break;
                    }

                case BlankCharacterOptions.Delete:
                    {
                        result = value.Trim(null);
                        break;
                    }

                case BlankCharacterOptions.OnlyDeleteFirst:
                    {
                        result = value.TrimStart(null);
                        break;
                    }

                case BlankCharacterOptions.OnlyDeleteLast:
                    {
                        result = value.TrimEnd(null);
                        break;
                    }

                case BlankCharacterOptions.DeleteAll:
                    {
                        result = value.Replace(" ", string.Empty);
                        break;
                    }
            }

            return result;
        }

        private static Dictionary<string, Dictionary<string, string>> InitializeFile(string path, BlankCharacterOptions options)
        {
            FileStream stream = null;
            StreamReader reader = null;

            try
            {
                stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None);
                reader = new StreamReader(stream);

                Dictionary<string, Dictionary<string, string>> sections = new Dictionary<string, Dictionary<string, string>>();

                Dictionary<string, string> kvs = null;

                while (true)
                {
                    string line = reader.ReadLine();

                    if (line == null)
                    {
                        break;
                    }

                    if (line == string.Empty)
                    {
                        continue;
                    }

                    if (line.StartsWith("[") && line.EndsWith("]"))
                    {
                        // 是一个section
                        kvs = new Dictionary<string, string>();
                        string section = line.Substring(1, line.Length - 2);
                        sections[section] = kvs;
                    }
                    else
                    {
                        string[] items = line.Split(Splitter, StringSplitOptions.RemoveEmptyEntries);
                        if (items.Length == 0)
                        {
                            // 格式不正确
                            continue;
                        }

                        if (items.Length > 2)
                        {
                            // 有多个等于号，合并value
                            string key = items[0];
                            int separatorIndex = line.IndexOf('=');
                            string value = line.Substring(separatorIndex + 1);
                            kvs[key] = ProcessValue(value, options);
                            continue;
                        }

                        if (items.Length == 2)
                        {
                            string key = items[0];
                            string value = items[1];
                            kvs[key] = ProcessValue(value, options);
                            continue;
                        }
                    }
                }

                return sections;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("打开INI文件异常, {0}, {1}", path, ex);
                return null;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                }

                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }

        #endregion
    }
}
