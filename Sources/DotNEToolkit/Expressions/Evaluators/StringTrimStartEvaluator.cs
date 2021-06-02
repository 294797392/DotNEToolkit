using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FactoryIV.Base.Expressions.Evaluators
{
    public class StringTrimStartEvaluator : ExpressionEvaluator
    {
        public override object Evaluate(Expression expression, EvaluationContext context)
        {
            TaskRuntime runtime = context as TaskRuntime;
            string source = expression.Parameters[0].ToString();
            string trim_string = expression.Parameters[1].ToString();
            return source.TrimStart(trim_string.ToArray());
        }
    }
}
