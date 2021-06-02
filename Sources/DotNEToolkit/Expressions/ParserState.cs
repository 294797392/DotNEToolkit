using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Expressions
{
    internal enum ParserState
    {
        /// <summary>
        /// 初始状态
        /// </summary>
        IDLE,

        /// <summary>
        /// 当前是解析的数据为字符串
        /// </summary>
        String,

        /// <summary>
        /// 当前解析的数据是表达式
        /// </summary>
        Expression,

        /// <summary>
        /// 当前解析的数据是宏定义
        /// </summary>
        MarcoDefinition,
    }
}
