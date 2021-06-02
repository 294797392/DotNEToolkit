using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.Modular
{
    /// <summary>
    /// 存储模块的元数据信息
    /// </summary>
    [JsonObject("ModuleMetadata")]
    public sealed class ModuleMetadata
    {
        [JsonProperty("TypeID")]
        public string ID { get; set; }

        [JsonProperty("EntryClass")]
        public string EntryClass { get; set; }
    }
}
