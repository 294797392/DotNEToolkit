using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Factory.NET.Communictions
{
    /// <summary>
    /// 封装Tcp客户端通信对象
    /// </summary>
    public class TcpClientCommObject : CommunicationObject
    {
        public override int Close()
        {
            throw new NotImplementedException();
        }

        public override bool IsOpened()
        {
            throw new NotImplementedException();
        }

        public override int Open()
        {
            throw new NotImplementedException();
        }

        public override int ReadLine(out string line)
        {
            throw new NotImplementedException();
        }

        public override int WriteLine(string line)
        {
            throw new NotImplementedException();
        }

        protected override int ReadBytes(byte[] bytes, int offset, int count)
        {
            throw new NotImplementedException();
        }

        protected override int WriteBytes(byte[] bytes, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}
