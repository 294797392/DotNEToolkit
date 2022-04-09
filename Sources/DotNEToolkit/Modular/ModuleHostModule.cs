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
    /// 运行在ModuleHosted里的模块
    /// 运行在外部进程的模块基类
    /// 如果你的模块想运行在外部进程，那么可以实现这个抽象类
    /// 这个类实现了与主进程通信的功能
    /// 调用者只需要关心主进程发送过来的命令并处理就可以了
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
            int code = DotNETCode.SUCCESS;

            ProcessCommTypes commType = parameters.GetValue<ProcessCommTypes>(KEY_COMM_TYPE);
            this.commSvc = ProcessCommFactory.CreateSvc(commType);
            this.commSvc.URI = parameters.GetValue<string>("uri");
            this.commSvc.DataReceived += this.CommSvc_DataReceived;
            if ((code = this.commSvc.Initialize()) != DotNETCode.SUCCESS)
            {
                logger.ErrorFormat("初始化commSvc失败, code = {0}", code);
                return code;
            }

            if ((code = this.commSvc.Start()) != DotNETCode.SUCCESS)
            {
                logger.ErrorFormat("启动commSvc失败, code = {0}", code);
                return code;
            }

            return this.OnInitialize(parameters);
        }

        public void Release()
        {
            this.commSvc.Release();

            this.OnRelease();
        }

        #endregion

        #region 抽象方法

        /// <summary>
        /// 当收到客户端进程发过来的消息的时候被调用
        /// </summary>
        /// <param name="cmdType"></param>
        /// <param name="cmdParam"></param>
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

        private void CommSvc_DataReceived(ProcessCommObject svc, int cmdType, object cmdParam)
        {
            this.OnClientDataReceived(cmdType, cmdParam);
        }

        #endregion

        /// <summary>
        /// 初始化子类
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected abstract int OnInitialize(IDictionary parameters);

        /// <summary>
        /// 释放子类占用的资源
        /// </summary>
        protected abstract void OnRelease();
    }

    /// <summary>
    /// 一个专门运行ModuleHost的模块
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

        /// <summary>
        /// 初始化ModuleHost
        /// </summary>
        /// <returns></returns>
        protected override int OnInitialize()
        {
            this.moduleEntry = this.GetInputValue<string>(KEY_MODULE_ENTRY, string.Empty);
            if (string.IsNullOrEmpty(this.moduleEntry))
            {
                logger.Error("未配置ModuleEntry");
                return DotNETCode.INVALID_PARAMS;
            }

            // 读取ModuleHost.exe的路径
            this.moduleHostPath = this.GetInputValue<string>(KEY_MODULEHOST_PATH, DefaultModuleHostPath);

            // 直接把JSON配置文件里的参数透传给ModuleHost
            // 把JSON参数写到一个文件里, 然后把文件名通过参数传递给子进程
            string json = JsonConvert.SerializeObject(this.InputParameters);
            string fileName = string.Format("moduleHost_{0}.json", this.Name);
            File.WriteAllText(fileName, json);
            this.moduleHostProc = this.SetupModuleHostProcess(this.moduleHostPath, fileName);
            if (this.moduleHostProc == null)
            {
                logger.ErrorFormat("创建ModuleHost进程异常", this.moduleHostPath);
                return DotNETCode.CREATE_PROC_FAILED;
            }

            // 读取ModuleHost的返回值判断是否启动成功
            while (true)
            {
                string line = this.moduleHostProc.StandardOutput.ReadLine();
                int code;
                if (!int.TryParse(line, out code))
                {
                    // 这里输出ModuleHost的日志
                    //logger.InfoFormat("ModuleHost:{0}", line);
                    continue;
                }

                if (code != DotNETCode.SUCCESS)
                {
                    logger.ErrorFormat("HostedModule初始化失败, code = {0}", code);
                    this.CloseModuleHostProcess();
                    return DotNETCode.INIT_HOSTED_MODULE_FAILED;
                }
                else
                {
                    break;
                }
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

    /// <summary>
    /// 一个用来和ModuleHost进行通信的代理模块
    /// 该类维护与远程模块的连接状态
    /// 调用者只需要关心子进程发送过来的命令并处理就可以了
    /// 主要的函数：OnDataReceived - 收到子进程发送的消息的时候触发
    /// </summary>
    public abstract class ModuleHostProxy : ServiceModule
    {
        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("ModuleHostProxy");

        private const string KEY_COMM_TYPE = "commType";
        private const string KEY_URI = "uri";

        #endregion

        #region 实例变量

        /// <summary>
        /// 用来和ModuleHost模块进行通信的客户端
        /// </summary>
        private ProcessCommClient commClient;

        #endregion

        #region ServiceModule

        protected override int OnInitialize()
        {
            ProcessCommTypes commType = this.GetInputValue<ProcessCommTypes>(KEY_COMM_TYPE);
            string uri = this.GetInputValue<string>(KEY_URI);

            this.commClient = ProcessCommFactory.CreateClient(commType);
            this.commClient.StatusChanged += this.CommClient_StatusChanged;
            this.commClient.DataReceived += this.CommClient_DataReceived;
            this.commClient.ServiceURI = uri;
            return this.commClient.Initialize();
        }

        protected override int OnStart()
        {
            return this.commClient.Connect();
        }

        protected override void OnStop()
        {
            this.commClient.Disconnect();
        }

        protected override void OnRelease()
        {
            this.commClient.Release();
        }

        #endregion

        #region 抽象方法

        /// <summary>
        /// 当收到了ModuleHost发送过来的消息的时候触发
        /// </summary>
        /// <param name="cmdType">命令类型</param>
        /// <param name="cmdParam">命令参数</param>
        protected abstract void OnDataReceived(int cmdType, object cmdParam);

        #endregion

        #region 实例方法

        /// <summary>
        /// 给外部进程发消息
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public int Send(int cmdType, byte[] cmdParam)
        {
            return this.commClient.Send(cmdType, cmdParam);
        }

        /// <summary>
        /// 给外部进程发消息
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public int Send(int cmdType, string cmdParam)
        {
            return this.commClient.Send(cmdType, cmdParam);
        }

        /// <summary>
        /// 给外部进程发消息
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public int Send(int cmdType, object cmdParam)
        {
            return this.commClient.Send(cmdType, cmdParam);
        }

        #endregion

        #region 事件处理器

        private void CommClient_DataReceived(ProcessCommObject commObject, int cmdType, object cmdParam)
        {
            this.OnDataReceived(cmdType, cmdParam);
        }

        private void CommClient_StatusChanged(ProcessCommClient commObject, CommClientStates status)
        {
            logger.InfoFormat("ProcessCommClient连接状态发生改变, {0}", status);
        }

        #endregion
    }
}