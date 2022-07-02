using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.Modular
{
    public abstract class ModuleBasedContext : SingletonObject<ModuleBasedContext>
    {
        public ModuleFactory Factory { get; private set; }

        public override int Initialize()
        {
            return base.Initialize();
        }

        public override void Release()
        {
            base.Release();
        }
    }
}
