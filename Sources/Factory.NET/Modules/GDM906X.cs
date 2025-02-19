using DotNEToolkit.Modular;
using Factory.NET.Channels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Factory.NET.Modules
{
    public class GDM906X : ModuleBase
    {
        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("GDM906X");

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

        /// <summary>
        /// 读取电压值
        /// </summary>
        /// <returns></returns>
        public bool ReadVoltage() 
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 读取电流值
        /// </summary>
        /// <returns></returns>
        public bool ReadCurrent() 
        {
            throw new NotImplementedException();
        }
    }
}
