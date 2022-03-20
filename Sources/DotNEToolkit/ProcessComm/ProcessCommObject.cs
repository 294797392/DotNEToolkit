using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.ProcessComm
{
    /// <summary>
    /// 提供进程间通信对象的通用接口
    /// </summary>
    public abstract class ProcessCommObject
    {
        /// <summary>
        /// 当收到客户端发送过来的命令的时候触发
        /// int：命令类型
        /// object：命令参数，可能是byte数组，可能是string，也可能是一个对象
        /// </summary>
        public event Action<ProcessCommObject, int, object> DataReceived;

        /// <summary>
        /// 向进程发送一个对象
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public abstract int Send(int cmdType, object instance);

        /// <summary>
        /// 发送一条命令
        /// </summary>
        /// <param name="cmdType">命令类型</param>
        /// <param name="cmdParams">命令参数</param>
        /// <returns></returns>
        public abstract int Send(int cmdType, string cmdParams);

        /// <summary>
        /// 发送一条命令
        /// </summary>
        /// <param name="cmdType">命令类型</param>
        /// <param name="cmdParams">字节数组类型的命令参数</param>
        /// <returns></returns>
        public abstract int Send(int cmdType, byte[] cmdParams);

        protected void NotifyDataReceived(int cmdType, object cmdParam)
        {
            if (this.DataReceived != null)
            {
                this.DataReceived(this, cmdType, cmdParam);
            }
        }
    }
}
