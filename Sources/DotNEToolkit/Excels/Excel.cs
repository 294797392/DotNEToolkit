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

        /// <summary>
        /// 把ExcelSheet对象保存成Excel文件
        /// </summary>
        /// <param name="excelSheet"></param>
        /// <param name="filePath"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static int Write<TRow>(string filePath, IList<TRow> dataList, ExcelVersions version = ExcelVersions.Xls) where TRow : IExcelRow
        {
            List<PropertyAttribute<ExcelColumnAttribute>> properties = Reflections.GetPropertyAttribute<ExcelColumnAttribute>(typeof(TRow));
            if (properties.Count == 0)
            {
                return DotNETCode.SUCCESS;
            }

            ExcelSheet sheet = new ExcelSheet();

            // 先写第一行，相当于是标题
            ExcelRow titleRow = new ExcelRow();
            IEnumerable<ExcelColumnAttribute> columns = properties.Select(v => v.Attribute);
            foreach (ExcelColumnAttribute column in columns)
            {
                titleRow.AddCell(column.Name, ExcelCellTypes.String);
            }
            sheet.AddRow(titleRow);

            foreach (TRow row in dataList)
            {
                ExcelRow excelRow = new ExcelRow();

                foreach (PropertyAttribute<ExcelColumnAttribute> property in properties)
                {
                    // 当前要写入的列
                    object value = property.Property.GetValue(row, null);

                    excelRow.AddCell(value, property.Attribute.Type);
                }

                sheet.AddRow(excelRow);
            }

            return QuickWrite(sheet, filePath, version);
        }

        /// <summary>
        /// 把excel文件转成内存对象
        /// </summary>
        /// <param name="options">读取Excel文件的选项</param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static int Read<TRow>(string filePath, ReadOptions options, out List<TRow> rows) where TRow : IExcelRow
        {
            throw new NotImplementedException();
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
    }
}
