using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace DotNEToolkit
{
    /// <summary>
    /// 封装Windows下绿色版mysql的安装程序逻辑
    /// 把这个程序放到Mysql/bin目录下，然后运行
    /// </summary>
    public class MySQLInstaller
    {
        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("MySQLInstaller");

        /// <summary>
        /// 默认的Mysql配置文件内容
        /// </summary>
        private const string DefualtMysqlConfig = "[mysqld]\r\nport=3306\r\nbasedir=\"{0}\"\r\ndatadir=\"{1}\"\r\nlog_error=\"{2}\"\r\nlog_error_verbosity=3";

        private const string MysqlServiceName = "MySQL";

        #endregion

        #region 公开接口

        /// <summary>
        /// 安装进度回调
        /// </summary>
        public event Action<MySQLInstaller, double> Progress;

        #endregion

        #region 实例变量

        private string mysqlDir;
        private string mysqldPath;

        #endregion

        #region 构造方法

        public MySQLInstaller(string mysqlDir = null)
        {
            if (string.IsNullOrEmpty(mysqlDir))
            {
                this.mysqlDir = AppDomain.CurrentDomain.BaseDirectory;
            }
            else
            {
                this.mysqlDir = mysqlDir;
            }

            this.mysqldPath = Path.Combine(this.mysqlDir, "bin", "mysqld.exe");
        }

        #endregion

        #region 公开接口

        /// <summary>
        /// 把Mysql安装成一个Windows服务
        /// </summary>
        public int Install()
        {
            #region 首先生成配置文件

            string myPath = Path.Combine(this.mysqlDir, "my.ini");
            if (File.Exists(myPath))
            {
                File.Delete(myPath);
            }

            logger.InfoFormat("生成配置文件, {0}", myPath);

            string dataDir = Path.Combine(this.mysqlDir, "data");
            string logDir = Path.Combine(this.mysqlDir, "log");
            string config = string.Format(DefualtMysqlConfig, this.mysqlDir, dataDir, logDir);

            try
            {
                File.WriteAllText(myPath, config);
            }
            catch (Exception ex)
            {
                logger.Error("保存Mysql配置文件异常", ex);
                return DotNETCode.FAILED;
            }

            #endregion

            this.NotifyProgress(25);

            #region 创建Windows服务

            // 创建Windows服务
            string matched;
            if (!this.RunCommand("--install", "Service successfully installed", out matched))
            {
                logger.ErrorFormat("安装Windows服务失败");
                return DotNETCode.MYSQL_INSTALL_SVC_FAILED;
            }

            #endregion

            this.NotifyProgress(50);

            #region 初始化数据库

            // 初始化数据库
            string logPath = Path.Combine(this.mysqlDir, "log.err");
            if (File.Exists(logPath))
            {
                File.Delete(logPath);
            }
            Process process = null;
            try
            {
                process = Processes.CreateProcess(this.mysqldPath, "--initialize");
                process.WaitForExit();      // 等待数据库初始化完
            }
            catch (Exception ex)
            {
                logger.Error("创建mysqld进程异常", ex);
                return DotNETCode.MYSQL_INITIALIZE_FAILED;
            }
            finally
            {
                if (process != null)
                {
                    process.Dispose();
                }
            }

            // 从日志文件里获取mysql密码
            string[] lines = File.ReadAllLines(logPath);
            matched = lines.FirstOrDefault(v => v.Contains("A temporary password is generated for"));
            if (string.IsNullOrEmpty(matched))
            {
                logger.ErrorFormat("从日志文件里获取Mysql密码失败");
                return DotNETCode.MYSQL_INITIALIZE_FAILED;
            }

            #endregion

            this.NotifyProgress(75);

            #region 保存Mysql的用户名和密码

            // 解析Mysql密码
            string[] items = matched.Split(new string[] { "root@localhost:" }, StringSplitOptions.RemoveEmptyEntries);
            string password = items.Last().Trim();
            string username = "root";

            // 把用户名和密码保存到当前目录
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(string.Format("用户名:{0}", username));
            builder.AppendLine(string.Format("密码:{0}", password));
            builder.AppendLine(string.Format("请使用mysqld -proot -u\"{0}\"指令登录mysql", password));
            string path = Path.Combine(this.mysqlDir, "password.txt");
            File.WriteAllText(path, builder.ToString());

            #endregion

            this.NotifyProgress(100);

            #region 最后启动Mysql服务

            if (!WindowsServices.StartService(MysqlServiceName))
            {
                logger.ErrorFormat("启动Mysql服务失败");
                return DotNETCode.MYSQL_INSTALL_SVC_FAILED;
            }

            #endregion

            return DotNETCode.SUCCESS;
        }

        /// <summary>
        /// 卸载Mysql Windows服务
        /// </summary>
        public int Uninstall()
        {
            // 先暂停Mysql服务
            if (!WindowsServices.StopService(MysqlServiceName))
            {
                return DotNETCode.MYSQL_INSTALL_SVC_FAILED;
            }

            // 删除Windows服务
            string matched;
            if (!this.RunCommand("--remove", "Service successfully removed", out matched))
            {
                return DotNETCode.MYSQL_INSTALL_SVC_FAILED;
            }

            string dataDir = Path.Combine(this.mysqlDir, "data");
            if (Directory.Exists(dataDir))
            {
                //// 先备份一下原有的db目录
                //string backupDir = Path.Combine(this.mysqlDir, string.Format("data_bakup{0}", DateTime.Now.ToString(DateTimeFormat.yyyyMMddhhmmss)));
                //Directory.Move(dataDir, backupDir);
                Directory.Delete(dataDir, true);

                // 再删除
                // 备份就相当于删除了，所以就不用删除了
            }

            // 删除配置文件
            

            return DotNETCode.SUCCESS;
        }

        #endregion

        #region 实例方法

        private bool RunCommand(string command, string match, out string matched)
        {
            matched = null;
            Process process = null;

            try
            {
                process = Processes.CreateProcess(this.mysqldPath, command);

                while (!process.StandardOutput.EndOfStream)
                {
                    string line = process.StandardOutput.ReadLine();

                    if (line.Contains(match))
                    {
                        matched = match;
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                logger.Error("创建Mysqld进程异常", ex);
                return false;
            }
            finally
            {
                if (process != null)
                {
                    process.Close();
                    process.Dispose();
                }
            }
        }

        private void NotifyProgress(double progress)
        {
            if (this.Progress != null)
            {
                this.Progress(this, progress);
            }
        }

        #endregion
    }
}

