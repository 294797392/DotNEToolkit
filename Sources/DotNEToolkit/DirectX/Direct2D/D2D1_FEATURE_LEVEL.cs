using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.DirectX.Direct2D
{
    public enum D3D_FEATURE_LEVEL
    {
        D3D_FEATURE_LEVEL_1_0_CORE = 0x1000,
        D3D_FEATURE_LEVEL_9_1 = 0x9100,
        D3D_FEATURE_LEVEL_9_2 = 0x9200,
        D3D_FEATURE_LEVEL_9_3 = 0x9300,
        D3D_FEATURE_LEVEL_10_0 = 0xa000,
        D3D_FEATURE_LEVEL_10_1 = 0xa100,
        D3D_FEATURE_LEVEL_11_0 = 0xb000,
        D3D_FEATURE_LEVEL_11_1 = 0xb100,
        D3D_FEATURE_LEVEL_12_0 = 0xc000,
        D3D_FEATURE_LEVEL_12_1 = 0xc100
    }

    /// <summary>
    /// Describes the minimum DirectX support required for hardware rendering by a
    /// render target.
    /// </summary>
    public enum D2D1_FEATURE_LEVEL : uint
    {
        /// <summary>
        /// The caller does not require a particular underlying D3D device level.
        /// </summary>
        D2D1_FEATURE_LEVEL_DEFAULT = 0,

        /// <summary>
        /// The D3D device level is DX9 compatible.
        /// </summary>
        D2D1_FEATURE_LEVEL_9 = D3D_FEATURE_LEVEL.D3D_FEATURE_LEVEL_9_1,

        /// <summary>
        /// The D3D device level is DX10 compatible.
        /// </summary>
        D2D1_FEATURE_LEVEL_10 = D3D_FEATURE_LEVEL.D3D_FEATURE_LEVEL_10_0,
        D2D1_FEATURE_LEVEL_FORCE_DWORD = 0xffffffff
    }
}