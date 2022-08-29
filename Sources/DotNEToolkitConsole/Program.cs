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
    public abstract class A
    {
        public A()
        {
            Type t = this.GetType();

            Console.WriteLine();
        }
    }

    public class B : A
    {

    }

    class Program
    {
        public void C()
        {
        }

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

            B b = new B();

            //MySQLInstaller installer = new MySQLInstaller(@"E:\mysql\mysql-8.0.26-winx64");
            //installer.Install();

            //Console.WriteLine("结束");

            //Console.ReadLine();

            //TestBytePool.Test();
            //TestImageCodec.TestSpeed();
            //TestFilePackage.WriteOnce();
            //TestFilePackage.WriteMulit();
            //TestFilePackage.PackFile(@"1.bmp", "1.tar");
            TestFilePackage.PackFile(@"1.bmp", @"image", "1.tar");

            Console.WriteLine("运行结束...");

            Console.ReadLine();
        }

        private static void Installer_Progress(MySQLInstaller installer, double progress)
        {
            Console.WriteLine("安装进度:{0}", progress);
        }
    }
}
