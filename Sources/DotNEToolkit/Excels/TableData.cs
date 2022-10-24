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

        public CellData(object data)
        {
            this.Value = data;
            this.SpanType = CellSpan.None;
            this.Span = 0;
        }

        public CellData(object data, CellSpan ts, int span)
        {
            this.Value = data;
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
        /// <param name="data"></param>
        public abstract void Set(int row, int col, object data);

        /// <summary>
        /// 设置某个跨行或者跨列单元格的值
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="spanType"></param>
        /// <param name="span"></param>
        /// <param name="data"></param>
        public abstract void Set(int row, int col, CellSpan spanType, int span, object data);

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
                    this.list[row].Add(new CellData(string.Empty));
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

        public override void Set(int row, int col, object data)
        {
            this.EnsureSpace(row, col);
            this.list[row][col].Value = data;
        }

        public override void Set(int row, int col, CellSpan spanType, int span, object data)
        {
            CellData cellData = this.GetCellData(row, col);
            cellData.Value = data;
            cellData.SpanType = spanType;
            cellData.Span = span;
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

        #endregion
    }
}
