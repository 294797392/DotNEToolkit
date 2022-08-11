using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.ProcessComm
{
    public class ProcessCommClientSHM : ProcessCommClient
    {
        public override int Connect(string remoteUri)
        {
            throw new NotImplementedException();
        }

        public override void Disconnect()
        {
            throw new NotImplementedException();
        }

        public override int Send(int cmdType, object instance)
        {
            throw new NotImplementedException();
        }

        public override int Send(int cmdType, string cmdParams)
        {
            throw new NotImplementedException();
        }

        public override int Send(int cmdType, byte[] cmdParams)
        {
            throw new NotImplementedException();
        }
    }
}
