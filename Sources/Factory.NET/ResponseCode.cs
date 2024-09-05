using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Factory.NET
{
    public static class ResponseCode
    {
        public const int FAILED = -1;

        public const int SUCCESS = 0;

        public const int IODRV_READ_TIMEOUT = 1;

        public const int DUTCLI_CMD_EXEC_ERROR = 2;

        public const int DUTCLI_CMD_EXEC_TIMEOUT = 3;

        public const int IODRV_OPEN_FAILED = 4;

        public const int IODRV_RECONNECT = 5;

        public const int IODRV_NOT_OPENED = 6;

        public const int UNKOWN_EXCEPTION = 7;

        public const int INVALID_PARAMETER = 8;

        public const int IODRV_WRITE_FAILED = 9;

        public const int IODRV_READ_FAILED = 10;

        public static string GetMessage(int code)
        {
            switch (code)
            {
                case FAILED: return "失败";
                case SUCCESS: return "成功";
                case IODRV_READ_TIMEOUT: return "从IO驱动读取数据超时";
                case DUTCLI_CMD_EXEC_ERROR: return "指令执行失败";
                case DUTCLI_CMD_EXEC_TIMEOUT: return "指令执行超时";
                case INVALID_PARAMETER: return "无效的参数";
                case IODRV_WRITE_FAILED: return "往IO驱动写入数据失败";
                case IODRV_READ_FAILED: return "从IO驱动读取数据失败";

                default:
                    return string.Format("未知错误, {0}", code);
            }
        }
    }
}
