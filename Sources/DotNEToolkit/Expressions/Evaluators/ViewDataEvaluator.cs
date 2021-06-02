using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FactoryIV.Base.Expressions.Evaluators
{
    public class ViewDataEvaluator : ExpressionEvaluator
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger("ViewDataEvaluator");

        public override ExpressionCategories Category { get { return ExpressionCategories.View; } }

        public override object Evaluate(Expression expression, EvaluationContext context)
        {
            TaskRuntime runtime = context as TaskRuntime;
            object key = expression.Parameters[0];
            if (!runtime.ViewData.Contains(key))
            {
                logger.ErrorFormat("不存在ViewData:{0}", key);
                return null;
            }

            return runtime.ViewData[key];
        }
    }
}
