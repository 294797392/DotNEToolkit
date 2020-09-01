using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.DatabaseSvc.Attributes
{
    /// <summary>
    /// 标识一张表的信息
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : DatabaseAttribute
    {
        public TableAttribute()
        {
        }
    }
}