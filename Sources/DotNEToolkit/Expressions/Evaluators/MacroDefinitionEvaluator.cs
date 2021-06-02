using FactoryIV.Base.IVTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FactoryIV.Base.Expressions.Evaluators
{
    /// <summary>
    /// 计算宏定义表达式
    /// </summary>
    public class MacroDefinitionEvaluator : ExpressionEvaluator
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger("MacroDefinitionEvaluator");

        internal override int EvaluateCore(Expression expression, EvaluationContext context, out object result)
        {
            result = null;

            IVRuntimeContext runtime = context as IVRuntimeContext;

            // 表达式是${XXX}类型的
            if (expression.Parameters.Count == 0)
            {
                if (!runtime.MacroDefinitions.TryGetValue(expression.ExpressionText, out result))
                {
                    logger.ErrorFormat("不存在宏定义:{0}", expression.ExpressionText);
                    return ResponseCode.MACRO_NOT_FOUND;
                }
                else
                {
                    return ResponseCode.SUCCESS;
                }
            }

            // 表达式是$macro('XXX')类型的
            string macroName = expression.Parameters[0].ToString();
            if (string.IsNullOrEmpty(macroName))
            {
                return ResponseCode.EXPR_PARAMS_ILLEGAL;
            }

            if (!runtime.MacroDefinitions.TryGetValue(macroName, out result))
            {
                logger.ErrorFormat("不存在宏定义:{0}", macroName);
                return ResponseCode.MACRO_NOT_FOUND;
            }

            return ResponseCode.SUCCESS;
        }
    }
}
