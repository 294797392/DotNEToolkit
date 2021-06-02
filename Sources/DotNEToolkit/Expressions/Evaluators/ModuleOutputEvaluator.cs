using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FactoryIV.Base.Expressions.Evaluators
{
    public class ModuleOutputEvaluator : ExpressionEvaluator
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger("ModuleOutputEvaluator");

        public override ExpressionCategories Category { get { return ExpressionCategories.Module; } }

        public override object Evaluate(Expression expression, EvaluationContext context)
        {
            TaskRuntime runtime = context as TaskRuntime;

            int count = expression.Parameters.Count;
            if (count == 2)
            {
                // 两个输入参数
                string moduleID = expression.Parameters[0].ToString();
                string key = expression.Parameters[1].ToString();

                IDictionary output;
                if (!runtime.TaskOutputs.TryGetValue(moduleID, out output))
                {
                    logger.ErrorFormat("没有找到模块{0}的输出参数");
                    return null;
                }

                if (!output.Contains(key))
                {
                    logger.ErrorFormat("模块{0}的输出不存在参数:{1}", moduleID, key);
                    return null;
                }

                return output[key];
            }

            return null;
        }
    }
}