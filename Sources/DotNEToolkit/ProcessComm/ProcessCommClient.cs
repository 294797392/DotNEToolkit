using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.ProcessComm
{
    public abstract class ProcessCommClient: ProcessCommObject
    {
        /// <summary>
        /// 当客户端的连接状态改变的时候触发
        /// </summary>
        public event Action<ProcessCommClient, CommClientStates> StatusChanged;

        /// <summary>
        /// 客户端的当前状态
        /// </summary>
        public CommClientStates Status { get; internal set; }

        /// <summary>
        /// 要连接的服务端的地址
        /// </summary>
        public string ServiceURI { get; set; }

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
        /// 连接IPC进程
        /// </summary>
        /// <returns></returns>
        public abstract int Connect();

        /// <summary>
        /// 与IPC进程断开连接
        /// </summary>
        public abstract void Disconnect();

        protected virtual int OnInitialize()
        {
            return DotNETCode.SUCCESS;
        }

        protected virtual void OnRelease()
        { }

        protected void NotifyStatusChanged(CommClientStates status)
        {
            this.Status = status;
            if (this.StatusChanged != null)
            {
                this.StatusChanged(this, status);
            }
        }
    }
}
