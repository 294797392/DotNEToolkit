﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.Communictions
{
    /// <summary>
    /// 定义通信对象的类型
    /// </summary>
    public enum CommTypes
    {
        /// <summary>
        /// 串口通信设备
        /// </summary>
        SerialPort,

        /// <summary>
        /// TCP客户端设备
        /// </summary>
        TcpClient,

        /// <summary>
        /// TCP服务
        /// </summary>
        TcpService
    }
}
