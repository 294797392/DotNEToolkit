using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Excels
{
    /// <summary>
    /// 指定对Excel里的空列的处理方式
    /// </summary>
    public enum ReadOptions
    {
        /// <summary>
        /// 忽略Excel里的空列
        /// </summary>
        IgnoreEmptyCell = 1,

        /// <summary>
        /// 保留Excel里的空列（会保存一个空对象在ExcelRow里）
        /// </summary>
        KeepEmptyCell = 2
    }
}
