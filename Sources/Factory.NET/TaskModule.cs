﻿using DotNEToolkit;
using DotNEToolkit.Media.Audio;
using DotNEToolkit.Modular;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace Factory.NET
{
    public class DemoTask : TaskModule
    {
        protected override int OnInitialize()
        {
            return ResponseCode.SUCCESS;
        }

        protected override void OnRelease()
        {
        }

        public override int Run()
        {
            return ResponseCode.SUCCESS;
        }
    }

    /// <summary>
    /// 表示一个测试流程
    /// </summary>
    public abstract class TaskModule : ModuleBase
    {
        public DispatcherContext Context { get; internal set; }

        /// <summary>
        /// 当测试流程运行错误的时候，保存错误消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 运行测试流程
        /// </summary>
        /// <returns>测试流程运行之后的状态</returns>
        public abstract int Run();

        /// <summary>
        /// 测试结束之后会调用
        /// </summary>
        public virtual void Reset()
        { }

        /// <summary>
        /// 获取所有测试流程共享的数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T GetGlobalParameter<T>(string key)
        {
            return base.GetParameter<T>(this.Context.GloablParameters, key);
        }

        /// <summary>
        /// 查询到目前为止已经运行结束的测试项的测试结果
        /// </summary>
        /// <returns></returns>
        public List<TaskResult> GetTaskResults()
        {
            return this.Context.TaskResults;
        }
    }
}
