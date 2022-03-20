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

            if (args.Length != 2)
            {
                Usage();
                Console.WriteLine("参数不正确");
                Exit();
            }

            string arguments = args[1];

            Console.WriteLine("arguments:");
            Console.WriteLine(arguments);

            // 解析参数
            IDictionary parameters = JSONHelper.Parse<IDictionary>(arguments, null);
            if (parameters == null)
            {
                Console.WriteLine("参数格式不正确");
                Exit();
            }

            // 创建IHostedModule实例
            IHostedModule hostedModule = null;
            string moduleEntry = parameters.GetValue<string>(KEY_MODULE_ENTRY);
            try
            {
                hostedModule = ConfigFactory<IHostedModule>.CreateInstance(moduleEntry);
                logger.InfoFormat("创建HostedModule成功");
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("创建HostedModule异常", ex);
                Console.WriteLine(DotNETCode.CREATE_HOSTED_MODULE_FAILED);
                return;
            }

            // 初始化HostedModule
            int code = hostedModule.Initialize(parameters);
            if (code != DotNETCode.SUCCESS)
            {
                Console.WriteLine(DotNETCode.INIT_HOSTED_MODULE_FAILED);
                return;
            }

            Console.WriteLine(DotNETCode.SUCCESS);
            Console.ReadLine();
        }
    }
}
