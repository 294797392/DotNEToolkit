using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.DirectX.Direct2D
{
    /// <summary>
    /// Describes the antialiasing mode used for drawing text.
    /// </summary>
    public enum D2D1_TEXT_ANTIALIAS_MODE : uint
    {
        /// <summary>
        /// Render text using the current system setting.
        /// </summary>
        D2D1_TEXT_ANTIALIAS_MODE_DEFAULT = 0,

        /// <summary>
        /// Render text using ClearType.
        /// </summary>
        D2D1_TEXT_ANTIALIAS_MODE_CLEARTYPE = 1,

        /// <summary>
        /// Render text using gray-scale.
        /// </summary>
        D2D1_TEXT_ANTIALIAS_MODE_GRAYSCALE = 2,

        /// <summary>
        /// Render text aliased.
        /// </summary>
        D2D1_TEXT_ANTIALIAS_MODE_ALIASED = 3,
        D2D1_TEXT_ANTIALIAS_MODE_FORCE_DWORD = 0xffffffff
    }
}