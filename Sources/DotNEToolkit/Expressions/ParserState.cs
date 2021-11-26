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
        Ground,

        /// <summary>
        /// 当前状态是函数参数状态
        /// </summary>
        ParamFunction,

        /// <summary>
        /// 当前是字符串类型的参数
        /// </summary>
        ParamString,

        /// <summary>
        /// 参数结束的状态
        /// 用该状态判断是进入函数模式继续解析还是进入Ground模式
        /// </summary>
        ParamEnd
    }
}
