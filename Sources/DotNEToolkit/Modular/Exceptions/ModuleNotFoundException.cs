using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Modular.Exceptions
{
    public class ModuleNotFoundException : Exception
    {
        public ModuleNotFoundException(ModuleDefinition module)
        {
            
        }
    }
}
