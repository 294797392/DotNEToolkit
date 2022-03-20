using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.ProcessComm
{
    public abstract class ProcessCommSvc : ProcessCommObject
    {
        /// <summary>
        /// 该服务端的地址
        /// </summary>
        public string URI { get; set; }

        /// <summary>
        /// 初始化IPC
        /// </summary>
        /// <returns></returns>
        public int Initialize()
        {
            return this.OnInitialize();
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Release()
        {
            this.OnRelease();
        }

        /// <summary>
        /// 运行IPC服务
        /// </summary>
        /// <returns></returns>
        public abstract int Start();

        /// <summary>
        /// 停止运行IPC服务
        /// </summary>
        public abstract void Stop();

        protected abstract int OnInitialize();

        protected abstract void OnRelease();
    }
}
