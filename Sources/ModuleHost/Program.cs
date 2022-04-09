using DotNEToolkit;
using DotNEToolkit.Modular;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleHost
{
    class Program
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger("ModuleHost");

        private const string KEY_MODULE_ENTRY = "moduleEntry";

        static void Usage()
        {
            
        }

        static void Exit()
        {
            Console.ReadLine();
            Process.GetCurrentProcess().Kill();
        }

        static void Main(string[] args)
        {
            Log4net.InitializeLog4net();

            if (args.Length != 1)
            {
                Usage();
                Console.WriteLine("参数不正确, 参数数量 = {0}", args.Length);
                Exit();
            }

            string arguments = args[0];

            // 解析参数
            IDictionary parameters = JSONHelper.ParseFile<Dictionary<string, object>>(arguments, null);
            if (parameters == null)
            {
                Console.WriteLine("参数格式不正确, 参数 = {0}", arguments);
                Exit();
            }

            // 创建IHostedModule实例
            IHostedModule hostedModule = null;
            string moduleEntry = parameters.GetValue<string>(KEY_MODULE_ENTRY);
            try
            {
                hostedModule = ConfigFactory<IHostedModule>.CreateInstance(moduleEntry);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("创建HostedModule异常", ex);
                Console.WriteLine(DotNETCode.CREATE_HOSTED_MODULE_FAILED.ToString());
                return;
            }

            // 初始化HostedModule
            int code = hostedModule.Initialize(parameters);
            if (code != DotNETCode.SUCCESS)
            {
                Console.WriteLine(DotNETCode.INIT_HOSTED_MODULE_FAILED.ToString());
                return;
            }

            Console.WriteLine(DotNETCode.SUCCESS.ToString());
            Console.ReadLine();
        }
    }
}
