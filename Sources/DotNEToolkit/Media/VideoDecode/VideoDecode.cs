using DotNEToolkit.Modular;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.Media.Video
{
    public abstract class VideoDecode : ModuleBase
    {
        public abstract int Decode(byte[] videoData, out byte[] decodeData, out int videoWidth, out int videoHeight);

        public abstract int Decode(byte[] videoData, out IntPtr decodeData, out int decodeDataSize, out int videoWidth, out int videoHeight);
    }
}
