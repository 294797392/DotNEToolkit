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
    /// ModuleFactory通过TypeID找到ModuleMetadata，然后动态创建模块实例
    /// </summary>
    [JsonObject("ModuleMetadata")]
    public class ModuleMetadata
    {
        /// <summary>
        /// 唯一标志符
        /// </summary>
        [JsonProperty("ID")]
        public string ID { get; set; }

        /// <summary>
        /// 模块的默认的名字
        /// </summary>
        [JsonProperty("Name")]
        public string Name { get; set; }

        /// <summary>
        /// 模块的完整类型名
        /// </summary>
        [JsonProperty("ClassName")]
        public string ClassName { get; set; }
    }
}
