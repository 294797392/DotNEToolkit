using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace DotNEToolkit.Modular
{
    /// <summary>
    /// 服务是一个一直在后台默默运行的模块
    /// </summary>
    public abstract class ServiceModule : ModuleBase
    {
        /// <summary>
        /// 开始运行服务
        /// </summary>
        /// <returns></returns>
        public int Start()
        {
            return this.OnStart();
        }

        /// <summary>
        /// 停止运行服务
        /// </summary>
        public void Stop()
        {
            this.OnStop();
        }

        /// <summary>
        /// 在服务开始的时候被调用
        /// </summary>
        /// <returns></returns>
        protected abstract int OnStart();

        /// <summary>
        /// 在服务结束的时候被调用
        /// </summary>
        protected abstract void OnStop();
    }
}