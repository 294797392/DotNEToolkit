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
        /// 异常
        /// </summary>
        public const int UNKNOWN_EXCEPTION = -2;

        /// <summary>
        /// 失败
        /// </summary>
        public const int FAILED = -1;

        /// <summary>
        /// 成功
        /// </summary>
        public const int SUCCESS = 0;

        /// <summary>
        /// 加载配置文件失败
        /// </summary>
        public const int LOAD_CONFIG_FAILED = 2;

        /// <summary>
        /// 无效的参数
        /// </summary>
        public const int INVALID_PARAMS = 3;

        /// <summary>
        /// 不支持的操作
        /// </summary>
        public const int NOT_SUPPORTED = 4;

        /// <summary>
        /// 文件不存在
        /// </summary>
        public const int FILE_NOT_FOUND = 5;

        /// <summary>
        /// 读数据失败
        /// </summary>
        public const int READ_FAILED = 6;

        /// <summary>
        /// 写数据失败
        /// </summary>
        public const int WRITE_FAILED = 7;

        /// <summary>
        /// 操作超时
        /// </summary>
        public const int TIMEOUT = 8;

        /// <summary>
        /// 打开文件失败
        /// </summary>
        public const int OPEN_FILE_FAILED = 9;

        /// <summary>
        /// 调用API失败
        /// </summary>
        public const int SYS_ERROR = 10;

        /// <summary>
        /// 解析配置文件失败
        /// </summary>
        public const int PARSE_CONFIG_FAILED = 11;

        #region 100 - 200 ModuleFactory

        public const int MODULE_NOT_FOUND = 100;

        /// <summary>
        /// 模块与模块之间存在循环依赖关系
        /// </summary>
        public const int MODULE_CIRCULAR_REFERENCE = 101;

        #endregion

        #region 201 - 300 JSON

        /// <summary>
        /// 无效的JSON格式
        /// </summary>
        public const int JSON_INVALID_FORMAT = 201;

        #endregion

        #region 301 - 400 File

        /// <summary>
        /// 写文件失败
        /// </summary>
        public const int FILE_WRITE_FAILED = 301;

        /// <summary>
        /// 没有权限
        /// </summary>
        public const int FILE_PERMISSION_ERROR = 302;

        #endregion

        #region 501 - 600 MySQLInstaller

        public const int MYSQL_INSTALL_SVC_FAILED = 501;
        public const int MYSQL_INITIALIZE_FAILED = 502;

        #endregion

        #region 601 - 650 IPC

        public const int IPC_CONNECT_FAILED = 601;

        /// <summary>
        /// 发送消息异常
        /// </summary>
        public const int IPC_SEND_FAILED = 602;

        public const int IPC_START_FAILED = 603;

        /// <summary>
        /// 创建进程失败
        /// </summary>
        public const int CREATE_PROC_FAILED = 604;

        /// <summary>
        /// 创建IHostedModule失败
        /// </summary>
        public const int CREATE_HOSTED_MODULE_FAILED = 605;

        /// <summary>
        /// 初始化IHostedModule失败
        /// </summary>
        public const int INIT_HOSTED_MODULE_FAILED = 606;

        #endregion

        public static string GetMessage(int code)
        {
            switch (code)
            {
                case DotNETCode.SUCCESS: return "成功";
                case DotNETCode.FAILED: return "失败";
                case DotNETCode.LOAD_CONFIG_FAILED: return "加载配置失败";
                case DotNETCode.INVALID_PARAMS: return "无效的参数";
                case DotNETCode.NOT_SUPPORTED: return "不支持的操作";
                case DotNETCode.FILE_NOT_FOUND: return "文件不存在";
                case DotNETCode.READ_FAILED: return "读取数据失败";
                case DotNETCode.WRITE_FAILED: return "写数据失败";
                case DotNETCode.TIMEOUT: return "操作超时";
                case DotNETCode.PARSE_CONFIG_FAILED: return "解析配置文件失败";

                case DotNETCode.MYSQL_INSTALL_SVC_FAILED: return "安装Mysql服务失败";
                case DotNETCode.MYSQL_INITIALIZE_FAILED: return "初始化Mysql数据库失败";

                case DotNETCode.IPC_SEND_FAILED: return "发送消息失败";

                default:
                    return "未知错误码";
            }
        }
    }
}