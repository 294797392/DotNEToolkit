using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.ProcessComm
{
    /// <summary>
    /// 定义进程间通信的方式
    /// </summary>
    public enum ProcessCommTypes
    {
        /// <summary>
        /// WCF NetNamedPipe绑定
        /// </summary>
        WCFNamedPipe,

        /// <summary>
        /// 使用共享内存实现的IPC通信
        /// </summary>
        SharedMemory,

        /// <summary>
        /// 使用TCP实现的IPC通信
        /// </summary>
        TCP
    }
}
