using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Excels
{
    /// <summary>
    /// 表示一个Excel的工作表
    /// </summary>
    public class ExcelSheet
    {
        private List<ExcelRow> rows;

        public string Name { get; set; }

        /// <summary>
        /// 工作表里的所有的行数据
        /// </summary>
        public ReadOnlyCollection<ExcelRow> Rows { get; private set; }

        public ExcelSheet()
        {
            this.rows = new List<ExcelRow>();
            this.Rows = new ReadOnlyCollection<ExcelRow>(this.rows);
        }

        public void AddRow(ExcelRow row)
        {
            this.rows.Add(row);
        }
    }
}
