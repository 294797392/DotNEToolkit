using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.DirectX.Direct2D
{
    /// <summary>
    /// The root interface for all resources in D2D.
    /// </summary>
    [Guid("2cd90691-12e2-11dc-9fed-001143a055f9")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ID2D1Resource
    {
        /// <summary>
        /// Retrieve the factory associated with this resource.
        /// </summary>
        void GetFactory(out IntPtr factory);
    }
}