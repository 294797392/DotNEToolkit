﻿using Factory.NET.Modbus;

namespace Factory.NET.Modules
{
    /// <summary>
    /// 零点自动化CH2221H数字输出模块
    /// </summary>
    public class ZDotCH2221H : ModbusTCPClient
    {
        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("ZDotCH2221H");

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
