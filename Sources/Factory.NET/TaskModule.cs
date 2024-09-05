using DotNEToolkit.Modular;
using System;
using System.Collections.Generic;
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
        internal DispatcherContext Context { get; set; }

        /// <summary>
        /// 运行测试流程
        /// </summary>
        /// <returns></returns>
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
    }
}
