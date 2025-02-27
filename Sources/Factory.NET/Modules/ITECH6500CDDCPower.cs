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
    /// ITECH6500CD系列直流程控电源
    /// </summary>
    public class ITECH6500CDDCPower : ModuleBase
    {
        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("ITECH6500CDDCPower");

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

        private bool Send(params string[] commands)
        {
            try
            {
                foreach (string command in commands)
                {
                    this.channel.WriteLine(command);
                }

                return true;
            }
            catch (Exception ex)
            {
                logger.Error("发送控制指令异常", ex);
                return false;
            }
        }

        #endregion

        #region 公开接口

        /// <summary>
        /// 打开电源
        /// </summary>
        /// <param name="voltage">电压，单位V</param>
        /// <returns></returns>
        public bool Open(int voltage) 
        {
            return this.Send("SYST:REM", string.Format("VOLT {0}", voltage), "OUTP ON");
        }

        /// <summary>
        /// 关闭电源
        /// </summary>
        /// <returns></returns>
        public bool Close() 
        {
            return this.Send("OUTP OFF");
        }

        #endregion
    }
}
