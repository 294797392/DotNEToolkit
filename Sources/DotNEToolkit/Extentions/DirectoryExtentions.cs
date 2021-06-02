using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit
{
    /// <summary>
    /// 对目录的扩展类
    /// </summary>
    public static class DirectoryExtentions
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger("DirectoryExtentions");

        /// <summary>
        /// 枚举某个目录，会把异常截住
        /// </summary>
        /// <param name="dir">要枚举的目录</param>
        /// <returns>子目录列表</returns>
        public static IEnumerable<string> EnumerateDirectories(string dir)
        {
            try
            {
                return Directory.EnumerateDirectories(dir, "*", SearchOption.TopDirectoryOnly);
            }
            catch (System.Security.SecurityException)
            {
                logger.Error(string.Format("目录不具有访问权限, dir = {0}", dir));
            }
            catch (System.UnauthorizedAccessException)
            {
                logger.Error(string.Format("目录不具有访问权限, dir = {0}", dir));
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("访问目录发生异常, {0}", dir), ex);
            }

            return new List<string>();
        }

        /// <summary>
        /// 枚举某个目录下的所有文件，会把异常截住
        /// 如果某个目录没有访问权限，.NET自带的枚举函数会抛异常
        /// </summary>
        /// <param name="dir">要枚举的目录</param>
        /// <returns>文件列表</returns>
        public static IEnumerable<string> EnumerateFiles(string dir)
        {
            try
            {
                return Directory.EnumerateFiles(dir, "*", SearchOption.TopDirectoryOnly);
            }
            catch (System.Security.SecurityException)
            {
                logger.Error(string.Format("目录不具有访问权限, dir = {0}", dir));
            }
            catch (System.UnauthorizedAccessException)
            {
                logger.Error(string.Format("目录不具有访问权限, dir = {0}", dir));
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("访问目录发生异常, {0}", dir), ex);
            }

            return new List<string>();
        }

        /// <summary>
        /// 获取目录大小
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static long DirectorySize(string dir)
        {
            if (!Directory.Exists(dir))
            {
                return 0;
            }

            long size = 0;

            // 通过GetFiles方法,获取di目录中的所有文件的大小
            IEnumerable<string> subFiles = DirectoryExtentions.EnumerateFiles(dir);
            foreach (string filePath in subFiles)
            {
                FileInfo file = new FileInfo(filePath);
                size += file.Length;
            }

            //获取di中所有的文件夹,并存到一个新的对象数组中,以进行递归
            IEnumerable<string> subDirs = DirectoryExtentions.EnumerateDirectories(dir);
            foreach (string subDir in subDirs)
            {
                size += DirectorySize(subDir);
            }

            return size;
        }

        /// <summary>
        /// 以易于阅读的格式输出文件大小（例如 1K 234M 2G等）
        /// </summary>
        /// <param name="size">以字节为单位的大小</param>
        /// <returns></returns>
        public static string DirectoryReadableSize(string dir)
        {
            return DirectoryReadableSize(DirectorySize(dir));
        }

        /// <summary>
        /// 以易于阅读的格式输出文件大小（例如 1K 234M 2G等）
        /// </summary>
        /// <param name="size">以字节为单位的大小</param>
        /// <returns></returns>
        public static string DirectoryReadableSize(long size)
        {
            string m_strSize = string.Empty;
            long FactSize = 0;
            FactSize = size;

            // if (FactSize < 1024.00)
            // m_strSize = FactSize.ToString("F2") + " Byte";
            if (FactSize < 1048576)
                m_strSize = (FactSize / 1024.00).ToString("F2") + " K";
            else if (FactSize >= 1048576)
                m_strSize = (FactSize / 1024.00 / 1024.00).ToString("F2") + " M";
            return m_strSize;
        }
    }
}