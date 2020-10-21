using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.DirectX.DirectSound
{
    /// <summary>
    /// DirectSound return values
    /// </summary>
    public class DSERR
    {
        public const int DS_OK = 0x00000000;

        public const int DSERR_BUFFERLOST = 0;// 0x88780096;

        public const int DSERR_INVALIDPARAM = 0;// 0x80070057;
    }
}