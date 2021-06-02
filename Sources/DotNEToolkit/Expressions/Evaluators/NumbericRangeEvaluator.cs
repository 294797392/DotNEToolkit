using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FactoryIV.Base.Expressions.Evaluators
{
    public class NumbericRangeEvaluator : ExpressionEvaluator
    {
        public override object Evaluate(Expression expression, EvaluationContext context)
        {
            double value = double.Parse(expression.Parameters[0].ToString());
            double min = double.Parse(expression.Parameters[1].ToString());
            double max = double.Parse(expression.Parameters[2].ToString());
            return value >= min && value <= max;
        }
    }
}