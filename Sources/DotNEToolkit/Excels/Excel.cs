using DotNEToolkit.Excels;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DotNEToolkit
{
    /// <summary>
    /// 封装Excel文件的导入，导出功能
    /// </summary>
    public static class Excel
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger("Excel");

        private static void WriteSheet(ISheet sheet, DataTable table)
        {
            int startRow = sheet.PhysicalNumberOfRows;
            int columns = table.Columns.Count;

            // 先把所有的列名写一行
            IRow titleRow = sheet.CreateRow(startRow++);
            for (int i = 0; i < columns; i++)
            {
                ICell cell = titleRow.CreateCell(i, CellType.String);
                cell.SetCellValue(table.Columns[i].ColumnName);
            }

            foreach (DataRow dataRow in table.Rows)
            {
                IRow row = sheet.CreateRow(startRow++);

                for (int columnIndex = 0; columnIndex < columns; columnIndex++)
                {
                    object value = dataRow[columnIndex];
                    if (value == null || value == DBNull.Value)
                    {
                        row.CreateCell(columnIndex, CellType.String);
                    }
                    else
                    {
                        CreateCell(row, columnIndex, value);
                    }
                }
            }
        }

        private static void WriteSheet(ISheet sheet, TableData table)
        {
            int startRow = sheet.PhysicalNumberOfRows;
            int rows = table.GetRows();

            for (int row = 0; row < rows; row++)
            {
                int cols = table.GetColumns(row);

                IRow irow = sheet.CreateRow(startRow++);

                for (int col = 0; col < cols; col++)
                {
                    object value = table.Get(row, col);
                    if (value == null)
                    {
                        irow.CreateCell(col, CellType.String);
                    }
                    else
                    {
                        CreateCell(irow, col, value);
                    }
                }
            }
        }

        private static void WriteSheet(ISheet sheet, object[,] excelData)
        {
            int startRow = sheet.PhysicalNumberOfRows;
            int rows = excelData.GetLength(0);
            int columns = excelData.GetLength(1);

            for (int rowIndex = 0; rowIndex < rows; rowIndex++)
            {
                IRow row = sheet.CreateRow(startRow++);

                for (int columnIndex = 0; columnIndex < columns; columnIndex++)
                {
                    object value = excelData[rowIndex, columnIndex];
                    if (value == null)
                    {
                        row.CreateCell(columnIndex, CellType.String);
                    }
                    else
                    {
                        CreateCell(row, columnIndex, value);
                    }
                }
            }
        }

        private static ISheet OpenOrCreateSheet(IWorkbook workbook, string sheetName, WriteOptions options)
        {
            switch (options)
            {
                case WriteOptions.Append:
                    {
                        ISheet sheet = workbook.GetSheet(sheetName);
                        if (sheet == null)
                        {
                            sheet = workbook.CreateSheet(sheetName);
                        }
                        return sheet;
                    }

                case WriteOptions.CreateNew:
                    {
                        return workbook.CreateSheet(sheetName);
                    }

                default:
                    throw new NotImplementedException();
            }
        }

        private static FileMode GetFileMode(WriteOptions writeOptions)
        {
            switch (writeOptions)
            {
                case WriteOptions.Append: return FileMode.OpenOrCreate;
                case WriteOptions.CreateNew: return FileMode.CreateNew;
                default: throw new NotImplementedException();
            }
        }

        #region 公开接口

        /// <summary>
        /// TableData转成Excel文件
        /// </summary>
        /// <param name="excelPath"></param>
        /// <param name="tableData"></param>
        /// <param name="options"></param>
        /// <param name="sheetName"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static int TableData2Excel(string excelPath, TableData tableData, WriteOptions options, string sheetName = "sheet1", ExcelVersions version = ExcelVersions.Xls)
        {
            FileMode fileMode = GetFileMode(options);

            using (FileStream fs = File.Open(excelPath, fileMode, FileAccess.ReadWrite))
            {
                IWorkbook workbook = OpenWrite(version);
                ISheet sheet = OpenOrCreateSheet(workbook, sheetName, options);
                WriteSheet(sheet, tableData);
                workbook.Write(fs);
                fs.Close();
                return DotNETCode.SUCCESS;
            }
        }

        /// <summary>
        /// Excel文件转换成TableData
        /// </summary>
        /// <param name="excelPath">要转换的Excel文件的完整路径</param>
        /// <returns></returns>
        public static TableData Excel2TableData(string excelPath)
        {
            TableData tableData = TableData.Create();

            using (FileStream fs = File.Open(excelPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                string extention = Path.GetExtension(excelPath);
                IWorkbook workbook = OpenRead(extention, fs);

                ISheet sheet = workbook.GetSheetAt(0);

                // 还剩leftRow行没读取
                int leftRow = sheet.PhysicalNumberOfRows;

                int row = 0;

                // 要注意处理空行
                while (leftRow > 0)
                {
                    IRow irow = sheet.GetRow(row);
                    if (irow == null)
                    {
                        // 该行是空行，不计算在内
                        row++;
                        continue;
                    }

                    int numCell = irow.PhysicalNumberOfCells;

                    for (int col = 0; col < numCell; col++)
                    {
                        ICell icell = irow.GetCell(col);
                        if (icell == null || icell.CellType == CellType.Blank)
                        {
                            // 空的单元格，直接写一个空字符串
                            continue;
                        }

                        switch (icell.CellType)
                        {
                            case CellType.Numeric:
                                {
                                    tableData.Set(row, col, icell.NumericCellValue);
                                    break;
                                }

                            case CellType.String:
                                {
                                    tableData.Set(row, col, icell.StringCellValue);
                                    break;
                                }

                            case CellType.Blank:
                                {
                                    tableData.Set(row, col, string.Empty);
                                    break;
                                }

                            default:
                                logger.ErrorFormat("不支持的Cell数据类型, {0}", icell.CellType);
                                continue;
                        }
                    }

                    leftRow--;
                    row++;
                }

                fs.Close();
            }

            return tableData;
        }

        /// <summary>
        /// Excel文件转换成CSV
        /// </summary>
        /// <param name="excelPath">要转换的Excel文件的完整路径</param>
        /// <param name="csvPath">要保存的CSV文件的完整路径</param>
        public static void Excel2CSV(string excelPath, string csvPath)
        {
            TableData tableData = Excel2TableData(excelPath);
            CSV.TableData2CSV(csvPath, tableData);
        }

        /// <summary>
        /// DataTable转成Excel文件
        /// </summary>
        /// <param name="excelPath"></param>
        /// <param name="table"></param>
        /// <param name="options"></param>
        /// <param name="sheetName"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static int DataTable2Excel(string excelPath, DataTable table, WriteOptions options, string sheetName = "sheet1", ExcelVersions version = ExcelVersions.Xls)
        {
            FileMode fileMode = GetFileMode(options);

            using (FileStream fs = File.Open(excelPath, fileMode, FileAccess.ReadWrite))
            {
                IWorkbook workbook = OpenWrite(version);
                ISheet sheet = OpenOrCreateSheet(workbook, sheetName, options);
                WriteSheet(sheet, table);
                workbook.Write(fs);
                fs.Close();
                return DotNETCode.SUCCESS;
            }
        }

        /// <summary>
        /// 把一个二维数组写到Excel里
        /// </summary>
        /// <param name="excelPath"></param>
        /// <param name="excelData">一维是行，二维是列</param>
        /// <param name="options"></param>
        /// <param name="sheetName"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static int Array2Excel(string excelPath, object[,] excelData, WriteOptions options, string sheetName = "sheet1", ExcelVersions version = ExcelVersions.Xls)
        {
            FileMode fileMode = GetFileMode(options);

            using (FileStream fs = File.Open(excelPath, fileMode, FileAccess.ReadWrite))
            {
                IWorkbook workbook = OpenWrite(version);
                ISheet sheet = OpenOrCreateSheet(workbook, sheetName, options);
                WriteSheet(sheet, excelData);
                workbook.Write(fs);
                fs.Close();
                return DotNETCode.SUCCESS;
            }
        }

        #endregion

        private static IWorkbook OpenRead(string extension, FileStream fs)
        {
            IWorkbook workbook = null;

            // 先根据后缀名判断不同格式的文件并打开

            if (string.Compare(extension, ".xlsx", true) == 0)
            {
                try
                {
                    workbook = new XSSFWorkbook(fs);
                }
                catch (Exception ex)
                {
                    try
                    {
                        workbook = new HSSFWorkbook(fs);
                    }
                    catch (Exception ex1)
                    {
                        logger.Error("打开Excel文件失败", ex1);
                        return null;
                    }
                }
            }
            else if (string.Compare(extension, ".xls", true) == 0)
            {
                try
                {
                    workbook = new HSSFWorkbook(fs);
                }
                catch (Exception ex)
                {
                    try
                    {
                        workbook = new XSSFWorkbook(fs);
                    }
                    catch (Exception ex1)
                    {
                        logger.Error("打开Excel文件失败", ex1);
                        return null;
                    }
                }
            }
            else
            {
                // 扩展名不是excel的扩展名，那么直接打开
                try
                {
                    workbook = new HSSFWorkbook(fs);
                }
                catch (Exception ex)
                {
                    try
                    {
                        workbook = new XSSFWorkbook(fs);
                    }
                    catch (Exception ex1)
                    {
                        logger.Error("打开Excel文件失败", ex1);
                        return null;
                    }
                }
            }

            // 到这里打开成功了
            return workbook;
        }

        private static IWorkbook OpenWrite(ExcelVersions version)
        {
            switch (version)
            {
                case ExcelVersions.Xls:
                    {
                        return new HSSFWorkbook();
                    }

                case ExcelVersions.Xlsx:
                    {
                        return new XSSFWorkbook();
                    }

                default:
                    throw new NotSupportedException("不支持的Excel格式");
            }
        }

        private static ICell CreateCell(IRow row, int cellIndex, object value)
        {
            Type valueType = value.GetType();
            if (valueType == typeof(int))
            {
                ICell cell = row.CreateCell(cellIndex, CellType.Numeric);
                cell.SetCellValue((int)value);
                return cell;
            }
            else if (valueType == typeof(float))
            {
                ICell cell = row.CreateCell(cellIndex, CellType.Numeric);
                cell.SetCellValue((float)value);
                return cell;
            }
            else if (valueType == typeof(double))
            {
                ICell cell = row.CreateCell(cellIndex, CellType.Numeric);
                cell.SetCellValue((double)value);
                return cell;
            }
            else if (valueType == typeof(string))
            {
                ICell cell = row.CreateCell(cellIndex, CellType.String);
                cell.SetCellValue(value.ToString());
                return cell;
            }
            else if (valueType == typeof(DateTime))
            {
                ICell cell = row.CreateCell(cellIndex, CellType.String);
                cell.SetCellValue((DateTime)value);
                return cell;
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
