using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Factory.NET
{
    public static class FactoryUtils
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger("FactoryUtils");

        /// <summary>
        /// 检查是否有ADB设备连接
        /// </summary>
        /// <returns>如果有链接则返回true，没有设备链接则返回false</returns>
        public static bool CheckAdbDevice()
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo()
            {
                FileName = "adb.exe",
                Arguments = "devices",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
            };

            try
            {
                Process process = Process.Start(processStartInfo);
                string message = process.StandardOutput.ReadToEnd();

                return message.Contains("\tdevices");
            }
            catch (Exception ex)
            {
                logger.Error("检查ADB设备状态异常", ex);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="adbExePath">adb可执行文件路径</param>
        /// <param name="remotePath">设备里的文件路径</param>
        /// <param name="localPath">要拷贝到的本地路径</param>
        /// <param name="fileExist">设备里的的文件是否存在</param>
        /// <param name="output">adb输出的内容</param>
        /// <returns>pull指令是否执行成功</returns>
        public static bool AdbPullFile(string adbExePath, string remotePath, string localPath, out bool fileExist, out string output)
        {
            fileExist = false;
            output = string.Empty;

            string pullCommand = string.Format("pull {0} {1}", remotePath, localPath);

            ProcessStartInfo psi = new ProcessStartInfo()
            {
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                FileName = adbExePath,
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


        /// <summary>
        /// 读取设备里的文件内容并返回
        /// 为了防止有些系统需要登录才能读取，做法是先把要读取的文件拷贝到本地，然后读取本地文件内容
        /// </summary>
        /// <param name="adbExePath"></param>
        /// <param name="remotePath"></param>
        /// <param name="tempPath">要保存本地文件的名字</param>
        /// <param name="content">保存读取到的文件内容</param>
        /// <returns></returns>
        public static int AdbReadFile(string adbExePath, string remotePath, string tempPath, out string content)
        {
            content = string.Empty;

            if (!File.Exists(tempPath))
            {
                try
                {
                    File.Create(tempPath).Close();
                }
                catch (Exception ex) 
                {
                    logger.Error("创建adb文件异常", ex);
                    return ResponseCode.FAILED;
                }
            }

            bool fileExist;
            string adbMessage;
            if (!AdbPullFile(adbExePath, remotePath, tempPath, out fileExist, out adbMessage))
            {
                return ResponseCode.FAILED;
            }

            if (!fileExist)
            {
                logger.ErrorFormat("读取文件失败, 文件不存在, {0}", remotePath);
                return ResponseCode.FAILED;
            }

            content = File.ReadAllText(tempPath);

            return ResponseCode.SUCCESS;
        }
    }
}
