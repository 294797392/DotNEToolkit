using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.ProcessComm
{
    /// <summary>
    /// 定义通信客户端的状态
    /// </summary>
    public enum CommStates
    {
        Connecting,

        /// <summary>
        /// 连接失败
        /// </summary>
        ConnectFailed,

        Connected,

        Disconnected
    }
}