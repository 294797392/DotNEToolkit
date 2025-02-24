using DotNEToolkit;
using DotNEToolkit.Crypto;
using DotNEToolkit.DataAccess;
using DotNEToolkit.Expressions;
using DotNEToolkit.Media;
using DotNEToolkit.Modular;
using DotNEToolkit.Utility;
using Factory.NET;
using Factory.NET.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace DotNEToolkitConsole
{
    class Program
    {
        public class TestItem
        {
            public void Run() { }
        }

        static void Main(string[] args)
        {
            List<TestItem> testItems = new List<TestItem>();
            for (int i = 0; i < 19; i++)
            {
                testItems.Add(new TestItem());
            }

            foreach (TestItem testItem in testItems)
            {
                testItem.Run();
            }

            //






            Console.WriteLine(System.Diagnostics.Process.GetCurrentProcess().Id);

            DotNEToolkit.Log4net.InitializeLog4net();

            Console.WriteLine(byte.MaxValue);

            Console.ReadLine();

            byte[] buffer = new byte[] { 0xAA, 0x2A, 0x20, 0x4E };
            int v = 0;
            for (int i = 0; i < buffer.Length; i++)
            {
                v += buffer[i];
            }

            Console.WriteLine(v % 256);
            Console.ReadLine();

            //string content;
            //FactoryUtils.AdbReadFile("adb.exe", "/etc/version.conf", "123", out content);
            //Console.WriteLine(content);

            Console.ReadLine();

            //TestExcel.Array2Excel();

            //TestTimer.CreateTimer();

            //TestCSV.CSVFile2Objects();
            //TestExcel.ExcelFile2Objects();

            //List<IPAddress> addresses = NetworkUtils.GetBroadcastAddresses();
            //foreach (IPAddress address in addresses)
            //{
            //    Console.WriteLine(address.ToString());
            //}

            //TestRecord.RecordAudio();
            //TestAudioPlay.libvlcPlay();

            //TestExcel.CreateSpan();
            //TestExcel.CreateOrAppend();

            //TestExcel.CreateNew();
            //while (true)
            //{
            //    TestExcel.CreateOrAppend();
            //    Console.WriteLine("请输入回车键继续Append");
            //    Console.ReadLine();
            //}

            //TestFilePackage.PackDirectory("E:/4", "1.zip");

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
