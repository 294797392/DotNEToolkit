using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FactoryIV.Base.Expressions.Evaluators
{
    /// <summary>
    /// 给没有冒号的MAC地址加上MAC地址
    /// </summary>
    public class MACEvaluator : ExpressionEvaluator
    {
        public override object Evaluate(Expression expression, EvaluationContext context)
        {
            //return expression.Parameters[0].ToString().InsertColon();
            throw new NotImplementedException();
        }
    }
}