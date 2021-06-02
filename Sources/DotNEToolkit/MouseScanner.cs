using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static DotNEToolkit.Win32API;

namespace DotNEToolkit
{
    /// <summary>
    /// 表示一个鼠标区域
    /// </summary>
    public class DesktopArea
    {
        /// <summary>
        /// 桌面区域的唯一标志符
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// 桌面区域
        /// </summary>
        public RECT Area { get; set; }
    }

    public class MouseScanner : SingletonObject<MouseScanner>
    {
        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("MouseScanner");

        #endregion

        #region 公开事件

        /// <summary>
        /// 当鼠标移动到指定区域的时候触发
        /// </summary>
        public event Action<MouseScanner, DesktopArea> MouseEnter;

        #endregion

        #region 实例变量

        private bool scanning;

        #endregion

        #region 公开接口

        public void StartScan()
        {
            if (this.scanning)
            {
                return;
            }

            this.scanning = true;

            Task.Factory.StartNew(this.WatchThreadProc);
        }

        public void StopScan()
        {
            this.scanning = false;
        }

        public void AddWatchArea(DesktopArea area)
        {
            
        }

        #endregion

        #region 实例方法

        private void ProcessCursorPosition(int x, int y)
        {
            
        }

        private void WatchThreadProc()
        {
            logger.Debug("鼠标监控线程启动成功");

            while (this.scanning)
            {
                try
                {
                    POINT pos;
                    GetCursorPos(out pos);

                    this.ProcessCursorPosition(pos.x, pos.y);
                }
                catch (Exception ex)
                {
                    logger.Error(string.Format("GetCursorPos Failed, Windows Error = {0}", Marshal.GetLastWin32Error()), ex);
                }

                Thread.Sleep(100);
            }
        }

        private void NotifyMouseEnter(DesktopArea area)
        {
            if (this.MouseEnter != null)
            {
                this.MouseEnter(this, area);
            }
        }

        #endregion
    }
}
