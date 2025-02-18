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
    }
}
