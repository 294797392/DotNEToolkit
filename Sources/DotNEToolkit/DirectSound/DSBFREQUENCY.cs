using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.DirectSound
{
    public class DSBFREQUENCY
    {
        public static readonly uint DSBFREQUENCY_ORIGINAL = 0;
        public static readonly uint DSBFREQUENCY_MIN = 100;
#if DIRECTSOUND_VERSION_0x0900
        public static readonly uint DSBFREQUENCY_MAX = 200000;
#else
        public static readonly uint DSBFREQUENCY_MAX = 100000;
#endif
    }
}
