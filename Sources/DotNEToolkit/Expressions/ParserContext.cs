using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Expressions
{
    internal class ParserContext
    {
        /// <summary>
        /// string: 表达式，形如parameter('a','b')
        /// int: start index
        /// int: end index
        /// object: userdata
        /// </summary>
        public event Action<string, int, int, ExpressionTypes, object> ExpressionReceived;

        /// <summary>
        /// 当前正在解析的表达式
        /// </summary>
        public string Expression { get; private set; }

        /// <summary>
        /// 当前解析的字符索引
        /// </summary>
        public int CurrentIndex { get; set; }

        /// <summary>
        /// 当前解析的字符
        /// </summary>
        public char CurrentChar { get; set; }

        /// <summary>
        /// 剩余的字符数
        /// </summary>
        public int RemainChar { get; set; }

        public object UserData { get; set; }

        public ParserContext(string exp)
        {
            this.Expression = exp;
        }

        public void RaiseExpressionReceived(string exp, int startIndex, int endIndex, ExpressionTypes type)
        {
            if (this.ExpressionReceived != null)
            {
                this.ExpressionReceived(exp, startIndex, endIndex, type, this.UserData);
            }
        }
    }
}
