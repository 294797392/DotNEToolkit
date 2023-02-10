using DotNEToolkit;
using DotNEToolkit.Excels;
using DotNEToolkit.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkitConsole
{
    public static class TestExcel
    {
        private static int value = 0;

        public static void CreateNew()
        {
            string excelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "1.xls");

            TableData tableData = TableData.Create();
            tableData.Set(0, 0, "0");
            tableData.Set(0, 1, "0");
            tableData.Set(0, 2, "0");
            tableData.Set(0, 3, "0");
            tableData.Set(0, 4, "0");
            tableData.Set(0, 5, "0");
            ExcelUtils.TableData2Excel(excelPath, tableData, WriteOptions.CreateNew);
        }

        public static void CreateOrAppend()
        {
            string excelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "1.xls");

            TableData tableData = TableData.Create();
            tableData.Set(0, 0, value++.ToString());
            tableData.Set(0, 1, value++.ToString());
            tableData.Set(0, 2, value++.ToString());
            tableData.Set(0, 3, value++.ToString());
            tableData.Set(0, 4, value++.ToString());
            tableData.Set(0, 6, value++.ToString());
            ExcelUtils.TableData2Excel(excelPath, tableData, WriteOptions.Append);
        }

        public static void CreateSpan()
        {
            string excelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "1.xls");

            TableData tableData = TableData.Create();
            tableData.Set(0, 0, CellSpan.RowSpan, 10, 0);
            tableData.Set(0, 1, CellSpan.RowSpan, 9, 1);
            tableData.Set(0, 2, CellSpan.RowSpan, 8, 2);
            tableData.Set(0, 3, CellSpan.RowSpan, 7, 3);
            tableData.Set(0, 4, CellSpan.RowSpan, 6, 4);
            tableData.Set(0, 5, CellSpan.RowSpan, 5, 5);
            tableData.Set(0, 6, CellSpan.RowSpan, 4, 6);
            tableData.Set(0, 7, CellSpan.RowSpan, 3, 7);
            tableData.Set(0, 8, CellSpan.RowSpan, 2, 8);
            tableData.Set(0, 9, CellSpan.RowSpan, 1, 9);

            tableData.Merge(tableData);

            ExcelUtils.TableData2Excel(excelPath, tableData, WriteOptions.Append);
        }
    }
}
