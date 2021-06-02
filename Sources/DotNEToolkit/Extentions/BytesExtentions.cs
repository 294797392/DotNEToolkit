using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Extentions
{
    public enum HexNumberOptions
    {
        /// <summary>
        /// 没有选项
        /// </summary>
        None,

        /// <summary>
        /// 带有0x前缀
        /// </summary>
        WithPrefix
    }

    public static class BytesExtentions
    {
        /// <summary>
        /// 把一个字节数组转成十六进制字符串
        /// 不带0x前缀
        /// </summary>
        /// <param name="source"></param>
        /// <param name="separator">每个字节之间的分隔符</param>
        /// <returns></returns>
        public static string JoinHexNumber(this byte[] source, string separator = null, HexNumberOptions options = HexNumberOptions.None)
        {
            if (source.Length == 0)
            {
                return string.Empty;
            }

            List<string> bytes = new List<string>();

            foreach (byte value in source)
            {
                string v = string.Empty;

                switch (options)
                {
                    case HexNumberOptions.None:
                        {
                            v = string.Format("{0:X2}", value);
                            break;
                        }

                    case HexNumberOptions.WithPrefix:
                        {
                            v = string.Format("0x{0:X2}", value);
                            break;
                        }
                }

                bytes.Add(v);
            }

            return bytes.Join(separator);
        }

        /// <summary>
        /// 把一个十六进制的字符串分隔成byte数组
        /// </summary>
        /// <param name="hex">要分隔的字符串</param>
        /// <param name="separator">分隔符</param>
        /// <returns></returns>
        public static byte[] SplitBytes(string hex, string separator)
        {
            string[] splitters = new string[1] { separator };

            string[] values = hex.Split(splitters, StringSplitOptions.RemoveEmptyEntries);

            return values.Select(v => byte.Parse(v, System.Globalization.NumberStyles.HexNumber)).ToArray();
        }
    }
}
