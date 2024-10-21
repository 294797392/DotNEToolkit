using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Factory.NET.Utility
{
    public enum AdbPullResult
    {
        /// <summary>
        /// Pull成功
        /// </summary>
        Susccess,

        /// <summary>
        /// 设备内部不存在这个文件
        /// </summary>
        DeviceFileNotExist,

        /// <summary>
        /// Pull指令执行失败，未知错误
        /// </summary>
        UnkownFailed,

        AdbProcessException,

        /// <summary>
        /// 创建本地文件失败
        /// </summary>
        CreateLocalFileFailed,
    }

    public enum AdbPushResult
    {
        /// <summary>
        /// Push指令执行成功
        /// </summary>
        Success,

        /// <summary>
        /// 要Push的文件不存在
        /// </summary>
        SourceFileNotExist,

        /// <summary>
        /// Push指令执行失败，未知错误
        /// </summary>
        UnkownFailed,

        /// <summary>
        /// Push发生异常情况
        /// </summary>
        AdbProcessException,
    }

    public static class AdbUtility
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger("AdbUtility");

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
        /// 把设备上的文件拉取到本地
        /// </summary>
        /// <param name="adbExePath">adb可执行文件路径</param>
        /// <param name="remotePath">设备里的文件路径</param>
        /// <param name="localPath">要拷贝到的本地路径</param>
        /// <param name="message">adb输出的内容</param>
        /// <returns>pull指令是否执行成功</returns>
        public static AdbPullResult AdbPullFile(string adbExePath, string remotePath, string localPath, out string message)
        {
            message = string.Empty;

            logger.InfoFormat("adb pull, remotePath = {0}, localPath = {1}", remotePath, localPath);

            try
            {
                if (!File.Exists(localPath))
                {
                    File.Create(localPath).Close();
                }
            }
            catch (Exception ex)
            {
                logger.Error("adb pull异常, 创建本地文件异常", ex);
                return AdbPullResult.CreateLocalFileFailed;
            }

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
                message = ex.Message;
                return AdbPullResult.AdbProcessException;
            }

            message = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            process.Dispose();

            if (message.Contains("remote object") && message.Contains("does not exist"))
            {
                return AdbPullResult.DeviceFileNotExist;
            }
            else if (message.Contains("file pulled"))
            {
                return AdbPullResult.Susccess;
            }
            else
            {
                logger.ErrorFormat("pull指令执行失败, {0}, {1}", pullCommand, message);
                return AdbPullResult.UnkownFailed;
            }
        }

        /// <summary>
        /// 把本地文件推送到设备上
        /// </summary>
        /// <param name="adbExePath"></param>
        /// <param name="srcPath">要推送到设备上的本地文件的路径</param>
        /// <param name="destPath">要推送到设备上的路径</param>
        /// <param name="message">adb输出的内容</param>
        /// <returns></returns>
        public static AdbPushResult AdbPushFile(string adbExePath, string srcPath, string destPath, out string message)
        {
            logger.InfoFormat("adb push, srcPath = {0}, destPath = {1}", srcPath, destPath);

            message = string.Empty;

            if (!File.Exists(srcPath))
            {
                logger.ErrorFormat("adb push失败, 要push的本地文件不存在, {0}", srcPath);
                return AdbPushResult.SourceFileNotExist;
            }

            string pullCommand = string.Format("push {0} {1}", srcPath, destPath);

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
                logger.Error("adb push异常", ex);
                message = ex.Message;
                return AdbPushResult.AdbProcessException;
            }

            message = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            process.Dispose();

            if (message.Contains("1 file pushed"))
            {
                return AdbPushResult.Success;
            }
            else
            {
                logger.ErrorFormat("push指令执行失败, {0}, {1}", pullCommand, message);
                return AdbPushResult.UnkownFailed;
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

            string adbMessage;
            AdbPullResult pullResult = AdbPullFile(adbExePath, remotePath, tempPath, out adbMessage);
            if (pullResult != AdbPullResult.Susccess)
            {
                return ResponseCode.FAILED;
            }

            content = File.ReadAllText(tempPath);

            return ResponseCode.SUCCESS;
        }
    }
}
