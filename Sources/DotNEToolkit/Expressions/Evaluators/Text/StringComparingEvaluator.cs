using FactoryIV.Base.IVTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FactoryIV.Base.Expressions.Evaluators.Text
{
    /// <summary>
    /// 判断表达式的所有参数字符串的值是否一致，大小写敏感的
    /// </summary>
    public class StringComparingEvaluator : ExpressionEvaluator
    {
        internal override int EvaluateCore(Expression expression, EvaluationContext context, out object result)
        {
            result = false;

            IVRuntimeContext runtime = context as IVRuntimeContext;

            if (expression.Parameters.Any(v => v == null))
            {
                return ResponseCode.EXPR_PARAMS_NULL_REFERENCE;
            }

            if (expression.Parameters.Any(v => string.IsNullOrEmpty(v.ToString())))
            {
                return ResponseCode.EXPR_PARAMS_NULL_REFERENCE;
            }

            string first = expression.Parameters[0].ToString();

            this.PubMessage("第1个参数 = {0}", first);

            int total = expression.Children.Count;

            for (int i = 1; i < total; i++)
            {
                string current = expression.Parameters[i].ToString();

                this.PubMessage("第{0}个参数 = {1}", i + 1, current);

                if (string.Compare(first, current, false) != 0)
                {
                    result = false;

                    return ResponseCode.SUCCESS;
                }
            }

            result = true;

            return ResponseCode.SUCCESS;
        }
    }
}