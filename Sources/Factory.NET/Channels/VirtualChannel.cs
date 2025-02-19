using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Factory.NET.Channels
{
    public class VirtualChannel : ChannelBase
    {
        public override ChannelTypes Type { get { return ChannelTypes.VirtualDevice; } }

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

        public override byte[] ReadBytesFull(int size)
        {
            return new byte[size];
        }

        public override void ClearExisting()
        {
        }

        public override string ReadLine()
        {
            return string.Empty;
        }

        public override void WriteBytes(byte[] bytes)
        {
        }

        public override void WriteLine(string line)
        {
        }
    }
}
