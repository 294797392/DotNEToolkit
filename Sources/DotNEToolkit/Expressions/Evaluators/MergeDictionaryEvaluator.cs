using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FactoryIV.Base.Expressions.Evaluators
{
    /// <summary>
    /// 把多个字典合并成一个字典
    /// merge_dictionary(dic1, dic2, dic3)
    /// </summary>
    public class MergeDictionaryEvaluator : ExpressionEvaluator
    {
        public override object Evaluate(Expression expression, EvaluationContext context)
        {
            IDictionary result = new Dictionary<object, object>();

            foreach (object o in expression.Parameters)
            {
                IDictionary map = o as IDictionary;
                foreach (var key in map.Keys)
                {
                    result[key] = map[key];
                }
            }

            return result;
        }
    }
}
