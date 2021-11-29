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
        /// 外部模块用来判断表达式是否执行成功的标志
        /// 如果执行不成功，那么立即停止表达式求值
        /// 如果执行成功，那么继续对子表达式或者父表达式进行求值
        /// </summary>
        internal bool Success { get; set; }

        /// <summary>
        /// 计算表达式
        /// 这个函数会对表达式进行一些计算前的校验操作，比如参数是不是合法..etc..
        /// 真正计算表达式的逻辑在EvaluateCore函数里
        /// 子类必须实现EvaluateCore函数
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="context"></param>
        /// <param name="result">表达式计算后的结果</param>
        /// <returns>计算表达式是否成功</returns>
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

                //// 有可能某个表达式计算出来的参数值就是NULL，而并不是表达式计算错误导致的
                //// 所以这里不检查空值，由外部通过Success属性进行判断表达式是否计算成功
                //if (expression.Parameters.Exists(v => v == null))
                //{
                //    logger.ErrorFormat("表达式参数出现空引用");
                //    return null;
                //}
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
