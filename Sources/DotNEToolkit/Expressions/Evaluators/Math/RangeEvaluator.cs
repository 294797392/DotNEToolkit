using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FactoryIV.Base.Expressions.Evaluators.Math
{
    public class RangeEvaluator : ExpressionEvaluator
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger("RangeEvaluator");

        internal override int EvaluateCore(Expression expression, EvaluationContext context, out object result)
        {
            result = null;

            string svalue = expression.Parameters[0].ToString();
            string smin = expression.Parameters[1].ToString();
            string smax = expression.Parameters[2].ToString();

            double value, min, max;

            if (!double.TryParse(svalue, out value) || !double.TryParse(smin, out min) || !double.TryParse(smax, out max))
            {
                logger.ErrorFormat("svalue = {0}, smin = {1}, smax = {2}", svalue, smin, smax);
                return ResponseCode.INVALID_PARAMETER;
            }

            logger.DebugFormat("value = {0}, min = {1}, max = {2}", value, min, max);

            result = value >= min && value <= max;
            return ResponseCode.SUCCESS;
        }
    }
}