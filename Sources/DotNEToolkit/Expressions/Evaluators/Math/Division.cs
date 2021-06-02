using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FactoryIV.Base.Expressions.Evaluators.Math
{
    /// <summary>
    /// 实现一个除法表达式
    /// </summary>
    public class Division : ExpressionEvaluator
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger("Division");

        internal override int EvaluateCore(Expression expression, EvaluationContext context, out object result)
        {
            result = null;

            string sdividend = expression.Parameters[0].ToString(); // 被除数，第一个数字
            string sdivisor = expression.Parameters[1].ToString(); // 除数：第二个数字

            double dividend, divisor;

            if (!double.TryParse(sdividend, out dividend))
            {
                logger.ErrorFormat("被除数不合法:{0}", dividend);
                return ResponseCode.EXPR_PARAMS_ILLEGAL;
            }

            if (!double.TryParse(sdivisor, out divisor))
            {
                logger.ErrorFormat("除数不合法:{0}", divisor);
                return ResponseCode.EXPR_PARAMS_ILLEGAL;
            }

            if (divisor == 0)
            {
                logger.ErrorFormat("除数不能为0");
                return ResponseCode.EXPR_PARAMS_ILLEGAL;
            }

            string value = (dividend / divisor).ToString();

            // C#默认会使用科学计数法表示数字。为了让用户能更直观的看到大小，这里转换一下，不使用科学计数法
            decimal dvalue;
            decimal.TryParse(value, System.Globalization.NumberStyles.Float, null, out dvalue);

            result = dvalue;
            //result = dividend / divisor;

            return ResponseCode.SUCCESS;
        }
    }
}
