using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Media
{
    public class WaveAPIPlay : AudioPlay
    {
        public override int Initialize(IDictionary parameters)
        {
            return base.Initialize(parameters);
        }

        public override void Release()
        {
            base.Release();
        }

        public override int PlayFile(string fileURI)
        {
            throw new NotImplementedException();
        }
    }
}
