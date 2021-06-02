using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Expressions
{
    public enum ExpressionState
    {
        /// <summary>
        /// 已经求过值了
        /// </summary>
        HasEvaluation,

        /// <summary>
        /// 等待求值
        /// </summary>
        WaitEvaluation
    }
}
