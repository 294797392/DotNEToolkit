using DotNEToolkit.Modbus;
using DotNEToolkit.Modular;
using Factory.NET.IODrivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Factory.NET.Modules
{
    /// <summary>
    /// 零点自动化CH2221H数字输出模块
    /// </summary>
    public class ZDotCH2221HDigitalOutputModule : ModbusRTUClient
    {
        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("ZDotCH2221HDigitalOutputModule");

        #endregion

        #region 实例变量

        

        #endregion

        #region ModuleBase

        protected override int OnInitialize()
        {
            base.OnInitialize();

            return ResponseCode.SUCCESS;
        }

        protected override void OnRelease()
        {
            // TODO：

            base.OnRelease();
        }

        #endregion
    }
}
