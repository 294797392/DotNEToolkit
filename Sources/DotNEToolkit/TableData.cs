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
        public abstract void Set(int row, int col, object value);

        /// <summary>
        /// 设置某个跨行或者跨列单元格的值
        /// 如果重复设置，那么会覆盖之前的值
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="spanType"></param>
        /// <param name="span"></param>
        /// <param name="value">该单元格的值</param>
        public abstract void Set(int row, int col, CellSpan spanType, int span, object value);

        /// <summary>
        /// 读取某个单元格的值
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public abstract CellData Get(int row, int col);

        /// <summary>
        /// 清除指定的单元格
        /// </summary>
        /// <param name="row">单元格所在行</param>
        /// <param name="col">单元格所在列</param>
        /// <returns></returns>
        public abstract void Clear(int row, int col);

        /// <summary>
        /// 获取从行数
        /// </summary>
        /// <returns></returns>
        public abstract int GetRows();

        /// <summary>
        /// 获取总列数
        /// </summary>
        /// <returns></returns>
        public abstract int GetColumns();

        /// <summary>
        /// 获取某一列的总行数
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public abstract int GetRows(int column);

        /// <summary>
        /// 获取某一行的总列数
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public abstract int GetColumns(int row);

        /// <summary>
        /// 和tableData合并
        /// 把tableData追加到该tableData下面
        /// </summary>
        /// <param name="tableData">要合并的tableData</param>
        public abstract void Merge(TableData tableData);

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
            int cols = this.GetColumns();

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
                    string propertyValue = this.Get(i, valueIndex).Value.ToString();

                    property.Property.SetValue(newObject, propertyValue);
                }

                result.Add(newObject);
            }

            return result;
        }

        /// <summary>
        /// 获取一行里的所有数据
        /// </summary>
        /// <returns></returns>
        public List<string> GetRowData(int row)
        {
            int cols = this.GetColumns(row);

            List<string> result = new List<string>();

            for (int i = 0; i < cols; i++)
            {
                CellData cellData = this.Get(row, i);
                result.Add(cellData.Value == null ? string.Empty : cellData.Value.ToString());
            }

            return result;
        }
    }

    internal class ListTableData : TableData
    {
        #region 实例变量

        private List<CellData> cellDatas;

        #endregion

        #region 构造方法

        public ListTableData()
        {
            this.cellDatas = new List<CellData>();
        }

        #endregion

        #region 实例方法

        private CellData EnsureCellData(int row, int col, CellSpan spanType, int span)
        {
            switch (spanType)
            {
                case CellSpan.None:
                    {
                        CellData cellData = this.cellDatas.FirstOrDefault(v => v.Row == row && v.Column == col);
                        if (cellData == null)
                        {
                            cellData = new CellData(row, col);
                            this.cellDatas.Add(cellData);
                        }

                        return cellData;
                    }

                case CellSpan.ColSpan:
                    {
                        int startCol = col;
                        int endCol = startCol + span;

                        for (int i = startCol; i < endCol; i++)
                        {
                            CellData cellData = this.cellDatas.FirstOrDefault(v => v.Row == row && v.Column == i);
                            if (cellData == null)
                            {
                                cellData = new CellData(row, i);
                                this.cellDatas.Add(cellData);
                            }
                        }

                        return this.cellDatas.FirstOrDefault(v => v.Row == row && v.Column == col);
                    }

                case CellSpan.RowSpan:
                    {
                        int startRow = row;
                        int endRow = startRow + span;

                        for (int i = startRow; i < endRow; i++)
                        {
                            CellData cellData = this.cellDatas.FirstOrDefault(v => v.Row == i && v.Column == col);
                            if (cellData == null)
                            {
                                cellData = new CellData(i, col);
                                this.cellDatas.Add(cellData);
                            }
                        }

                        return this.cellDatas.FirstOrDefault(v => v.Row == row && v.Column == col);
                    }

                default:
                    throw new NotImplementedException();
            }
        }

        #endregion

        #region TableData

        public override bool IsEmpty()
        {
            return this.cellDatas.Count == 0;
        }

        public override void Set(int row, int col, object value)
        {
            CellData cellData = this.EnsureCellData(row, col, CellSpan.None, 0);
            cellData.Value = value;
        }

        public override void Set(int row, int col, CellSpan spanType, int span, object value)
        {
            if (span == 0)
            {
                spanType = CellSpan.None;
            }

            switch (spanType)
            {
                case CellSpan.None:
                    {
                        this.Set(row, col, value);
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

        public override CellData Get(int row, int col)
        {
            return this.cellDatas.FirstOrDefault(v => v.Row == row && v.Column == col);
        }

        public override void Clear(int row, int col)
        {
            CellData cellData = this.cellDatas.FirstOrDefault(v => v.Row == row && v.Column == col);
            if (cellData != null)
            {
                cellData.Span = 0;
                cellData.SpanType = CellSpan.None;
                cellData.Value = string.Empty;
            }
        }

        public override int GetRows()
        {
            if (this.cellDatas.Count == 0)
            {
                return 0;
            }
            else
            {
                return this.cellDatas.Max(v => v.Row) + 1;
            }
        }

        public override int GetColumns()
        {
            if (this.cellDatas.Count == 0)
            {
                return 0;
            }
            else
            {
                return this.cellDatas.Max(v => v.Column) + 1;
            }
        }

        public override int GetRows(int column)
        {
            if (this.cellDatas.Exists(v => v.Column == column))
            {
                return this.cellDatas.Where(v => v.Column == column).Max(v => v.Row) + 1;
            }
            else
            {
                return 0;
            }
        }

        public override int GetColumns(int row)
        {
            if (this.cellDatas.Exists(v => v.Row == row))
            {
                return this.cellDatas.Where(v => v.Row == row).Max(v => v.Column) + 1;
            }
            else
            {
                return 0;
            }
        }

        public override void Merge(TableData tableData)
        {
            int thisRows = this.GetRows();

            int rows = tableData.GetRows();

            for (int row = 0; row < rows; row++)
            {
                int cols = tableData.GetColumns(row);

                for (int col = 0; col < cols; col++)
                {
                    CellData cellData = tableData.Get(row, col);
                    if (cellData == null)
                    {
                        continue;
                    }

                    this.Set(thisRows + row, col, cellData.SpanType, cellData.Span, cellData.Value);
                }
            }
        }

        #endregion
    }
}
