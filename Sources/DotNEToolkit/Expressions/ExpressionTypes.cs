using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Expressions
{
    /// <summary>
    /// 表达式类型
    /// </summary>
    public enum ExpressionTypes
    {
        None,

        /// <summary>
        /// 宏定义
        /// </summary>
        MacroDefinition,

        /// <summary>
        /// 字符串常量
        /// </summary>
        StringConstant,

        /// <summary>
        /// 函数表达式
        /// </summary>
        Function,

        /// <summary>
        /// 表达式是一个属性访问符
        /// </summary>
        Property
    }
}
