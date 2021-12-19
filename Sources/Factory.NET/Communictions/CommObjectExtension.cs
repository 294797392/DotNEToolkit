using DotNEToolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Factory.NET.Communictions
{
    public static class CommObjectExtension
    {
        private const int DefaultTimeout = 30000;

        public static int ReadMatches(this CommunicationObject commObject, string match, int timeout, out string line)
        {
            line = null;

            DateTime start = DateTime.Now;

            while ((DateTime.Now - start).TotalMilliseconds < timeout)
            {
                string readed;
                int code = commObject.ReadLine(out readed);
                if (code != DotNETCode.SUCCESS)
                {
                    return code;
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

        public static int ReadMatches(this CommunicationObject commObject, string match, out string line)
        {
            return ReadMatches(commObject, match, DefaultTimeout, out line);
        }
    }
}
