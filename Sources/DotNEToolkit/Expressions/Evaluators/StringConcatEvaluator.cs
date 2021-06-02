using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FactoryIV.Base.Expressions.Evaluators
{
    public class StringConcatEvaluator : ExpressionEvaluator
    {
        public override ExpressionCategories Category { get { return ExpressionCategories.String; } }

        public override object Evaluate(Expression expr, EvaluationContext context)
        {
            return string.Concat(expr.Parameters);
        }
    }
}
