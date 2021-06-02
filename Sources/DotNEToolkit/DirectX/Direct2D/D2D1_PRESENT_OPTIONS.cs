using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.DirectX.Direct2D
{
    /// <summary>
    /// Describes how present should behave.
    /// </summary>
    public enum D2D1_PRESENT_OPTIONS : uint
    {
        D2D1_PRESENT_OPTIONS_NONE = 0x00000000,

        /// <summary>
        /// Keep the target contents intact through present.
        /// </summary>
        D2D1_PRESENT_OPTIONS_RETAIN_CONTENTS = 0x00000001,

        /// <summary>
        /// Do not wait for display refresh to commit changes to display.
        /// </summary>
        D2D1_PRESENT_OPTIONS_IMMEDIATELY = 0x00000002,
        D2D1_PRESENT_OPTIONS_FORCE_DWORD = 0xffffffff
    }
}