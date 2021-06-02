using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FactoryIV.Base.Expressions.Evaluators
{
    public class StringCompareEvaluator : ExpressionEvaluator
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger("StringCompareEvaluator");

        public override object Evaluate(Expression expression, EvaluationContext context)
        {
            if (expression.Parameters.Count <= 1)
            {
                // 最少要两个string
                logger.ErrorFormat("{0}最少需要两个string", expression);
                return false;
            }

            if (expression.Parameters.Count(p => p == null) > 0)
            {
                // 不允许有null值
                logger.ErrorFormat("{0}有空值", expression);
                return false;
            }

            object prevo = null;
            foreach (object o in expression.Parameters)
            {
                if (prevo != null)
                {
                    if (string.IsNullOrEmpty(o.ToString()) || string.IsNullOrEmpty(prevo.ToString()) ||
                        string.Compare(o.ToString(), prevo.ToString(), true) != 0)
                    {
                        logger.ErrorFormat("0 ={0}, 1={1}", o.ToString(), prevo.ToString());
                        return false;
                    }
                }

                prevo = o;
            }

            return true;
        }
    }
}
