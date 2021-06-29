using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using DotNEToolkit.Modular;
using DotNEToolkit.Modular.Attributes;
using Newtonsoft.Json;

namespace DotNEToolkit.TaskDispatchers
{
    /// <summary>
    /// 测试流程的实例
    /// 测试流程执行器
    /// </summary>
    [ModuleOutput("Status", typeof(TaskStatus))]
    public abstract class Task : ModuleBase
    {
        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("Task");

        #endregion

        #region 实例变量

        #endregion

        #region 属性

        #endregion

        #region 构造方法

        public Task()
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