using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.ProcessComm
{
    public abstract class ProcessCommClient: ProcessCommObject
    {
        /// <summary>
        /// 进程间通信对象的状态改变的时候触发
        /// </summary>
        public event Action<ProcessCommClient, CommStates> StatusChanged;

        /// <summary>
        /// 客户端的当前状态
        /// </summary>
        public CommStates Status { get; internal set; }

        /// <summary>
        /// 初始化IPC
        /// </summary>
        /// <returns></returns>
        public int Initialize(string uri)
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
        public abstract int Connect(string remoteUri);

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

        protected void NotifyStatusChanged(CommStates status)
        {
            this.Status = status;
            if (this.StatusChanged != null)
            {
                this.StatusChanged(this, status);
            }
        }
    }
}
