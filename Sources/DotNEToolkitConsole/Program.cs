using DotNEToolkit;
using DotNEToolkit.Crypto;
using DotNEToolkit.DataAccess;
using DotNEToolkit.Expressions;
using DotNEToolkit.Media;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace DotNEToolkitConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            DotNEToolkit.Log4net.InitializeLog4net();

            TableData tableData = CSV.CSVFile2TableData("2718BOT-2.csv");
            Excel.TableData2Excel(tableData, "1.xls");

            Console.WriteLine("运行结束...");

            Console.ReadLine();
        }
    }
}
