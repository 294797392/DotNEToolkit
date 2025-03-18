using Factory.NET.Modbus;
using System.Collections.Generic;
using System;
using DotNEToolkit.Utility;
using System.Linq;

namespace Factory.NET.Modules
{
    /// <summary>
    /// 零点自动化CH2221H数字输出模块
    /// 32个输出口，输出电压24V
    /// 
    /// 第一块板子地址：0-31
    /// 第二块板子地址：32-63
    /// 第三块板子地址：64-95
    /// 第4块板子地址：96
    /// 5:128
    /// 6:160
    /// 7:192
    /// 8:224
    /// </summary>
    public class ZDotCH2221HOutputModule : ModbusTCPClient
    {
        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("ZDotCH2221HOutputModule");

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

        #region 公开接口

        #endregion
    }
}
