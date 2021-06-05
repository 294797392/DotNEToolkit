using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Excels
{
    /// <summary>
    /// 表示Exce里的一个单元格的信息
    /// </summary>
    public class ExcelCell
    {
        public ExcelCellTypes Type { get; set; }

        public object Value { get; set; }

        public ExcelCell(ExcelCellTypes type, object value)
        {
            this.Type = type;
            this.Value = value;
        }
    }
}
