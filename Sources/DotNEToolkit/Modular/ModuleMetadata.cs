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
        /// <summary>
        /// 元数据唯一标志符
        /// </summary>
        [JsonProperty("TypeID")]
        public string ID { get; set; }

        /// <summary>
        /// 模块的完整类型名
        /// </summary>
        [JsonProperty("EntryClass")]
        public string ClassName { get; set; }
    }
}
