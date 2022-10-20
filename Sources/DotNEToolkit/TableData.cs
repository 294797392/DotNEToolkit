using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit
{
    /// <summary>
    /// 描述表格类型的数据
    /// </summary>
    public abstract class TableData
    {
        public abstract bool IsEmpty();

        /// <summary>
        /// 设置某个单元格的值
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="data"></param>
        public abstract void Set(int row, int col, object data);

        /// <summary>
        /// 读取某个单元格的值
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public abstract object Get(int row, int col);

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

        public static TableData Create()
        {
            return new ListTableData();
        }
    }

    internal class ListTableData : TableData
    {
        #region 实例变量

        private List<List<object>> list;

        #endregion

        #region 构造方法

        public ListTableData()
        {
            this.list = new List<List<object>>();
        }

        #endregion

        #region TableData

        public override bool IsEmpty()
        {
            return this.list.Count == 0;
        }

        public override void Set(int row, int col, object data)
        {
            int rows = this.GetRows();
            if (row >= rows)
            {
                // 缺少的行数
                int num = row - rows + 1;
                for (int i = 0; i < num; i++)
                {
                    this.list.Add(new List<object>());
                }
            }

            int cols = this.GetColumns(row);
            if (col >= cols)
            {
                // 缺少的列数
                int num = col - cols + 1;
                for (int i = 0; i < num; i++)
                {
                    this.list[row].Add(string.Empty);        
                }
            }

            this.list[row][col] = data;
        }

        public override object Get(int row, int col)
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

            List<object> rowData = this.list[row];
            return rowData.Count;
        }

        #endregion
    }
}
