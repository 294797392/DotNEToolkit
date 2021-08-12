using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using DotNEToolkit.Expressions;
using DotNEToolkit.Modular;
using DotNEToolkit.Modular.Attributes;
using Newtonsoft.Json;

namespace DotNEToolkit.TaskDispatchers
{
    /// <summary>
    /// 定义工作流任务的通用属性
    /// </summary>
    public static class WorkflowProperties
    {
        /// <summary>
        /// 工作流的状态
        /// </summary>
        public const string WP_STATUS = "STATUS";
    }

    /// <summary>
    /// 测试流程的实例
    /// 测试流程执行器
    /// </summary>
    [ModuleOutput(WorkflowProperties.WP_STATUS, typeof(WorkflowStatus))]
    public abstract class WorkflowTask : ModuleBase
    {
        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("Task");

        #endregion

        #region 实例变量

        #endregion

        #region 属性

        #endregion

        #region 构造方法

        public WorkflowTask()
        {
        }

        #endregion

        #region 公开接口

        /// <summary>
        /// 初始化测试流程
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [ModuleAction()]
        public override int Initialize(IDictionary parameters)
        {
            base.Initialize(parameters);
            return DotNETCode.SUCCESS;
        }

        /// <summary>
        /// 释放测试流程
        /// </summary>
        /// <returns></returns>
        [ModuleAction()]
        public override void Release()
        {
        }

        #endregion

        #region 抽象方法

        /// <summary>
        /// 开始运行测试流程
        /// </summary>
        /// <returns></returns>
        [ModuleAction()]
        public abstract int Run();

        /// <summary>
        /// 重置Task的缓存数据
        /// 当复用Task实例的时候，会首先调用Reset函数
        /// </summary>
        [ModuleAction()]
        public virtual void Reset()
        { }

        #endregion

        #region 实例方法

        #endregion
    }
}