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
    public static class DotNETCode
    {
        /// <summary>
        /// 成功
        /// </summary>
        public const int Success = 0;

        /// <summary>
        /// 异常
        /// </summary>
        public const int Exception = -1;

        /// <summary>
        /// 加载配置文件失败
        /// </summary>
        public const int LoadConfigFailed = 2;

        /// <summary>
        /// 无效的参数
        /// </summary>
        public const int InvalidParams = 3;

        /// <summary>
        /// 不支持的操作
        /// </summary>
        public const int NotSupported = 4;

        /// <summary>
        /// 文件不存在
        /// </summary>
        public const int FileNotFound = 5;









        #region 100 - 200 ModuleFactory

        public const int ModuleNotFound = 100;

        /// <summary>
        /// 加载模块失败
        /// </summary>
        public const int CreateModuleFailed = 101;

        #endregion

        public static string GetMessage(int code)
        {
            return string.Empty;
        }
    }
}