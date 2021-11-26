using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit
{
    /// <summary>
    /// 提供颜色转换函数
    /// </summary>
    public static class Colors
    {
        /// <summary>
        /// 十六进制的颜色转rgb
        /// 支持以井号开头或者不以井号开头的十六进制颜色值
        /// </summary>
        /// <param name="color"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name=""></param>
        public static void ConvertRGB(string color, out byte r, out byte g, out byte b)
        {
            bool startWithPoundKey = color.StartsWith("#");

            if (color.Length != 6 || (startWithPoundKey && color.Length != 7))
            {
                throw new ArgumentException(string.Format("十六进制颜色格式不正确, {0}", color));
            }

            int start = startWithPoundKey ? 1 : 0;

            if (!Numberics.IsHexadecimal(color[start + 0]) || !Numberics.IsHexadecimal(color[start + 1]) || !Numberics.IsHexadecimal(color[start + 2]) ||
                !Numberics.IsHexadecimal(color[start + 3]) || !Numberics.IsHexadecimal(color[start + 4]) || Numberics.IsHexadecimal(color[start + 5]))
            {
                throw new ArgumentException(string.Format("十六进制颜色格式不正确, {0}", color));
            }

            string valueR = color.Substring(start, 2);
            string valueG = color.Substring(start + 2, 2);
            string valueB = color.Substring(start + 4, 2);

            r = byte.Parse(valueR);
            g = byte.Parse(valueG);
            b = byte.Parse(valueB);
        }
    }
}
