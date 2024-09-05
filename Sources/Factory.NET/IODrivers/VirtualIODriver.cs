using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Factory.NET.IODrivers
{
    public class VirtualIODriver : AbstractIODriver
    {
        public override IODriverTypes Type { get { return IODriverTypes.VirtualDevice; } }

        protected override int OnInitialize()
        {
            return ResponseCode.SUCCESS;
        }

        protected override void OnRelease()
        {
        }

        public override int ReadBytes(byte[] bytes, int offset, int len)
        {
            return len;
        }

        public override void ClearExisting()
        {
        }

        public override int ReadLine(out string line)
        {
            line = string.Empty;
            return ResponseCode.SUCCESS;
        }

        public override void WriteBytes(byte[] bytes)
        {
        }

        public override void WriteLine(string line)
        {
        }
    }
}
