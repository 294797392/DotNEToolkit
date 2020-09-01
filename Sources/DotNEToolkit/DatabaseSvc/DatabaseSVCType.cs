using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.DatabaseSvc
{
    public enum DatabaseSVCType
    {
        /// <summary>
        /// WCF实现方式
        /// </summary>
        WCF,

        /// <summary>
        /// HttpListener实现方式
        /// </summary>
        HttpListener
    }
}