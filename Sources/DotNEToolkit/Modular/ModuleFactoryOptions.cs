using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.Modular
{
    public class ModuleFactoryOptions
    {
        /// <summary>
        /// 要加载的模块列表
        /// </summary>
        public List<ModuleDefinition> ModuleList { get; set; }

        public ModuleFactoryOptions()
        {
            this.ModuleList = new List<ModuleDefinition>();
        }
    }
}
