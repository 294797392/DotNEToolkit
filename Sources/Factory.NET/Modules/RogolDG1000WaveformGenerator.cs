using DotNEToolkit.Modular;
using Factory.NET.Channels;
using NationalInstruments.VisaNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Factory.NET.Modules
{
    /// <summary>
    /// Rogo DG1000型号的信号发生器
    /// </summary>
    public class RogolDG1000Generator : ModuleBase
    {
        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("RogolDG1000WaveformGenerator");

        #endregion

        #region 实例变量

        private ChannelBase channel;
        private int channelIndex;

        #endregion

        #region ModuleBase

        protected override int OnInitialize()
        {
            this.channelIndex = this.GetParameter<int>("channelIndex");

            this.channel = ChannelFactory.Create(this.InputParameters);
            this.channel.Initialize(this.InputParameters);

            return ResponseCode.SUCCESS;
        }

        protected override void OnRelease()
        {
            this.channel.Release();
        }

        #endregion

        #region 公开接口

        /// <summary>
        /// 输出PWM信号
        /// </summary>
        /// <param name="frequency">频率，单位赫兹</param>
        /// <param name="dutyCycle">占空比，单位百分比，占空比越高功率越高</param>
        /// <param name="highVoltage">高电平电压</param>
        public void OpenPWMOutput(int frequency, int dutyCycle, double highVoltage)
        {
            string ch = this.channelIndex == 1 ? string.Empty : string.Format(":CH{0}", this.channel);
            this.channel.WriteLine(string.Format("FUNC{0} PULSe", ch, highVoltage));
            this.channel.WriteLine(string.Format("VOLT{0} {1}", ch, highVoltage));
            this.channel.WriteLine(string.Format("FREQ{0} {1}", ch, frequency));
            this.channel.WriteLine(string.Format("FUNCtion:SQUare:DCYCle{0} {1}", ch, dutyCycle));
            this.channel.WriteLine(string.Format("OUTP{0} ON", ch));
        }

        /// <summary>
        /// 关闭信号输出
        /// </summary>
        public void Close()
        {
            string ch = this.channelIndex == 1 ? string.Empty : string.Format(":CH{0}", this.channel);
            this.channel.WriteLine(string.Format("OUTP{0} OFF", ch));
        }

        #endregion
    }
}
