using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Factory.NET.IODrivers
{
    public class StreamIODevice : AbstractIODriver
    {
        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("StreamIODevice");

        #endregion

        #region 实例变量

        private Stream stream;
        private StreamWriter writer;
        private StreamReader reader;

        #endregion

        #region 属性

        public override IODriverTypes Type { get { return IODriverTypes.StreamDevice; } }

        #endregion

        #region AbstractIODevice

        protected override int OnInitialize()
        {
            base.OnInitialize();

            return ResponseCode.SUCCESS;
        }

        protected override void OnRelease()
        {
        }

        public override bool ReadBuffer(byte[] buffer)
        {
            try
            {
                this.stream.Read(buffer, 0, buffer.Length);
                return true;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("ReadBuffer异常", ex);
                return false;
            }
        }

        public override bool ReadLine(out string line)
        {
            line = null;
            try
            {
                line = this.reader.ReadLine();
                return true;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("ReadLine异常", ex);
                return false;
            }
        }

        public override bool TestConnection()
        {
            return this.stream != null && this.reader != null && this.writer != null;
        }

        public override bool WriteBuffer(byte[] buffer)
        {
            try
            {
                this.stream.Write(buffer, 0, buffer.Length);
                return true;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("WriteBuffer异常", ex);
                return false;
            }
        }

        public override bool WriteLine(string line)
        {
            try
            {
                this.writer.WriteLine(line);
                return true;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("WriteLine异常", ex);
                return false;
            }
        }

        #endregion

        public void SetStream(Stream stream)
        {
            logger.InfoFormat("SetStream:{0}", base.Name);
            this.stream = stream;
            this.writer = new StreamWriter(this.stream);
            this.reader = new StreamReader(this.stream);
        }
    }
}