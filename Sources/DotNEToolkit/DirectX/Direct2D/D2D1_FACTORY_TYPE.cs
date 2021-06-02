using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.DirectX.Direct2D
{
    /// <summary>
    /// D2D1_FACTORY_TYPE类
    /// </summary>
    public enum D2D1_FACTORY_TYPE : uint
    {
        /// <summary>
        /// The resulting factory and derived resources may only be invoked serially.
        /// Reference counts on resources are interlocked, however, resource and render
        /// target state is not protected from multi-threaded access.
        /// </summary>
        D2D1_FACTORY_TYPE_SINGLE_THREADED = 0,

        /// <summary>
        /// The resulting factory may be invoked from multiple threads. Returned resources
        /// use interlocked reference counting and their state is protected.
        /// </summary>
        D2D1_FACTORY_TYPE_MULTI_THREADED = 1,
        D2D1_FACTORY_TYPE_FORCE_DWORD = 0xffffffff
    }
}