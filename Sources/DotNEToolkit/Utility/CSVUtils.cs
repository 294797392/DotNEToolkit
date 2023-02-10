using DotNEToolkit.Excels;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.Utility
{
    //public enum CSVDataTypes
    //{
    //    Int32,

    //    String
    //}

    [AttributeUsage(AttributeTargets.Property)]
    public class CSVColumnAttribute : Attribute
    {
        public string Name { get; set; }

        //public CSVDataTypes DataType { get; set; }

        public CSVColumnAttribute(string name)
        {
            this.Name = name;
        }
    }

    /// <summary>
    /// 提供操作CSV格式文件的方法
    /// </summary>
    public static class CSVUtils
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger("CSV");

        /// <summary>
        /// CSV文件一行的分隔符
        /// 有的CSV文件是用逗号，有的是用空格分隔
        /// </summary>
        private static readonly char[] CSVSplitter = new char[] { ',', ' ' };

        /// <summary>
        /// 把TableData保存成一个CSV文件
        /// </summary>
        /// <param name="tableData"></param>
        /// <param name="csvPath"></param>
        public static void TableData2CSVFile(string csvPath, TableData tableData)
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
                string[] csvItems = line.Split(CSVSplitter, StringSplitOptions.None);

                foreach (string csvItem in csvItems)
                {
                    table.Set(row, col, csvItem);
                    col++;
                }

                col = 0;
                row++;
            }

            return table;
        }

        private static string[] ReadCSVLines(string csvFile)
        {
            if (!File.Exists(csvFile))
            {
                logger.WarnFormat("CSV文件不存在, {0}", csvFile);
                return null;
            }

            try
            {
                string[] lines = File.ReadAllLines(csvFile);
                return lines;
            }
            catch (Exception ex)
            {
                logger.Error("读取CSV文件异常", ex);
                return null;
            }
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
            string[] lines = ReadCSVLines(csvFile);
            if (lines == null)
            {
                return null;
            }

            return CSV2TableData(lines);
        }

        /// <summary>
        /// 把CSV文件转换成Excel
        /// </summary>
        /// <param name="csvPath"></param>
        /// <param name="excelPath"></param>
        /// <param name="options">写入Excel文件的选项</param>
        /// <returns></returns>
        public static void CSVFile2Excel(string csvPath, string excelPath, WriteOptions options)
        {
            TableData tableData = CSVFile2TableData(csvPath);
            ExcelUtils.TableData2Excel(excelPath, tableData, options);
        }

        /// <summary>
        /// 把CSV文件内容转换成Excel
        /// </summary>
        /// <param name="csvText"></param>
        /// <param name="excelPath"></param>
        /// <param name="options">写入Excel文件的选项</param>
        public static void CSV2Excel(string csvText, string excelPath, WriteOptions options)
        {
            TableData tableData = CSV2TableData(csvText);
            ExcelUtils.TableData2Excel(excelPath, tableData, options);
        }

        /// <summary>
        /// 把一个CSV文件转换成一个对象集合
        /// 要使用CSVColumnAttribute把类里的属性和CSV文件里的字段映射起来
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="csvPath"></param>
        /// <returns></returns>
        public static List<T> CSVFile2Objects<T>(string csvPath)
        {
            string[] lines = ReadCSVLines(csvPath);
            if (lines == null)
            {
                return default(List<T>);
            }

            List<PropertyAttribute<CSVColumnAttribute>> properties = ReflectionUtils.GetPropertyAttribute<CSVColumnAttribute, T>();

            List<string> headers = lines[0].Split(CSVSplitter, StringSplitOptions.RemoveEmptyEntries).ToList();

            List<T> result = new List<T>();

            for (int i = 1; i < lines.Length; i++)
            {
                string[] values = lines[i].Split(CSVSplitter, StringSplitOptions.RemoveEmptyEntries);

                if(values.Length != headers.Count)
                {
                    // 标题行的数量和数据行的数量不一致，CSV文件有问题
                    return default(List<T>);
                }

                T newObject = Activator.CreateInstance<T>();

                foreach (PropertyAttribute<CSVColumnAttribute> property in properties)
                {
                    string propertyName = property.Property.Name;
                    int valueIndex = headers.IndexOf(property.Attribute.Name);
                    string propertyValue = values[valueIndex];

                    property.Property.SetValue(newObject, propertyValue);
                }

                result.Add(newObject);
            }

            return result;
        }
    }
}
