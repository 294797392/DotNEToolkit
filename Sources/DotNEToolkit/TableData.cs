using DotNEToolkit.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit
{
    /// <summary>
    /// 指定某个单元格的跨行方式
    /// </summary>
    public enum CellSpan
    {
        /// <summary>
        /// 不跨行
        /// </summary>
        None,

        /// <summary>
        /// 跨行
        /// </summary>
        RowSpan,

        /// <summary>
        /// 跨列
        /// </summary>
        ColSpan
    }

    /// <summary>
    /// 存储一个单元格的数据
    /// </summary>
    public class CellData
    {
        /// <summary>
        /// 该单元格所在行
        /// </summary>
        public int Row { get; set; }

        /// <summary>
        /// 单元格所在列
        /// </summary>
        public int Column { get; set; }

        /// <summary>
        /// 单元格的值
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// 单元格的跨行或跨列方式
        /// </summary>
        public CellSpan SpanType { get; set; }

        /// <summary>
        /// 单元格跨了几个单元格
        /// </summary>
        public int Span { get; set; }

        /// <summary>
        /// 创建一个空的CellData
        /// </summary>
        public CellData()
        {
            this.Value = null;
        }

        public CellData(int row, int col)
        {
            this.Row = row;
            this.Column = col;
        }

        public CellData(object value)
        {
            this.Value = value;
            this.SpanType = CellSpan.None;
            this.Span = 0;
        }

        public CellData(object value, CellSpan ts, int span)
        {
            this.Value = value;
            this.SpanType = ts;
            this.Span = span;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class TableColumnAttribute : Attribute
    {
        public string Name { get; set; }

        /// <summary>
        /// 指定列宽度
        /// </summary>
        public int Width { get; set; }

        //public CSVDataTypes DataType { get; set; }

        public TableColumnAttribute(string name)
        {
            this.Name = name;
        }
    }

    /// <summary>
    /// 描述表格类型的数据
    /// </summary>
    public abstract class TableData
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger("TableData");

        /// <summary>
        /// 表格名字
        /// </summary>
        public string Name { get; set; }
     
        /// <summary>
        /// 返回该TableData是否为空
        /// </summary>
        /// <returns></returns>
        public abstract bool IsEmpty();

        /// <summary>
        /// 设置某个单元格的值
        /// 如果重复设置，那么会覆盖之前的值
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="value">该单元格的值</param>
        public abstract void SetCell(int row, int col, object value);

        /// <summary>
        /// 设置某个跨行或者跨列单元格的值
        /// 如果重复设置，那么会覆盖之前的值
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="spanType"></param>
        /// <param name="span"></param>
        /// <param name="value">该单元格的值</param>
        public abstract void SetCell(int row, int col, CellSpan spanType, int span, object value);

        public object GetCellValue(int row, int col, object defaultValue)
        {
            CellData cellData = this.GetCell(row, col);
            return cellData == null ? defaultValue : cellData.Value;
        }

        /// <summary>
        /// 读取某个单元格对象
        /// 如果没有这个单元格，那么返回空
        /// </summary>
        /// <param name="row">单元格所在行索引</param>
        /// <param name="col">单元格所在列索引</param>
        /// <returns></returns>
        public abstract CellData GetCell(int row, int col);

        /// <summary>
        /// 获取某一行的所有列
        /// 如果没有该行，返回空
        /// </summary>
        /// <returns></returns>
        public abstract List<CellData> GetCells(int row);

        /// <summary>
        /// 清除指定的单元格
        /// </summary>
        /// <param name="row">单元格所在行</param>
        /// <param name="col">单元格所在列</param>
        /// <returns></returns>
        public abstract void Clear(int row, int col);

        /// <summary>
        /// 获取总行数
        /// </summary>
        /// <returns></returns>
        public abstract int GetRows();

        /// <summary>
        /// 获取某一行的列数
        /// </summary>
        /// <param name="row">要获取的行的索引</param>
        /// <returns></returns>
        public abstract int GetColumns(int row);

        /// <summary>
        /// 和tableData合并
        /// 把tableData追加到该tableData下面
        /// </summary>
        /// <param name="tableData">要合并的tableData</param>
        public void Merge(TableData tableData)
        {
            int thisRows = this.GetRows();

            int rows = tableData.GetRows();

            for (int row = 0; row < rows; row++)
            {
                int cols = tableData.GetColumns(row);

                for (int col = 0; col < cols; col++)
                {
                    CellData cellData = tableData.GetCell(row, col);
                    if (cellData == null)
                    {
                        continue;
                    }

                    this.SetCell(thisRows + row, col, cellData.SpanType, cellData.Span, cellData.Value);
                }
            }
        }

        /// <summary>
        /// 创建一个TableData的实例
        /// </summary>
        /// <returns>TableData实例</returns>
        public static TableData Create()
        {
            return new ListTableData();
        }

        /// <summary>
        /// 转换成ObjectList
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> ConvertToObjects<T>()
        {
            int rows = this.GetRows();

            List<string> headers = this.GetRowData(0);

            List<PropertyAttribute<TableColumnAttribute>> properties = ReflectionUtils.GetPropertyAttribute<TableColumnAttribute, T>();

            List<T> result = new List<T>();

            for (int i = 1; i < rows; i++)
            {
                T newObject = Activator.CreateInstance<T>();

                foreach (PropertyAttribute<TableColumnAttribute> property in properties)
                {
                    string propertyName = property.Property.Name;
                    int valueIndex = headers.IndexOf(property.Attribute.Name);
                    object propertyValue = this.GetCell(i, valueIndex).Value;

                    // 如果从excel里读到的数据类型和要转换的类的属性类型不一致，那么尝试转换成类里的属性类型
                    object convertedValue = Convert.ChangeType(propertyValue, property.Property.PropertyType);

                    property.Property.SetValue(newObject, convertedValue);
                }

                result.Add(newObject);
            }

            return result;
        }

        /// <summary>
        /// 获取一行里的所有数据
        /// </summary>
        /// <returns></returns>
        private List<string> GetRowData(int row)
        {
            int cols = this.GetColumns(row);

            List<string> result = new List<string>();

            for (int i = 0; i < cols; i++)
            {
                CellData cellData = this.GetCell(row, i);
                result.Add(cellData.Value == null ? string.Empty : cellData.Value.ToString());
            }

            return result;
        }
    }

    internal class ListTableData : TableData
    {
        #region 实例变量

        private List<List<CellData>> table;

        #endregion

        #region 构造方法

        public ListTableData()
        {
            this.table = new List<List<CellData>>();
        }

        #endregion

        #region 实例方法

        //private void Resize(int rows, int cols) 
        //{
        //    for (int row = 0; row < rows; row++)
        //    {
        //        List<CellData> cellDatas = new List<CellData>();
        //        this.table.Add(cellDatas);
        //    }

        //    for (int col = 0; col < cols; col++)
        //    {

        //    }
        //}

        private List<CellData> EnsureRow(int row)
        {
            List<CellData> rowData = null;

            #region 确保行存在

            if (this.table.Count <= row)
            {
                // 表格数据不够

                // 算出来缺少的行数
                int rows = row - this.table.Count + 1;

                for (int i = 0; i < rows; i++)
                {
                    rowData = new List<CellData>();
                    this.table.Add(rowData);
                }
            }
            else
            {
                rowData = this.table[row];
            }

            #endregion

            return rowData;
        }

        private CellData EnsureColumn(List<CellData> cells, int col)
        {
            CellData cellData = null;

            if (cells.Count <= col)
            {
                // 列数量不够

                int cols = col - cells.Count + 1;

                for (int i = 0; i < cols; i++)
                {
                    cellData = new CellData();
                    cells.Add(cellData);
                }
            }
            else
            {
                cellData = cells[col];
            }

            return cellData;
        }

        private CellData EnsureCellData(int row, int col, CellSpan spanType, int span)
        {
            switch (spanType)
            {
                case CellSpan.None:
                    {
                        List<CellData> cells = this.EnsureRow(row);
                        return this.EnsureColumn(cells, col);
                    }

                case CellSpan.ColSpan:
                    {
                        List<CellData> cells = this.EnsureRow(row);
                        this.EnsureColumn(cells, col + span);
                        return cells[col];
                    }

                case CellSpan.RowSpan:
                    {
                        int lastRow = row + span;

                        #region 确保行存在

                        if (this.table.Count <= lastRow)
                        {
                            // 表格数据不够

                            // 算出来缺少的行数
                            int rows = lastRow - this.table.Count + 1;

                            for (int i = 0; i < rows; i++)
                            {
                                List<CellData> cells = new List<CellData>();
                                this.table.Add(cells);

                                // 每一行都需要至少col列
                                this.EnsureColumn(cells, col);
                            }
                        }
                        else
                        {
                            for (int i = row; i < lastRow; i++)
                            {
                                List<CellData> cells = this.table[i];
                                this.EnsureColumn(cells, col);
                            }
                        }

                        #endregion

                        return this.table[row][col];
                    }

                default:
                    throw new NotImplementedException();
            }
        }

        #endregion

        #region TableData

        public override bool IsEmpty()
        {
            return this.table.Count == 0;
        }

        public override void SetCell(int row, int col, object value)
        {
            CellData cellData = this.EnsureCellData(row, col, CellSpan.None, 0);
            cellData.Value = value;
        }

        public override void SetCell(int row, int col, CellSpan spanType, int span, object value)
        {
            if (span == 0)
            {
                spanType = CellSpan.None;
            }

            switch (spanType)
            {
                case CellSpan.None:
                    {
                        this.SetCell(row, col, value);
                        break;
                    }

                case CellSpan.ColSpan:
                    {
                        CellData cellData = this.EnsureCellData(row, col, spanType, span);
                        cellData.Value = value;
                        cellData.SpanType = spanType;
                        cellData.Span = span;
                        break;
                    }

                case CellSpan.RowSpan:
                    {
                        CellData cellData = this.EnsureCellData(row, col, spanType, span);
                        cellData.Value = value;
                        cellData.SpanType = spanType;
                        cellData.Span = span;
                        break;
                    }

                default:
                    throw new NotImplementedException();
            }
        }

        public override CellData GetCell(int row, int col)
        {
            if (this.table.Count <= row)
            {
                return null;
            }

            List<CellData> cells = this.table[row];

            if (cells.Count <= col)
            {
                return null;
            }

            return cells[col];
        }

        public override List<CellData> GetCells(int row)
        {
            if (this.table.Count <= row)
            {
                return null;
            }

            return this.table[row];
        }

        public override void Clear(int row, int col)
        {
            CellData cellData = this.GetCell(row, col);
            if (cellData == null)
            {
                return;
            }

            cellData.Span = 0;
            cellData.SpanType = CellSpan.None;
            cellData.Value = string.Empty;
        }

        public override int GetRows()
        {
            return this.table.Count;
        }

        public override int GetColumns(int row)
        {
            List<CellData> cells = this.GetCells(row);
            if (cells == null) 
            {
                return 0;
            }

            return cells.Count;
        }

        #endregion
    }
}
