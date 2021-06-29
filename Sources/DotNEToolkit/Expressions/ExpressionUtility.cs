using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Expressions
{
    public static class ExpressionUtility
    {
        public static List<string> Find(string text)
        {
            List<string> expressions = new List<string>();
            return expressions;
        }

        /// <summary>
        /// 对source里的所有的键值对进行表达式计算操作，并把计算后的值存储到output集合里
        /// </summary>
        /// <param name="source">带有表达式的参数</param>
        /// <param name="output">存储解析后的参数</param>
        /// <param name="context">解析表达式需要的上下文信息</param>
        public static int EvaluateExpressions(IDictionary source, EvaluationContext context, out IDictionary output)
        {
            string json = JsonConvert.SerializeObject(source);
            output = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

            IEnumerator enumerator = source.Keys.GetEnumerator();

            while (enumerator.MoveNext())
            {
                object key = enumerator.Current;
                object value = source[key];

                // 这里只处理字符串类型的参数
                if (value is string)
                {
                    string value_string = value.ToString();

                    if (ExpressionBuilder.Instance.IsExpression(value_string))
                    {
                        object result;
                        int code = ExpressionBuilder.Instance.Evaluate(value_string, context, out result);
                        if (code != DotNETCode.SUCCESS)
                        {
                            return code;
                        }

                        output[key] = result;
                    }
                }
                else
                {

                }
            }

            return DotNETCode.SUCCESS;
        }
    }
}
