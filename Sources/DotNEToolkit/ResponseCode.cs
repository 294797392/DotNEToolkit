using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit
{
    /// <summary>
    /// DotNEToolkit的返回值
    /// </summary>
    public static class ResponseCode
    {
        /// <summary>
        /// 成功
        /// </summary>
        public const int SUCCESS = 1;

        /// <summary>
        /// 异常
        /// </summary>
        public const int EXCEPTION = -1;

        /// <summary>
        /// 加载配置文件失败
        /// </summary>
        public const int LoadConfigFailed = 2;
    }
}