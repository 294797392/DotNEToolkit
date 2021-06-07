using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Excels.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ExcelColumnAttribute : Attribute
    {
        /// <summary>
        /// 列名字
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 列的数据类型
        /// </summary>
        public ExcelCellTypes Type { get; set; }

        public ExcelColumnAttribute(string name, ExcelCellTypes type)
        {
            this.Name = name;
            this.Type = type;
        }
    }
}
