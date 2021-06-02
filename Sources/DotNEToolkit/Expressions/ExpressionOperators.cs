using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Expressions
{
    /// <summary>
    /// 表达式用到的运算符
    /// </summary>
    public enum ExpressionOperators
    {
        /// <summary>
        /// 使用单引号表示一个字符串
        /// </summary>
        StringOperator,

        /// <summary>
        /// 使用中括号加索引的形式获取数组里的某个值
        /// </summary>
        ArrayElement
    }
}
