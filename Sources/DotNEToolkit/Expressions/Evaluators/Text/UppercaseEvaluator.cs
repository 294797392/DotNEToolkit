using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FactoryIV.Base.Expressions.Evaluators.Text
{
    public class UppercaseEvaluator : ExpressionEvaluator
    {
        internal override int EvaluateCore(Expression expression, EvaluationContext context, out object result)
        {
            result = null;

            if (expression.Parameters[0] == null)
            {
                return ResponseCode.EXPR_PARAMS_ILLEGAL;
            }

            string value = expression.Parameters[0].ToString();

            result = value.ToUpper();

            return ResponseCode.SUCCESS;
        }
    }
}
