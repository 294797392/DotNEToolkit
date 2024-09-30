using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace Factory.NET.Modules
{
    public class CheckAdbFile : TaskModule
    {
        /// <summary>
        /// 定义该测试流程的错误码
        /// </summary>
        public enum FailReasons
        {
            /// <summary>
            /// 测试成功
            /// </summary>
            PASS,

            /// <summary>
            /// 执行pull指令失败
            /// </summary>
            PullCommandFailed,

            /// <summary>
            /// 创建本地文件失败
            /// </summary>
            CreateFileFailed,

            /// <summary>
            /// 设备里不存在这个文件
            /// </summary>
            FileNotExist,

            /// <summary>
            /// 文件内容不匹配
            /// </summary>
            NotMatch,

            /// <summary>
            /// 读取文件内容失败
            /// </summary>
            ReadFileFailed
        }

        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger("CheckAdbFile");

        public override int Run()
        {
            string defaultAdbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "adb.exe");

            string source = this.GetParameter<string>("source");
            string target = this.GetParameter<string>("target");
            string adbPath = this.GetParameter<string>("adbPath", defaultAdbPath);

            bool success = true;

            this.Message = FailReasons.PASS.ToString();

            #region 先创建一个本地文件

            if (!File.Exists(target))
            {
                try
                {
                    File.Create(target).Close();
                }
                catch (Exception ex)
                {
                    this.Message = FailReasons.CreateFileFailed.ToString();
                    logger.Error("创建本地文件异常", ex);
                    return ResponseCode.FAILED;
                }
            }

            #endregion

            #region 用adb拷贝到本机

            bool fileExist;
            string output;
            if (!this.AdbPullFile(adbPath, source, target, out fileExist, out output))
            {
                // ADB指令执行失败
                this.Message = string.Format("{0},{1}", FailReasons.PullCommandFailed, output);
                return ResponseCode.FAILED;
            }

            // pull指令执行成功，但是设备不存在这个文件
            if (!fileExist)
            {
                this.Message = string.Format("{0}", FailReasons.FileNotExist);
                return ResponseCode.FAILED;
            }

            #endregion

            #region 检查文件内容

            string match = this.GetParameter<string>("match", string.Empty);
            if (!string.IsNullOrEmpty(match))
            {
                string content;

                try
                {
                    content = File.ReadAllText(target);
                }
                catch (Exception ex)
                {
                    this.Message = string.Format("{0},{1}", FailReasons.ReadFileFailed, ex.Message);
                    logger.Error("读取文件内容异常", ex);
                    return ResponseCode.FAILED;
                }
                success = content.Contains(match);

                if (!success)
                {
                    this.Message = string.Format("{0},{1}", FailReasons.NotMatch, content);
                }
            }

            #endregion

            return success ? ResponseCode.SUCCESS : ResponseCode.FAILED;
        }

        protected override int OnInitialize()
        {
            return ResponseCode.SUCCESS;
        }

        protected override void OnRelease()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="adbPath"></param>
        /// <param name="srcPath"></param>
        /// <param name="destPath"></param>
        /// <param name="fileExist">远程文件是否存在</param>
        /// <returns>pull指令是否执行成功</returns>
        private bool AdbPullFile(string adbPath, string srcPath, string destPath, out bool fileExist, out string output)
        {
            fileExist = false;
            output = string.Empty;

            string pullCommand = string.Format("pull {0} {1}", srcPath, destPath);

            ProcessStartInfo psi = new ProcessStartInfo()
            {
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                FileName = adbPath,
                Arguments = pullCommand
            };

            Process process = null;

            try
            {
                process = Process.Start(psi);
            }
            catch (Exception ex)
            {
                logger.Error("adb pull异常", ex);
                output = ex.Message;
                return false;
            }

            output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            process.Dispose();

            if (output.Contains("remote object") && output.Contains("does not exist"))
            {
                fileExist = false;
                return true;
            }
            else if (output.Contains("file pulled"))
            {
                fileExist = true;
                return true;
            }
            else
            {
                logger.ErrorFormat("pull指令执行失败, {0}, {1}", pullCommand, output);
                fileExist = false;
                return false;
            }
        }
    }
}

