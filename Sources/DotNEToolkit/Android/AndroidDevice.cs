using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Android
{
    /// <summary>
    /// 表示一个ADB设备
    /// </summary>
    public class AndroidDevice
    {
        /// <summary>
        /// 设备名字
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// USB ClassID
        /// </summary>
        public string ClassID { get; set; }
    }
}
