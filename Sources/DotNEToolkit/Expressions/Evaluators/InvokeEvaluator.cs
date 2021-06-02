using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FactoryIV.Base.Expressions.Evaluators
{
    public class InvokeEvaluator : ExpressionEvaluator
    {
        public override object Evaluate(Expression expression, EvaluationContext context)
        {
            object instance = expression.Parameters[0];
            string methodName = expression.Parameters[1].ToString();
            object[] methodParams = expression.Parameters.Skip(2).ToArray();

            Type t = instance.GetType();

            var ms = t.GetMember(methodName).OfType<MethodInfo>();

            MethodInfo target = null;
            foreach (MethodInfo m in ms)
            {
                // 判断参数是否相等
                if (this.CheckParameters(methodParams, m.GetParameters()))
                {
                    target = m;
                    break;
                }
            }

            return target.Invoke(instance, methodParams);
        }

        /// <summary>
        /// 比较参数类型和数量是否相等
        /// </summary>
        /// <returns></returns>
        private bool CheckParameters(object[] input, ParameterInfo[] ps)
        {
            if (ps.Length != input.Length)
            {
                return false;
            }

            for (int i = 0; i < ps.Length; i++)
            {
                if (ps[i].ParameterType.Name != input[i].GetType().Name)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
