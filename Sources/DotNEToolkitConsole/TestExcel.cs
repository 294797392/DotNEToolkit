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
        public class Msg_log
        {
            public string id_msg { get; set; }
            [TableColumn("AlarmID")]
            public int AlarmID { get; set; }
            public string id_layout { get; set; }
            [TableColumn("Description")]
            public string module_name { get; set; }
            [TableColumn("DescriptionCN")]
            public string label_name { get; set; }
            [TableColumn("state")]
            public int state { get; set; }
            [TableColumn("AlarmType")]
            public string automatic_mode { get; set; }
        }

        public class AlarmItem
        {
            [TableColumn("ModuleName", Width = 8000)]
            public string ModuleName { get; set; }

            [TableColumn("Label", Width = 8000)]
            public string Label { get; set; }

            [TableColumn("StartTime", Width = 8000)]
            public string DisplayStartTime { get; set; }

            [TableColumn("EndTime", Width = 8000)]
            public string DisplayEndTime { get; set; }

            [TableColumn("UsedTime", Width = 8000)]
            public string UsedTime { get; set; }
        }

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
            ExcelUtils.TableData2ExcelFile(excelPath, tableData, WriteOptions.CreateNew);
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
            ExcelUtils.TableData2ExcelFile(excelPath, tableData, WriteOptions.Append);
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

            ExcelUtils.TableData2ExcelFile(excelPath, tableData, WriteOptions.Append);
        }

        public static void ExcelFile2Objects()
        {
            List<Msg_log> list = ExcelUtils.ExcelFile2Objects<Msg_log>("123.xls");
            Console.WriteLine();
        }

        public static void Objects2ExcelFile()
        {
            List<AlarmItem> messageItems = new List<AlarmItem>();
            for (int i = 0; i < 11000; i++)
            {
                messageItems.Add(new AlarmItem()
                {
                    DisplayStartTime = DateTime.Now.ToString(),
                    DisplayEndTime = DateTime.Now.ToString(),
                    Label = Guid.NewGuid().ToString(),
                    ModuleName = Guid.NewGuid().ToString(),
                    UsedTime = Guid.NewGuid().ToString()
                });
            }

            ExcelUtils.Objects2ExcelFile<AlarmItem>(messageItems, "1.xls", WriteOptions.Append, "Message");
        }

        public static void Array2Excel()
        {
            List<AlarmItem> messageItems = new List<AlarmItem>();
            for (int i = 0; i < 11000; i++)
            {
                messageItems.Add(new AlarmItem()
                {
                    DisplayStartTime = DateTime.Now.ToString(),
                    DisplayEndTime = DateTime.Now.ToString(),
                    Label = Guid.NewGuid().ToString(),
                    ModuleName = Guid.NewGuid().ToString(),
                    UsedTime = Guid.NewGuid().ToString()
                });
            }


            object[,] datas = new object[20000, 5];
            for (int i = 0; i < 20000; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    datas[i, j] = Guid.NewGuid().ToString();
                }
            }

            ExcelUtils.Array2Excel("1.xls", datas, WriteOptions.Append, "Message");
        }
    }
}
