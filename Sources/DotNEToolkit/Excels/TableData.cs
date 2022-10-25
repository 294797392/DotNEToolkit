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

    public class CellData
    {
        public object Value { get; set; }

        public CellSpan SpanType { get; set; }

        public int Span { get; set; }

        /// <summary>
        /// 创建一个空的CellData
        /// </summary>
        public CellData()
        {
            this.Value = null;
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

    /// <summary>
    /// 描述表格类型的数据
    /// </summary>
    public abstract class TableData
    {
        /// <summary>
        /// 返回该TableData是否为空
        /// </summary>
        /// <returns></returns>
        public abstract bool IsEmpty();

        /// <summary>
        /// 设置某个单元格的值
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="value">该单元格的值</param>
        public abstract void Set(int row, int col, object value);

        /// <summary>
        /// 设置某个跨行或者跨列单元格的值
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
        /// 获取总函数
        /// </summary>
        /// <returns></returns>
        public abstract int GetRows();

        /// <summary>
        /// 获取某行的列数
        /// </summary>
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
    }

    internal class ListTableData : TableData
    {
        #region 实例变量

        private List<List<CellData>> list;

        #endregion

        #region 构造方法

        public ListTableData()
        {
            this.list = new List<List<CellData>>();
        }

        #endregion

        #region 实例方法

        /// <summary>
        /// 保证至少有row行和col列
        /// 如果小于row行或者col列，那么用空补齐
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        private void EnsureSpace(int row, int col)
        {
            int rows = this.GetRows();
            if (row >= rows)
            {
                // 缺少的行数
                int num = row - rows + 1;
                for (int i = 0; i < num; i++)
                {
                    this.list.Add(new List<CellData>());
                }
            }

            int cols = this.GetColumns(row);
            if (col >= cols)
            {
                // 缺少的列数
                int num = col - cols + 1;
                for (int i = 0; i < num; i++)
                {
                    this.list[row].Add(new CellData());
                }
            }
        }

        private CellData GetCellData(int row, int col)
        {
            this.EnsureSpace(row, col);
            return this.list[row][col];
        }

        #endregion

        #region TableData

        public override bool IsEmpty()
        {
            return this.list.Count == 0;
        }

        public override void Set(int row, int col, object value)
        {
            this.EnsureSpace(row, col);
            this.list[row][col].Value = value;
        }

        public override void Set(int row, int col, CellSpan spanType, int span, object value)
        {
            switch (spanType)
            {
                case CellSpan.None:
                    {
                        this.Set(row, col, value);
                        break;
                    }

                case CellSpan.ColSpan:
                    {
                        CellData cellData = this.GetCellData(row + span, col);
                        cellData.Value = value;
                        cellData.SpanType = spanType;
                        cellData.Span = span;
                        break;
                    }

                case CellSpan.RowSpan:
                    {
                        
                    }
            }

        }

        public override CellData Get(int row, int col)
        {
            int rows = this.GetRows();
            if (row >= rows)
            {
                return null;
            }

            int cols = this.GetColumns(row);
            if (col >= cols)
            {
                return null;
            }

            return this.list[row][col];
        }

        public override void Clear(int row, int col)
        {
            CellData cellData = this.GetCellData(row, col);
            cellData.Span = 0;
            cellData.SpanType = CellSpan.None;
            cellData.Value = string.Empty;
        }

        public override int GetRows()
        {
            return this.list.Count;
        }

        public override int GetColumns(int row)
        {
            if (row >= this.list.Count)
            {
                return 0;
            }

            List<CellData> rowData = this.list[row];
            return rowData.Count;
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

                    this.Set(thisRows + row, col, cellData.SpanType, cellData.Span, cellData.Value);
                }
            }
        }

        #endregion
    }
}
