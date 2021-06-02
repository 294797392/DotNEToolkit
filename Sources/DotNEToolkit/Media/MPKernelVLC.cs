using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Media
{
    internal class MPKernelVLC : MPKernel
    {
        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("VLCPlayer");

        #endregion

        #region MediaPlayer

        public override void Close()
        {
            throw new NotImplementedException();
        }

        public override int FastForward(TimeSpan ts)
        {
            throw new NotImplementedException();
        }

        public override int FastRewind(TimeSpan ts)
        {
            throw new NotImplementedException();
        }

        public override int GetDuration()
        {
            throw new NotImplementedException();
        }

        public override int Open(string uri)
        {
            throw new NotImplementedException();
        }

        public override int Pause()
        {
            throw new NotImplementedException();
        }

        public override int Play()
        {
            throw new NotImplementedException();
        }

        public override int Resume()
        {
            throw new NotImplementedException();
        }

        public override int Stop()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
