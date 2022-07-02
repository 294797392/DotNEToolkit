using DotNEToolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.Communictions
{
    public static class CommObjectExtension
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger("CommObjectExtension");

        private const int DefaultTimeout = 30000;

        public static int ReadMatches(this CommObject commObject, string match, int timeout, out string line)
        {
            line = null;

            DateTime start = DateTime.Now;

            while ((DateTime.Now - start).TotalMilliseconds < timeout)
            {
                string readed = string.Empty;

                try
                {
                    readed = commObject.ReadLine();
                }
                catch (Exception ex) 
                {
                    logger.Error("从通信对象中读取数据异常", ex);
                    return DotNETCode.UNKNOWN_EXCEPTION;
                }

                if (!readed.Contains(match))
                {
                    continue;
                }

                line = readed;
                return DotNETCode.SUCCESS;
            }

            return DotNETCode.TIMEOUT;
        }

        public static int ReadMatches(this CommObject commObject, string match, out string line)
        {
            return ReadMatches(commObject, match, DefaultTimeout, out line);
        }
    }
}
