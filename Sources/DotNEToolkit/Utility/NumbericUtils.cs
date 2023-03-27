using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Utility
{
    /// <summary>
    /// 操作数字的帮助函数
    /// </summary>
    public static class NumbericUtils
    {
        /// <summary>
        /// 保留num位小数，会四舍五入
        /// </summary>
        /// <param name="src"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        public static double FixedDecimal(double src, int num)
        {
            string format = "#0.0";
            return double.Parse(src.ToString(format.PadRight(num + 3, '0')));
        }

        /// <summary>
        /// 判断一个字符是否是十六进制字符
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        public static bool IsHexadecimal(char ch)
        {
            return (ch >= '0' && ch <= '9') ||
                (ch >= 'A' && ch <= 'Z') ||
                (ch >= 'a' && ch <= 'z');
        }

        /// <summary>
        /// 函数功能：
        /// 如果value是437，ch是2，那么返回的结果就是4372
        /// 
        /// 从terminal项目拷贝
        /// </summary>
        /// <param name="value"></param>
        /// <param name="add"></param>
        /// <returns></returns>
        public static int AccumulateTo(int value, int add)
        {
            int digit = add - '0';

            value = value * 10 + digit;

            return value;
        }

        public static int AccumulateTo(int value, int add, int max)
        {
            int result = AccumulateTo(value, add);
            return result > max ? max : result;
        }

        /// <summary>
        /// 比较两个double数值的大小
        /// </summary>
        /// <param name="d1"></param>
        /// <param name="d2"></param>
        /// <param name="tolerance">比较容差，如果容差小于该值，那么就判断为相等</param>
        /// <returns></returns>
        public static bool Max(double d1, double d2, double tolerance = 0.000001d)
        {
            double delta = Math.Abs(d1 - d2);
        }
    }
}
