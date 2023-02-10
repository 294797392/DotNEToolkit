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

    /// <summary>
    /// CSV文件的分隔符
    /// </summary>
    public enum CSVSplitters
    {
        /// <summary>
        /// 未知
        /// </summary>
        Unkown,

        /// <summary>
        /// 逗号分隔
        /// </summary>
        Comma,

        /// <summary>
        /// 空格分隔
        /// </summary>
        Space,

        /// <summary>
        /// Tab键分隔
        /// </summary>
        Tab
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
        private static readonly char[] CSVSplitter = new char[] { ',', ' ', '\t' };

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
        /// 把TableData保存成一个CSV文件
        /// </summary>
        /// <param name="tableData"></param>
        /// <param name="csvPath"></param>
        /// <param name="splitter">CSV文件的分隔符</param>
        public static void TableData2CSVFile(TableData tableData, string csvPath, CSVSplitters splitter = CSVSplitters.Comma)
        {
            string splitterText = string.Empty;

            switch (splitter)
            {
                case CSVSplitters.Comma: splitterText = ","; break;
                case CSVSplitters.Space: splitterText = " "; break;
                case CSVSplitters.Tab: splitterText = "\t"; break;
                case CSVSplitters.Unkown: splitterText = ","; break;
                default:
                    throw new NotImplementedException();
            }

            StringBuilder builder = new StringBuilder();

            int rows = tableData.GetRows();

            for (int row = 0; row < rows; row++)
            {
                int cols = tableData.GetColumns(row);

                for (int col = 0; col < cols; col++)
                {
                    object data = tableData.Get(row, col).Value;

                    builder.AppendFormat("{0}{1}", data == null ? string.Empty : data, splitterText);
                }

                builder.Replace(splitterText, Environment.NewLine, builder.Length - 1, 1);
            }

            File.WriteAllText(csvPath, builder.ToString());
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
            ExcelUtils.TableData2ExcelFile(excelPath, tableData, options);
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
            ExcelUtils.TableData2ExcelFile(excelPath, tableData, options);
        }

        /// <summary>
        /// 把一个CSV文件转换成一个对象集合
        /// 要使用CSVColumnAttribute把类里的属性和CSV文件里的字段映射起来
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="csvPath"></param>
        /// <param name="splitter">CSV文件分隔符</param>
        /// <returns></returns>
        public static List<T> CSVFile2Objects<T>(string csvPath, CSVSplitters splitter = CSVSplitters.Unkown)
        {
            string[] lines = ReadCSVLines(csvPath);
            if (lines == null)
            {
                return default(List<T>);
            }

            return CSVLines2Objects<T>(lines, splitter);
        }

        public static List<T> CSVLines2Objects<T>(string[] lines, CSVSplitters splitter = CSVSplitters.Unkown)
        {
            #region 判断CSV文件的分隔符

            StringSplitOptions splitOptions = StringSplitOptions.RemoveEmptyEntries;

            switch (splitter)
            {
                case CSVSplitters.Comma:
                case CSVSplitters.Tab:
                    {
                        // 用这些字符分隔的CSV文件是可以区分是否有空内容的
                        splitOptions = StringSplitOptions.None;
                        break;
                    }

                case CSVSplitters.Unkown:
                case CSVSplitters.Space:
                    {
                        // 这些分隔符区分不了是否有空内容，遇到空内容直接忽略，不当成CSV的内容处理
                        splitOptions = StringSplitOptions.RemoveEmptyEntries;
                        break;
                    }
            }

            #endregion

            List<string> headers = lines[0].Split(CSVSplitter, splitOptions).ToList();

            List<T> result = new List<T>();

            List<PropertyAttribute<TableColumnAttribute>> properties = ReflectionUtils.GetPropertyAttribute<TableColumnAttribute, T>();

            for (int i = 1; i < lines.Length; i++)
            {
                string[] values = lines[i].Split(CSVSplitter, splitOptions);

                if (values.Length != headers.Count)
                {
                    // 标题行的数量和数据行的数量不一致，CSV文件有问题
                    Console.WriteLine("行不一致, LineIndex = {0}", i);
                    return default(List<T>);
                }

                T newObject = Activator.CreateInstance<T>();

                foreach (PropertyAttribute<TableColumnAttribute> property in properties)
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
