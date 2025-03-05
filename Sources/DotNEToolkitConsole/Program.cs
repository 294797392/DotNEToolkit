using DotNEToolkit;
using DotNEToolkit.Crypto;
using DotNEToolkit.DataAccess;
using DotNEToolkit.Expressions;
using DotNEToolkit.Media;
using DotNEToolkit.Modular;
using DotNEToolkit.Utility;
using Factory.NET;
using Factory.NET.Modules;
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
        private static log4net.ILog logger = log4net.LogManager.GetLogger("Program");

        static void Add(ZDotCH2221HOutputModule outputModule)
        {
            byte start = 0x80;

            while (true)
            {
                byte[] bytes = new byte[] { start++, 0x00, 0x00, 0x00, };
                string hex = ByteUtils.ToString(bytes, " ", HexNumberOptions.WithPrefix);
                logger.InfoFormat(hex);
                outputModule.WriteCoils(192, 32, bytes);
                logger.InfoFormat("按回车键测量下一个");
                Console.ReadLine();
            }
        }

        static void Add2(ZDotCH2221HOutputModule outputModule)
        {
            byte[] values = new byte[] { 0x80, 0x90, 0xA0, 0xB0, 0xC0, 0xD0, 0xE0, 0xF0 };

            byte start = 0;

            while (true)
            {
                byte[] bytes = new byte[] { values[start++], 0x00, 0x00, 0x00, };
                string hex = ByteUtils.ToString(bytes, " ", HexNumberOptions.WithPrefix);
                logger.InfoFormat(hex);
                outputModule.WriteCoils(192, 32, bytes);
                logger.InfoFormat("按回车键测量下一个");
                Console.ReadLine();
            }
        }

        static void Add3(ZDotCH2221HOutputModule outputModule)
        {
            byte start = 0x01;

            while (true)
            {
                byte[] bytes = new byte[] {  0x00,  0x00, start, 0x00,  };
                //byte[] bytes = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, };
                string hex = ByteUtils.ToString(bytes, " ", HexNumberOptions.WithPrefix);
                logger.InfoFormat(hex);
                outputModule.WriteCoils(224, 32, bytes);
                logger.InfoFormat("按回车键测量下一个");
                Console.ReadLine();
                start *= 2;
            }
        }


        static void Main(string[] args)
        {
            Log4net.InitializeLog4net();

            ModuleFactoryOptions moduleFactoryOptions = new ModuleFactoryOptions()
            {
                ModuleList = JSONHelper.File2Object<List<ModuleDefinition>>("demo.json")
            };

            ModuleFactory moduleFactory = ModuleFactory.CreateFactory(moduleFactoryOptions);
            moduleFactory.Initialize();
            ZDotCH2221HOutputModule outputModule = moduleFactory.LookupModule<ZDotCH2221HOutputModule>();

            Add3(outputModule);

            Console.WriteLine(System.Diagnostics.Process.GetCurrentProcess().Id);

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
