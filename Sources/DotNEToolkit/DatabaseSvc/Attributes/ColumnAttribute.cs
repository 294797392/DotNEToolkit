using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.DatabaseSvc.Attributes
{
    /// <summary>
    /// 标识一个列
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute : DatabaseAttribute
    {
    }
}