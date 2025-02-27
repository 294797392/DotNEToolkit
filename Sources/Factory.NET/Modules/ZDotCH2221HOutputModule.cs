using Factory.NET.Modbus;
using System.Collections.Generic;
using System;
using DotNEToolkit.Utility;

namespace Factory.NET.Modules
{
    /// <summary>
    /// 零点自动化CH2221H数字输出模块
    /// 32个输出口，输出电压24V
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

        /// <summary>
        /// 设置端口的状态
        /// </summary>
        /// <param name="moduleIndex">第几个模块</param>
        /// <param name="count">要设置多少个端口</param>
        /// <param name="states32">要设置的每个端口的状态</param>
        /// <returns></returns>
        public bool WriteDO(int moduleIndex, int count, List<bool> states32)
        {
            int data = 0;

            for (int i = 0; i < states32.Count; i++)
            {
                data = ByteUtils.SetBit(data, (byte)i, states32[i]);
            }

            byte[] values = BitConverter.GetBytes(data);
            int addr = moduleIndex * 32;
            return this.WriteCoils((ushort)addr, (ushort)count, values);
        }

        #endregion
    }
}
