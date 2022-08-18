using DotNEToolkit;
using DotNEToolkit.Crypto;
using DotNEToolkit.DataAccess;
using DotNEToolkit.Expressions;
using DotNEToolkit.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DotNEToolkitConsole
{
    class Program
    {
        private static void ConnectMysql()
        {
            DBManager dbManager = new DBManager("Server=127.0.0.1;Database=sys;Uid=root;Pwd=123456;", ProviderType.MySql);
            if (dbManager.TestConnection())
            {
                Console.WriteLine("OK");
            }
            else
            {
                Console.WriteLine("Fail");
            }
        }

        static void Main(string[] args)
        {
            DotNEToolkit.Log4net.InitializeLog4net();

            TestImageCodec.TestSpeed();

            //TestFilePackage.WriteOnce();
            //TestFilePackage.WriteMulit();

            Console.WriteLine("运行结束...");

            Console.ReadLine();
        }

        private static void Installer_Progress(MySQLInstaller installer, double progress)
        {
            Console.WriteLine("安装进度:{0}", progress);
        }
    }
}
