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
    /// GDM906X万用表
    /// </summary>
    public class GDM906XMeter : ModuleBase
    {
        #region 类变量

        private static readonly char[] Splitter = new char[] { ',' };

        private static log4net.ILog logger = log4net.LogManager.GetLogger("GDM906XMeter");

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

        private bool Query(string action, out string result, params string[] commands)
        {
            result = string.Empty;

            try
            {
                foreach (string command in commands)
                {
                    this.channel.WriteLine(command);
                }
                this.channel.WriteLine("READ?");

                result = this.channel.ReadLine();

                return true;

                // 经常会报4XX的错误
                //return this.SYST_ERR(action);
            }
            catch (Exception ex)
            {
                logger.Error("发送查询指令异常", ex);
                return false;
            }
        }

        private bool SendSCPIRequest(string action, string scpi, out string result)
        {
            result = string.Empty;

            try
            {
                this.channel.WriteLine(scpi);

                result = this.channel.ReadLine();

                return this.SYST_ERR(action);
            }
            catch (Exception ex)
            {
                logger.Error("发送SCPI指令异常", ex);
                return false;
            }
        }

        private bool SendSCPIRequest(string action, string scpi)
        {
            try
            {
                this.channel.WriteLine(scpi);
            }
            catch (Exception ex)
            {
                logger.Error("发送SCPI指令异常", ex);
                return false;
            }

            return this.SYST_ERR(action);
        }

        private bool SYST_ERR(string action)
        {
            string result = string.Empty;

            try
            {
                this.channel.WriteLine("SYST:ERR?");

                result = this.channel.ReadLine();
            }
            catch (Exception ex)
            {
                logger.Error("发送SCPI指令异常", ex);
                return false;
            }

            if (string.IsNullOrEmpty(result))
            {
                logger.ErrorFormat("{0}, 执行失败, 仪器有错误发生, SYST:ERR?返回值为空", action);
                return false;
            }

            string[] items = result.Split(Splitter, StringSplitOptions.RemoveEmptyEntries);

            if (items.Length != 2)
            {
                logger.ErrorFormat("{0}, 执行失败, SYST:ERR?返回值不正确, {1}", action, result);
                return false;
            }

            if (items[0] != "0")
            {
                logger.ErrorFormat("{0}, 执行失败, {1}", action, result);
                return false;
            }

            return true;
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
            //value = 0;

            //if (!this.SendSCPIRequest("设置电压测量参数", string.Format("CONF:VOLT:DC;:VOLT:DC:RANG:AUTO OFF;:VOLT:DC:RANGE {0};", range)))
            //{
            //    return false;
            //}

            //string result;
            //if (!this.SendSCPIRequest("电压测量", ":SAMP:COUN 1;:TRIG:SOUR IMM;:READ?", out result))
            //{
            //    return false;
            //}

            //return this.Scientific2Double(result, out value);

            value = 0;
            string result;
            if (!this.Query("读取电压", out result, "CONF:VOLT:DC", "READ?"))
            {
                return false;
            }

            return this.Scientific2Double(result, out value);
        }

        /// <summary>
        /// 读取电阻值，单位欧姆
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool ReadResistance(out double value)
        {
            value = 0;
            string result;
            if (!this.Query("读取电阻", out result, "CONF:RES", "READ?"))
            {
                return false;
            }

            return this.Scientific2Double(result, out value);
        }

        #endregion
    }
}
