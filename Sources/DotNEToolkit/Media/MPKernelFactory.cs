using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Media
{
    public static class MPKernelFactory
    {
        public static MPKernel Create(MPKernels k)
        {
            switch (k)
            {
                case MPKernels.VLC: return new MPKernelVLC();

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
