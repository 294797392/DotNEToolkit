using DotNEToolkit.Modular;
using NationalInstruments.VisaNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Factory.NET.Modules
{
    public class RogolDG1000WaveformGenerator : ModuleBase
    {
        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("RogolDG1000WaveformGenerator");

        #endregion

        #region 实例变量

        private MessageBasedSession messageSession;

        #endregion

        #region ModuleBase

        protected override int OnInitialize()
        {
            string resourceName = this.GetParameter<string>("resourceName");

            ResourceManager resourceMgr = ResourceManager.GetLocalManager();

            // 查询所有资源
            string[] allResource = resourceMgr.FindResources("?*INSTR");
            string name = allResource.FirstOrDefault(v => v.Contains(resourceName));
            if (string.IsNullOrEmpty(name))
            {
                logger.ErrorFormat("没有找到对应的资源, {0}", resourceName);
                return ResponseCode.FAILED;
            }

            this.messageSession = resourceMgr.Open(resourceName) as MessageBasedSession;

            return ResponseCode.SUCCESS;
        }

        protected override void OnRelease()
        {
            this.messageSession.Dispose();
        }

        #endregion

        #region 公开接口

        public void Write(string command) 
        {
            this.messageSession.Write(command);
        }

        #endregion
    }
}
