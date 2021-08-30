using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit
{
    /// <summary>
    /// 字节数组的帮助函数
    /// </summary>
    public static class Bytes
    {
        /// <summary>
        /// 对两个字节数组进行比较操作
        /// </summary>
        /// <param name="array1">要比较的第一个字节数组</param>
        /// <param name="array2">要比较的第二个字节数组</param>
        /// <returns>每个字节是否相等</returns>
        public static bool Compare(byte[] array1, byte[] array2)
        {
            if (array1.Length != array2.Length)
            {
                return false;
            }

            int length = array1.Length;

            for (int i = 0; i < length; i++)
            {
                if (array1[i] != array2[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}