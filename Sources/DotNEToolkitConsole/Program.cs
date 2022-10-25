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
            Console.WriteLine(System.Diagnostics.Process.GetCurrentProcess().Id);

            DotNEToolkit.Log4net.InitializeLog4net();

            //TestExcel.CreateSpan();
            TestExcel.CreateOrAppend();

            //TestExcel.CreateNew();
            //while (true)
            //{
            //    TestExcel.CreateOrAppend();
            //    Console.WriteLine("请输入回车键继续Append");
            //    Console.ReadLine();
            //}

            //TestFilePackage.PackDirectory("Expressions", "1.pkg");

            //TableData tableData = CSV.CSVFile2TableData("2718BOT-2.csv");
            //Excel.TableData2Excel(tableData, "1.xls");

            //while (true)
            //{
            //    TestFilePackage.PackFile("DotNEToolkit.dll", "1.zip");
            //    System.Threading.Thread.Sleep(2000);
            //}

            Console.WriteLine("运行结束...");

            Console.ReadLine();
        }
    }
}
