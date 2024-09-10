using log4net.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Factory.NET.Modules
{
    public class CheckAdbFile : TaskModule
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger("CheckAdbFile");

        public override int Run()
        {
            string defaultAdbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "adb.exe");

            string source = this.GetParameter<string>("source");
            string target = this.GetParameter<string>("target");
            string adbPath = this.GetParameter<string>("adbPath", defaultAdbPath);

            bool success = true;

            #region 先创建一个本地文件

            if (File.Exists(target))
            {
                try
                {
                    File.Delete(target);
                }
                catch (Exception ex)
                {
                    logger.Error("删除本地文件异常", ex);
                    return ResponseCode.FAILED;
                }
            }

            try
            {
                File.Create(target).Close();
            }
            catch (Exception ex)
            {
                logger.Error("创建本地文件异常", ex);
                return ResponseCode.FAILED;
            }

            #endregion

            #region 用adb拷贝到本机

            bool fileExist;
            if (!this.AdbPullFile(adbPath, source, target, out fileExist))
            {
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
                    logger.Error("读取文件内容异常", ex);
                    return ResponseCode.FAILED;
                }
                success = content.Contains(match);
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
        /// <returns></returns>
        private bool AdbPullFile(string adbPath, string srcPath, string destPath, out bool fileExist)
        {
            fileExist = false;

            string command = string.Format("pull {0} {1}", srcPath, destPath);

            ProcessStartInfo psi = new ProcessStartInfo()
            {
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                FileName = adbPath,
                Arguments = command
            };

            Process process = null;

            try
            {
                process = Process.Start(psi);
            }
            catch (Exception ex) 
            {
                logger.Error("adb pull异常", ex);
                return false;
            }

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            process.Dispose();

            if (output.Contains("remote object") && output.Contains("does not exist"))
            {
                fileExist = false;
            }
            else
            {
                fileExist = true;
            }

            return true;
        }
    }
}

