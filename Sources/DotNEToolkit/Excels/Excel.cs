using DotNEToolkit.Excels;
using DotNEToolkit.Excels.Attributes;
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

        public static int QuickWrite(string filePath, DataTable table, string sheetName = "sheet1", ExcelVersions version = ExcelVersions.Xls)
        {
            using (FileStream fs = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                IWorkbook workbook = OpenWrite(version);
                ISheet sheet = workbook.CreateSheet(sheetName);
                WriteSheet(sheet, table);
                workbook.Write(fs);
                fs.Close();
                return DotNETCode.SUCCESS;
            }
        }

        /// <summary>
        /// 把一个二维数组写到Excel里
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="excelData">一维是行，二维是列</param>
        /// <param name="sheetName"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static int QuickWrite(string filePath, object[,] excelData, string sheetName = "sheet1", ExcelVersions version = ExcelVersions.Xls)
        {
            using (FileStream fs = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                IWorkbook workbook = OpenWrite(version);
                ISheet sheet = workbook.CreateSheet(sheetName);
                WriteSheet(sheet, excelData);
                workbook.Write(fs);
                fs.Close();
                return DotNETCode.SUCCESS;
            }
        }

        public static int QuickWrite(string filePath, object[,] firstWrite, DataTable secondWrite, string sheetName = "sheet1", ExcelVersions version = ExcelVersions.Xls)
        {
            using (FileStream fs = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                IWorkbook workbook = OpenWrite(version);
                ISheet sheet = workbook.CreateSheet(sheetName);
                WriteSheet(sheet, firstWrite);
                WriteSheet(sheet, secondWrite);
                workbook.Write(fs);
                fs.Close();
                return DotNETCode.SUCCESS;
            }
        }

        public static int QuickWrite(ExcelSheet excelSheet, string filePath, ExcelVersions version = ExcelVersions.Xls)
        {
            using (FileStream fs = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                IWorkbook workbook = OpenWrite(version);
                ISheet sheet = workbook.CreateSheet(string.IsNullOrEmpty(excelSheet.Name) ? "sheet1" : excelSheet.Name);

                for (int i = 0; i < excelSheet.Rows.Count; i++)
                {
                    IRow row = sheet.CreateRow(i);

                    ExcelRow excelRow = excelSheet.Rows[i];

                    for (int j = 0; j < excelRow.Cells.Count; j++)
                    {
                        ExcelCell excelCell = excelRow.GetCell(j);

                        ICell cell = CreateCell(row, j, excelCell);
                    }
                }

                workbook.Write(fs);
                fs.Close();
                return DotNETCode.SUCCESS;
            }
        }

        public static int QuickRead(string filePath, ReadOptions options, out ExcelSheet excelSheet)
        {
            excelSheet = null;

            if (!File.Exists(filePath))
            {
                return DotNETCode.FILE_NOT_FOUND;
            }

            using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                string extention = Path.GetExtension(filePath);
                IWorkbook workbook = OpenRead(extention, fs);

                ISheet sheet = workbook.GetSheetAt(0);

                // 还剩leftRow行没读取
                int leftRow = sheet.PhysicalNumberOfRows;

                excelSheet = new ExcelSheet();
                excelSheet.Name = sheet.SheetName;
                int currentRow = 0;

                // 要注意处理空行
                while (leftRow > 0)
                {
                    IRow row = sheet.GetRow(currentRow);
                    if (row == null)
                    {
                        // 该行是空行，不计算在内
                        currentRow++;
                        continue;
                    }

                    int numCell = row.PhysicalNumberOfCells;

                    ExcelRow excelRow = new ExcelRow();

                    for (int cellnum = 0; cellnum < numCell; cellnum++)
                    {
                        ICell cell = row.GetCell(cellnum);
                        if (cell == null || cell.CellType == CellType.Blank)
                        {
                            // 空列，根据选项进行处理
                            if (options.HasFlag(ReadOptions.IgnoreEmptyCell))
                            {

                            }
                            else if (options.HasFlag(ReadOptions.KeepEmptyCell))
                            {
                                excelRow.AddCell(null, ExcelCellTypes.Null);
                            }
                            continue;
                        }

                        switch (cell.CellType)
                        {
                            case CellType.Numeric:
                                {
                                    excelRow.AddCell(cell.NumericCellValue, ExcelCellTypes.Numberic);
                                    break;
                                }

                            case CellType.String:
                                {
                                    excelRow.AddCell(cell.StringCellValue, ExcelCellTypes.String);
                                    break;
                                }

                            case CellType.Blank:
                                {
                                    // 空列，在107行进行了处理
                                    break;
                                }

                            default:
                                logger.ErrorFormat("不支持的Cell数据类型, {0}", cell.CellType);
                                return DotNETCode.NOT_SUPPORTED;
                        }
                    }

                    excelSheet.AddRow(excelRow);

                    leftRow--;
                    currentRow++;
                }

                fs.Close();
            }

            return DotNETCode.SUCCESS;
        }

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

        private static ICell CreateCell(IRow row, int cellIndex, ExcelCell excelCell)
        {
            switch (excelCell.Type)
            {
                case ExcelCellTypes.Numberic:
                    {
                        ICell cell = row.CreateCell(cellIndex, CellType.Numeric);
                        cell.SetCellValue((double)excelCell.Value);
                        return cell;
                    }

                case ExcelCellTypes.String:
                    {
                        ICell cell = row.CreateCell(cellIndex, CellType.String);
                        cell.SetCellValue(excelCell.Value.ToString());
                        return cell;
                    }

                case ExcelCellTypes.Null:
                    {
                        ICell cell = row.CreateCell(cellIndex, CellType.Blank);
                        return cell;
                    }

                default:
                    throw new NotSupportedException("不支持的数据格式");
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
