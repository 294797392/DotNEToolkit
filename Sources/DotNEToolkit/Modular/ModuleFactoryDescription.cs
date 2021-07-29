using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Modular
{
    /// <summary>
    /// 定义ModuleFactory的配置
    /// </summary>
    public class ModuleFactoryDescription
    {
        [JsonProperty("Modules")]
        public List<ModuleDefinition> ModuleList { get; set; }

        public ModuleFactoryDescription()
        {
            this.ModuleList = new List<ModuleDefinition>();
        }
    }
}
