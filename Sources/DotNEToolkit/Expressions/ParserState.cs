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
        /// 字符串参数结束状态
        /// </summary>
        ParamTermination,

        /// <summary>
        /// 成员访问状态入口，当出现了'.'符号的时候就会进入这个状态
        /// 对象的成员有如下机种状态：
        /// 1. 属性，以逗号结尾
        /// 2. 函数，以左括号开头，右括号结尾
        /// 3. 数组，以左中括号开头，右中括号结尾
        /// </summary>
        AccessMemberEntry,

        ///// <summary>
        ///// 访问属性
        ///// </summary>
        //AccessProperty

        /// <summary>
        /// 语法错误状态
        /// </summary>
        GrammarError
    }
}
