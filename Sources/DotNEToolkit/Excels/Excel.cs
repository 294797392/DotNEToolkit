using DotNEToolkit.Excels;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace DotNEToolkit
{
    /// <summary>
    /// 封装Excel文件的导入，导出功能
    /// </summary>
    public static class Excel
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger("Excel");

        public static int QuickWrite()
        {
            return DotNETCode.NotSupported;
        }

        /// <summary>
        /// 把ExcelSheet对象保存成Excel文件
        /// </summary>
        /// <param name="excelSheet"></param>
        /// <param name="filePath"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static int Write(ExcelSheet excelSheet, string filePath, ExcelVersions version = ExcelVersions.Xls)
        {
            using (FileStream fs = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                IWorkbook workbook = OpenWrite(version, fs);
                ISheet sheet = workbook.CreateSheet("sheet1");

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
                return DotNETCode.Success;
            }
        }

        /// <summary>
        /// 把excel文件转成内存对象
        /// </summary>
        /// <param name="options">读取Excel文件的选项</param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static int Read(string filePath, ReadOptions options, out ExcelSheet excelSheet)
        {
            excelSheet = null;

            if (!File.Exists(filePath))
            {
                return DotNETCode.FileNotFound;
            }

            using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read))
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
                        if (cell == null)
                        {
                            // 空列，根据选项进行处理
                            if (options.HasFlag(ReadOptions.IgnoreEmptyCell))
                            {

                            }
                            else if (options.HasFlag(ReadOptions.RetainEmptyCell))
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

                            default:
                                logger.ErrorFormat("不支持的Cell数据类型, {0}", cell.CellType);
                                return DotNETCode.NotSupported;
                        }
                    }

                    excelSheet.AddRow(excelRow);

                    leftRow--;
                    currentRow++;
                }

                fs.Close();
            }

            return DotNETCode.Success;
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

        private static IWorkbook OpenWrite(ExcelVersions version, FileStream fs)
        {
            switch (version)
            {
                case ExcelVersions.Xls:
                    {
                        return new HSSFWorkbook(fs);
                    }

                case ExcelVersions.Xlsx:
                    {
                        return new XSSFWorkbook(fs);
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

                default:
                    throw new NotSupportedException("不支持的数据格式");
            }
        }
    }
}
