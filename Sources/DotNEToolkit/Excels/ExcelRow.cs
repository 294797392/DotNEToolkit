using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Excels
{
    /// <summary>
    /// 表示Excel里的某一行
    /// </summary>
    public class ExcelRow
    {
        private List<ExcelCell> cells;

        /// <summary>
        /// 这一行里一共多少列有数据
        /// </summary>
        public ReadOnlyCollection<ExcelCell> Cells { get; private set; }

        public ExcelRow()
        {
            this.cells = new List<ExcelCell>();
            this.Cells = new ReadOnlyCollection<ExcelCell>(this.cells);
        }

        public void AddCell(object value, ExcelCellTypes cellType)
        {
            this.cells.Add(new ExcelCell(cellType, value));
        }

        public ExcelCell GetCell(int index)
        {
            if (index >= this.cells.Count)
            {
                return null;
            }

            return this.cells[index];
        }

        public double GetCellValueNumberic(int index, double defaultValue)
        {
            ExcelCell cell = this.GetCell(index);
            if (cell == null || cell.Type == ExcelCellTypes.Null)
            {
                return defaultValue;
            }

            double value;
            if (!double.TryParse(cell.Value.ToString(), out value))
            {
                return defaultValue;
            }
            else
            {
                return value;    
            }
        }

        public string GetCellValueString(int index, string defaultValue)
        {
            ExcelCell cell = this.GetCell(index);
            if (cell == null || cell.Type == ExcelCellTypes.Null)
            {
                return defaultValue;
            }

            return cell.Value.ToString();
        }
    }
}
