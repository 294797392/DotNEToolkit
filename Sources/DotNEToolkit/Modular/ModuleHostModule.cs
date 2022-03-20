using DotNEToolkit.ProcessComm;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Modular
{
    /// <summary>
    /// 表示一个要在其他进程运行的模块
    /// 在ModuleHost里会通过反射机制创建IHostedModule的实例并调用Initialize方法
    /// </summary>
    public interface IHostedModule
    {
        /// <summary>
        /// 初始化该模块
        /// </summary>
        /// <param name="parameters">该模块的参数</param>
        /// <returns></returns>
        int Initialize(IDictionary parameters);

        /// <summary>
        /// 释放该模块所占用的资源
        /// </summary>
        void Release();
    }

    /// <summary>
    /// 一个实现了进程间通信的HostedModule
    /// </summary>
    public abstract class AbstractHostedModule : IHostedModule
    {
        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("AbstractHostedModule");

        /// <summary>
        /// 进程间通信方式
        /// </summary>
        private const string KEY_COMM_TYPE = "commType";

        #endregion

        #region 实例变量

        private ProcessCommSvc commSvc;

        #endregion

        #region IHostedModule

        public int Initialize(IDictionary parameters)
        {
            ProcessCommTypes commType = parameters.GetValue<ProcessCommTypes>(KEY_COMM_TYPE);
            this.commSvc = ProcessCommFactory.CreateSvc(commType);
            this.commSvc.URI = parameters.GetValue<string>("uri");
            this.commSvc.DataReceived += this.CommSvc_DataReceived1; ;
            this.commSvc.Initialize();
            return this.commSvc.Start();
        }

        public void Release()
        {
            this.commSvc.Release();
        }

        #endregion

        #region 抽象方法

        /// <summary>
        /// 当收到客户端进程发过来的消息的时候被调用
        /// </summary>
        /// <param name="data"></param>
        public abstract void OnClientDataReceived(int cmdType, object cmdParam);

        #endregion

        #region 实例方法

        /// <summary>
        /// 给外部进程发消息
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public int Send(int cmdType, byte[] cmdParam)
        {
            return this.commSvc.Send(cmdType, cmdParam);
        }

        /// <summary>
        /// 给外部进程发消息
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public int Send(int cmdType, string cmdParam)
        {
            return this.commSvc.Send(cmdType, cmdParam);
        }

        /// <summary>
        /// 给外部进程发消息
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public int Send(int cmdType, object cmdParam)
        {
            return this.commSvc.Send(cmdType, cmdParam);
        }

        #endregion

        #region 事件处理器

        private void CommSvc_DataReceived1(ProcessCommObject svc, int cmdType, object cmdParam)
        {
            this.OnClientDataReceived(cmdType, cmdParam);
        }

        #endregion
    }

    /// <summary>
    /// 表示一个独立进程的模块
    /// </summary>
    public class ModuleHostModule : ModuleBase
    {
        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("ModuleHostModule");

        private const string KEY_MODULEHOST_PATH = "moduleHost";
        private const string KEY_MODULE_ENTRY = "moduleEntry";
        private static readonly string DefaultModuleHostPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ModuleHost.exe");

        #endregion

        #region 实例变量

        private string moduleHostPath;
        private Process moduleHostProc;
        private string moduleEntry;

        #endregion

        #region ModuleBase

        protected override int OnInitialize()
        {
            this.moduleEntry = this.InputParameters.GetValue<string>(KEY_MODULE_ENTRY, string.Empty);
            if (string.IsNullOrEmpty(this.moduleEntry))
            {
                logger.Error("未配置ModuleEntry");
                return DotNETCode.INVALID_PARAMS;
            }

            // 直接把JSON配置文件里的参数透传给ModuleHost
            string arguments = JsonConvert.SerializeObject(this.InputParameters);
            this.moduleHostProc = this.SetupModuleHostProcess(this.moduleHostPath, arguments);
            if (this.moduleHostProc == null)
            {
                logger.ErrorFormat("创建ModuleHost进程异常", this.moduleHostPath);
                return DotNETCode.CREATE_PROC_FAILED;
            }

            // 读取ModuleHost的返回值判断是否启动成功
            string result = this.moduleHostProc.StandardOutput.ReadLine();
            if (result != DotNETCode.SUCCESS.ToString())
            {
                logger.ErrorFormat("HostedModule初始化失败, code = {0}", result);
                this.CloseModuleHostProcess();
                return DotNETCode.INIT_HOSTED_MODULE_FAILED;
            }

            // 注册进程的退出事件
            // 如果进程崩溃了会收到这个事件，然后可以进行下一步的处理（比如重启崩溃进程）
            this.moduleHostProc.Exited += this.Process_Exited;

            // 此时ModuleHost已经在运行了

            return DotNETCode.SUCCESS;
        }

        protected override void OnRelease()
        {
            this.CloseModuleHostProcess();

            base.OnRelease();
        }

        #endregion

        #region 实例方法

        private Process SetupModuleHostProcess(string moduleHostPath, string arguments) 
        {
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = moduleHostPath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
            };

            try
            {
                Process process = new Process();
                process.StartInfo = startInfo;
                process.Start();
                return process;
            }
            catch (Exception ex)
            {
                logger.Error("启动ModuleHost进程异常", ex);
                return null;
            }
        }

        private void CloseModuleHostProcess()
        {
            try
            {
                this.moduleHostProc.Exited -= this.Process_Exited;
                this.moduleHostProc.Kill();
                this.moduleHostProc.Dispose();
                this.moduleHostProc = null;
            }
            catch (Exception ex)
            {
                logger.Error("关闭ModuleHost进程异常", ex);
            }
        }

        #endregion

        #region 事件处理器

        private void Process_Exited(object sender, EventArgs e)
        {
            // todo：如果ModuleHost异常退出（崩溃），那么增加重新启动机制

            Process process = sender as Process;
            logger.InfoFormat("{0}, 进程已退出, exitCode = {1}, exitTime = {2}", this.Name, process.ExitCode, process.ExitTime);
        }

        #endregion
    }
}