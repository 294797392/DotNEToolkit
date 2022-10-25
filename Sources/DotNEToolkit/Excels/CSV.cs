using DotNEToolkit.Excels;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit
{
    /// <summary>
    /// 提供操作CSV格式文件的方法
    /// </summary>
    public static class CSV
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger("CSV");

        /// <summary>
        /// 把TableData保存成一个CSV文件
        /// </summary>
        /// <param name="tableData"></param>
        /// <param name="csvPath"></param>
        public static void TableData2CSV(string csvPath, TableData tableData)
        {
            StringBuilder builder = new StringBuilder();

            int rows = tableData.GetRows();

            for (int row = 0; row < rows; row++)
            {
                int cols = tableData.GetColumns(row);

                for (int col = 0; col < cols; col++)
                {
                    object data = tableData.Get(row, col);

                    builder.AppendFormat("{0},", data);
                }

                builder.Replace(",", Environment.NewLine, builder.Length - 1, 1);
            }

            File.WriteAllText(csvPath, builder.ToString());
        }

        private static TableData CSV2TableData(string[] lines)
        {
            TableData table = TableData.Create();

            int row = 0, col = 0;

            foreach (string line in lines)
            {
                string[] substrs = line.Split(',');

                foreach (string substr in substrs)
                {
                    table.Set(row, col, substr);
                    col++;
                }

                col = 0;
                row++;
            }

            return table;
        }

        /// <summary>
        /// CSV格式的字符串转成DataTable
        /// </summary>
        /// <returns></returns>
        public static TableData CSV2TableData(string csvText)
        {
            string[] lines = csvText.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return CSV2TableData(lines);
        }

        /// <summary>
        /// CSV文件转成DataTable
        /// </summary>
        /// <param name="csvFile"></param>
        /// <returns></returns>
        public static TableData CSVFile2TableData(string csvFile)
        {
            if (!File.Exists(csvFile))
            {
                logger.WarnFormat("CSV文件不存在, {0}", csvFile);
                return TableData.Create();
            }

            string[] lines = File.ReadAllLines(csvFile);
            return CSV2TableData(lines);
        }

        /// <summary>
        /// 把CSV文件转换成Excel
        /// </summary>
        /// <param name="csvPath"></param>
        /// <param name="excelPath"></param>
        /// <param name="options">写入Excel文件的选项</param>
        /// <returns></returns>
        public static void CSV2Excel(string csvPath, string excelPath, WriteOptions options)
        {
            TableData tableData = CSVFile2TableData(csvPath);
            Excel.TableData2Excel(excelPath, tableData, options);
        }

        /// <summary>
        /// 把CSV文件内容转换成Excel
        /// </summary>
        /// <param name="csvText"></param>
        /// <param name="excelPath"></param>
        /// <param name="options">写入Excel文件的选项</param>
        public static void CSVContent2Excel(string csvText, string excelPath, WriteOptions options)
        {
            TableData tableData = CSV2TableData(csvText);
            Excel.TableData2Excel(excelPath, tableData, options);
        }
    }
}
