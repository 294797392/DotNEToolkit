using Factory.NET.Modbus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Factory.NET.Modules
{
    /// <summary>
    /// 上海裕信测控技术有限公司
    /// PK9015M模拟量输入模块
    /// 用来读取电压或电流
    /// </summary>
    public class PK9015M : ModbusRTUClient
    {
        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("PK9015M");

        /// <summary>
        /// 第0路的地址是0x03
        /// </summary>
        private const ushort BaseAddress = 0x03;

        #endregion

        #region 实例变量

        /// <summary>
        /// 量程
        /// </summary>
        private double range;

        #endregion

        #region ModuleBase

        protected override int OnInitialize()
        {
            base.OnInitialize();

            // 初始化的时候读取量程
            byte[] rangeBytes = this.ReadHoldingRegister(1, 0x02, 1);
            this.range = BitConverter.ToUInt16(rangeBytes.Reverse().ToArray(), 0);

            return ResponseCode.SUCCESS;
        }

        #endregion

        #region 公开接口

        /// <summary>
        /// 读取指定口的电流
        /// </summary>
        /// <param name="portIndex"></param>
        /// <returns></returns>
        public bool ReadCurrent(int portIndex, out double current)
        {
            current = 0;

            byte[] bytes = this.ReadHoldingRegister(1, (ushort)(BaseAddress + portIndex), 1);
            if (bytes == null) 
            {
                return false;
            }
            
            ushort value = BitConverter.ToUInt16(bytes.Reverse().ToArray(), 0);
            current = (double)value / 10000 * this.range;
            return true;
        }

        #endregion
    }
}
