using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace DotNEToolkit
{
    /// <summary>
    /// 管理Windows服务
    /// </summary>
    public static class WindowsServices
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger("WindowsServices");

        /// <summary>
        /// 启动一个Windows服务
        /// </summary>
        /// <param name="svcName">要启动的服务名字</param>
        /// <returns>
        /// 启动成功返回true
        /// 如果没找到服务或者启动失败则返回false
        /// </returns>
        public static bool StartService(string svcName)
        {
            ServiceController svc = ServiceController.GetServices().FirstOrDefault(v => v.ServiceName == svcName);
            if (svc == null)
            {
                return false;
            }

            if (svc.Status == ServiceControllerStatus.Running)
            {
                return true;
            }

            try
            {
                svc.Start();
                svc.WaitForStatus(ServiceControllerStatus.Running);
                return true;
            }
            catch (Exception ex)
            {
                logger.Error("启动Windows服务异常", ex);
                return false;
            }
        }

        /// <summary>
        /// 停止一个Windows服务
        /// </summary>
        /// <param name="svcName">要停止的服务名字</param>
        /// <returns>
        /// 停止成功返回true
        /// 如果没找到服务或者停止失败则返回false
        /// </returns>
        public static bool StopService(string svcName)
        {
            ServiceController svc = ServiceController.GetServices().FirstOrDefault(v => v.ServiceName == svcName);
            if (svc == null)
            {
                return false;
            }

            if (svc.Status == ServiceControllerStatus.Stopped)
            {
                return true;
            }

            try
            {
                svc.Stop();
                svc.WaitForStatus(ServiceControllerStatus.Stopped);
                return true;
            }
            catch (Exception ex)
            {
                logger.Error("停止Windows服务异常", ex);
                return false;
            }
        }
    }
}
