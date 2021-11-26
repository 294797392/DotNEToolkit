using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Expressions
{
    /// <summary>
    /// 表达式树的子节点
    /// </summary>
    public class Expression
    {
        /// <summary>
        /// 父节点
        /// </summary>
        public Expression Parent { get; set; }

        /// <summary>
        /// 表达式名字
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 表达式的内容
        /// </summary>
        public string ExpressionText { get; set; }

        /// <summary>
        /// 表达式参数列表
        /// 也就是子表达式求值之后的结果
        /// </summary>
        public List<object> Parameters { get; private set; }

        /// <summary>
        /// 表达式的值
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// 子表达式
        /// </summary>
        public List<Expression> Children { get; private set; }

        /// <summary>
        /// 表达式的起始索引
        /// </summary>
        public int StartIndex { get; set; }

        /// <summary>
        /// 表达式的结束索引
        /// </summary>
        public int EndIndex { get; set; }

        /// <summary>
        /// 表达式状态
        /// </summary>
        public ExpressionState State { get; set; }

        public Expression()
        {
            this.Children = new List<Expression>();
            this.State = ExpressionState.WaitEvaluation;
            this.Parameters = new List<object>();
        }

        public override string ToString()
        {
            return string.Format("{0} = {1}", this.Name, this.Value);
        }
    }
}