using FactoryIV.Base.IVTasks;
using FactoryIV.Definitions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FactoryIV.Base.Expressions.Evaluators.Module
{
    public class ModuleOutputEvaluator : ExpressionEvaluator
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger("ModuleOutputEvaluator");

        internal override int EvaluateCore(Expression expression, EvaluationContext context, out object result)
        {
            result = null;

            IVRuntimeContext runtime = context as IVRuntimeContext;

            // 两个输入参数
            string moduleID = expression.Parameters[0].ToString();
            string key = expression.Parameters[1].ToString();

            IDictionary output;
            if (!runtime.TaskOutputs.TryGetValue(moduleID, out output))
            {
                logger.ErrorFormat("没有找到模块{0}的输出参数", moduleID);
                return ResponseCode.MODULE_NOT_FOUND;
            }

            if (!output.Contains(key))
            {
                logger.ErrorFormat("模块{0}的输出不存在参数:{1}", moduleID, key);
                return ResponseCode.PROPERTY_NOT_FOUND;
            }

            result = output[key];

            return ResponseCode.SUCCESS;
        }
    }
}
