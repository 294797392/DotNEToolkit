using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit
{
    /// <summary>
    /// 操作数字的帮助函数
    /// </summary>
    public static class Numberics
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
    }
}
