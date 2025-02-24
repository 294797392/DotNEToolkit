using DotNEToolkit.Modular;
using Factory.NET.Channels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Factory.NET.Modules
{
    /// <summary>
    /// NGI N36100系列直流电源
    /// </summary>
    public class N36100DCPower : ModuleBase
    {
        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("N36100DCPower");

        #endregion

        #region 实例变量

        private ChannelBase channel;

        #endregion

        #region ModuleBase

        protected override int OnInitialize()
        {
            this.channel = ChannelFactory.Create(this.InputParameters);
            this.channel.Initialize(this.InputParameters);

            return ResponseCode.SUCCESS;
        }

        protected override void OnRelease()
        {
            this.channel.Release();
        }

        #endregion

        #region 实例方法

        private string Query(params string[] commands)
        {
            try
            {
                foreach (string command in commands)
                {
                    this.channel.WriteLine(command);
                }
                this.channel.WriteLine("READ?");

                return this.channel.ReadLine();
            }
            catch (Exception ex)
            {
                logger.Error("发送查询指令异常", ex);
                return string.Empty;
            }
        }

        /// <summary>
        /// 科学计数法转换成小数，保留两位小数
        /// </summary>
        /// <param name="scientific"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private bool Scientific2Double(string scientific, out double result)
        {
            scientific = scientific.Replace("\r", string.Empty).Replace("\n", string.Empty);
            if (!double.TryParse(scientific, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
            {
                return false;
            }

            result = Math.Round(result, 2);
            return true;
        }

        #endregion

        #region 公开接口

        /// <summary>
        /// 读取电压值
        /// 单位伏特
        /// </summary>
        /// <returns></returns>
        public bool ReadVoltage(out double value)
        {
            value = 0;
            string result = this.Query("CONF:VOLT:DC", "READ?");
            if (string.IsNullOrEmpty(result))
            {
                return false;
            }

            return this.Scientific2Double(result, out value);
        }

        /// <summary>
        /// 读取电流值
        /// </summary>
        /// <returns></returns>
        //public bool ReadCurrent() 
        //{
        //    string result = this.Query("CONF:CURR:DC", "READ?");
        //    if (string.IsNullOrEmpty(result)) 
        //    {
        //        return false;
        //    }
        //}

        /// <summary>
        /// 读取电阻值，单位欧姆
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool ReadResistance(out double value)
        {
            value = 0;
            string result = this.Query("CONF:RES", "READ?");
            if (string.IsNullOrEmpty(result))
            {
                return false;
            }

            return this.Scientific2Double(result, out value);
        }

        #endregion
    }
}
