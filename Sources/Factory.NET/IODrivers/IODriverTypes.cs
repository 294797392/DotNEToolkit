using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Factory.NET.IODrivers
{
    /// <summary>
    /// DUT连接方式
    /// </summary>
    public enum IODriverTypes
    {
        /// <summary>
        /// 串口连接
        /// </summary>
        SerialPort = 0,

        /// <summary>
        /// Adb连接
        /// </summary>
        ADB = 1,

        /// <summary>
        /// Tcp连接
        /// </summary>
        TcpClient = 2,

        /// <summary>
        /// Tcp服务器
        /// </summary>
        TcpService = 3,

        /// <summary>
        /// 控制VISA设备的连接
        /// </summary>
        VISADriver = 5,

        /// <summary>
        /// 虚拟设备
        /// 在如下情况会用到：
        /// 1. 模拟运行环境使用
        /// 2. 当DUT不需要IODevice的时候会实例化VirtualDevice类型的IODevice
        /// </summary>
        VirtualDevice = 8,
    }
}