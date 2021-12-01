using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Expressions.Evaluators
{
    /// <summary>
    /// 表达式求值程序
    /// 当你需要实现一个表达式的时候，只要继承并实现这个类就可以了
    /// </summary>
    public abstract class ExpressionEvaluator
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger("ExpressionEvaluator");

        /// <summary>
        /// 最少要多少个参数
        /// </summary>
        internal int MinimalParameters { get; set; }

        /// <summary>
        /// 计算表达式
        /// 这个函数会对表达式进行一些计算前的校验操作，比如参数是不是合法..etc..
        /// 真正计算表达式的逻辑在EvaluateCore函数里
        /// 子类必须实现EvaluateCore函数
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="context"></param>
        /// <returns>
        /// 返回null则表示失败，否则成功
        /// </returns>
        public object Evaluate(Expression expression, IEvaluationContext context)
        {
            if (this.MinimalParameters > 0)
            {
                // 检查参数个数是否匹配
                if (expression.Parameters.Count != this.MinimalParameters)
                {
                    logger.ErrorFormat("表达式参数个数不匹配, 最小个数:{0}, 实际个数:{1}", this.MinimalParameters, expression.Parameters.Count);
                    return null;
                }

                if (expression.Parameters.Exists(v => v == null))
                {
                    logger.ErrorFormat("表达式参数出现空引用");
                    return null;
                }
            }

            // 开始真正计算表达式
            return this.EvaluateCore(expression, context);
        }

        /// <summary>
        /// 真正的计算表达式的逻辑
        /// 子类必须实现该类
        /// </summary>
        /// <returns></returns>
        protected abstract object EvaluateCore(Expression expression, IEvaluationContext context);
    }
}
