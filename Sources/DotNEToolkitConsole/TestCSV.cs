using DotNEToolkit;
using DotNEToolkit.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkitConsole
{
    public class TestCSV
    {
        public class Msg_log
        {
            //PLC报警ID
            [TableColumn("AlarmID")]
            public string id_msg { get; set; }

            public string id_layout { get; set; }
            //PLC报警类型
            [TableColumn("AlarmType")]
            public string module_name { get; set; }
            //PLC报警名称
            [TableColumn("DescriptionCN")]
            public string label_name { get; set; }
            //66
            [TableColumn("Description")]
            public string state { get; set; }
            //8
            [TableColumn("AlarmLevel")]
            public string automatic_mode { get; set; }

            public DateTime starttime { get; set; }

            public DateTime? endtime { get; set; }
        }

        public static void CSVFile2Objects()
        {
            //List<Msg_log> list = CSVUtils.CSVFile2Objects<Msg_log>("1.csv");
            //Console.WriteLine();
        }

        public static void TableData2CSVFile()
        {
            TableData tableData = TableData.Create();

            for (int i = 0; i < 500; i++)
            {
                for (int j = 0; j < 500; j++)
                {
                    tableData.SetCell(i, j, string.Format("{0}_{1}", i, j));
                }
            }

            CSVUtils.TableData2CSVFile(tableData, "1.csv");
        }

        public static void TableData2CSVFile3()
        {
            TableData tableData = CSVUtils.CSVFile2TableData("1.csv", new string[] { "," }, Encoding.GetEncoding("GB2312"));

            int cols = tableData.GetColumns(0);

            for (int row = 0; row < 10; row++)
            {
                int rows = tableData.GetRows();

                for (int col = 0; col < cols; col++)
                {
                    int newRow = rows;

                    tableData.SetCell(newRow, col, string.Format("{0}_{1}", newRow, col));
                }
            }

            CSVUtils.TableData2CSVFile(tableData, "1.csv");
        }

        public static void TableData2CSVFile2()
        {
            TableData tableData = TableData.Create();

            tableData.SetCell(10, 10, "10_10");
            tableData.SetCell(0, 0, "0_0");
            tableData.SetCell(5, 5, "5_5");

            CSVUtils.TableData2CSVFile(tableData, "2.csv");
        }
    }
}
