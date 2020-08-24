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
        /// <summary>
        /// 存储表里的所有列名
        /// </summary>
        internal List<ColumnAttribute> Columns { get; set; }

        public TableAttribute()
        {
        }
    }
}