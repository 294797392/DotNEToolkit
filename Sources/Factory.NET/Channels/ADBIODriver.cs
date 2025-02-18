using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Factory.NET.IODrivers
{
    /// <summary>
    /// 在使用ADBIODriver的时候，一定要注意不能把接收缓冲区存满，不然会卡住，导致工作流程无法正常运行
    /// </summary>
    public class ADBIODriver : AbstractIODriver
    {
        /// <summary>
        /// 默认的adb路径
        /// </summary>
        public static string DefaultADBPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "adb.exe");

        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("ADBIODevice");

        #endregion

        #region 实例变量

        private string adbPath;
        private Process adbProcess;

        #endregion

        public override IODriverTypes Type { get { return IODriverTypes.ADB; } }

        #region 构造方法

        public ADBIODriver()
        {

        }

        #endregion

        #region AbstractIODriver

        protected override int OnInitialize()
        {
            base.OnInitialize();

            this.adbPath = this.GetParameter<string>("ADBPath", DefaultADBPath);

            this.StartAdbProcess();

            // 为什么这里每次都返回true？
            // 这个函数是在客户端打开之后立即调用的，可能此时adb并没有连接上，如果返回false，那么就初始化失败导致无法运行TestFlow。
            return ResponseCode.SUCCESS;
        }

        protected override void OnRelease()
        {
            this.CloseAdbProcess();
        }

        public override int WriteBytes(byte[] buffer)
        {
            string text = Encoding.ASCII.GetString(buffer);
            this.adbProcess.StandardInput.Write(text);
            return ResponseCode.SUCCESS;
        }

        public override int ReadBytes(byte[] buffer)
        {
            this.adbProcess.StandardOutput.BaseStream.Read(buffer, 0, buffer.Length);
            return ResponseCode.SUCCESS;
        }

        public override int WriteLine(string cmd)
        {
            cmd = string.Format("{0}{1}", cmd, base.newLine);
            byte[] buff = Encoding.ASCII.GetBytes(cmd);
            this.adbProcess.StandardInput.Write(cmd);
            return ResponseCode.SUCCESS;
        }

        public override int ReadLine(out string data)
        {
            data = this.adbProcess.StandardOutput.ReadLine();
            return ResponseCode.SUCCESS;
        }

        public override void ClearExisting()
        {
            this.adbProcess.StandardOutput.ReadToEnd();
        }

        #endregion

        #region 实例方法

        private bool StartAdbProcess()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = this.adbPath,
                Arguments = "shell",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                RedirectStandardError = true
            };

            try
            {
                this.adbProcess = Process.Start(startInfo);
                /*
                     停止两秒等待，然后检测adb进程是否还在运行，如果不在运行，有可能是设备没连接到电脑上
                     因为adb是个控制台程序，刚打开马上关闭，说明肯定参数错误，我用的shell参数，如果设备没连接上，
                     会报错然后直接退出
                 */
                System.Threading.Thread.Sleep(2000);
                if (this.adbProcess.HasExited)
                {
                    this.CloseAdbProcess();
                    logger.DebugFormat("启动adb进程失败");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                logger.Error("启动adb进程异常", ex);
                this.CloseAdbProcess();
                return false;
            }
        }

        private void CloseAdbProcess()
        {
            if (this.adbProcess != null)
            {
                try
                {
                    this.adbProcess.Close();
                    this.adbProcess.Dispose();
                }
                catch (Exception ex)
                {
                    logger.Error("关闭adb进程异常", ex);
                }
                finally
                {
                    this.adbProcess = null;
                }
            }
        }

        private bool TestConnection()
        {
            if (this.adbProcess == null)
            {
                return false;
            }

            if (this.adbProcess.HasExited)
            {
                this.CloseAdbProcess();
                return false;
            }

            return true;
        }

        #endregion
    }
}