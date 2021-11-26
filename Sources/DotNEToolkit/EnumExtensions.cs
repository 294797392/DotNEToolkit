using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Extentions
{
    public static class EnumExtensions
    {
        public static int UnsetFlag(this int source, int toUnset)
        {
            return source &= ~toUnset;
        }
    }
}
