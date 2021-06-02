using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.DatabaseSvc.Attributes
{
    /// <summary>
    /// 所有自定义特性的基类
    /// </summary>
    public abstract class DatabaseAttribute : Attribute
    {
        /// <summary>
        /// 名字
        /// </summary>
        public string Name { get; set; }
    }
}