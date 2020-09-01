using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.DatabaseSvc.Internals
{
    /// <summary>
    /// 存储表信息
    /// </summary>
    internal class InternalTable
    {
        /// <summary>
        /// 数据表名字
        /// </summary>
        internal string Name { get; set; }

        /// <summary>
        /// 表里的所有列名
        /// </summary>
        internal List<InternalColumn> Columns { get; set; }

        /// <summary>
        /// 数据模型类型
        /// </summary>
        internal Type ModelType { get; set; }

        internal InternalTable()
        {
        }
    }
}