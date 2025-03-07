using log4net.Repository.Hierarchy;
using NationalInstruments.VisaNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Factory.NET.Channels
{
    public class NIVisaChannel : ChannelBase
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger("NIVisaChannel");

        #region 实例变量

        private MessageBasedSession messageSession;

        #endregion

        #region 属性

        public override ChannelTypes Type => ChannelTypes.VISADriver;

        #endregion

        #region ChannelBase

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

        public override void ClearExisting()
        {
            this.messageSession.Clear();
        }

        public override int ReadBytes(byte[] bytes, int offset, int len)
        {
            byte[] buffer = this.messageSession.ReadByteArray(len);
            Buffer.BlockCopy(buffer, 0, bytes, offset, buffer.Length);
            return buffer.Length;
        }

        public override byte[] ReadBytesFull(int size)
        {
            return this.messageSession.ReadByteArray(size);
        }

        public override string ReadLine()
        {
            return this.messageSession.ReadString();
        }

        public override void WriteBytes(byte[] bytes)
        {
            this.messageSession.Write(bytes);
        }

        public override void WriteLine(string line)
        {
            this.messageSession.Write(line);
        }

        #endregion
    }
}
