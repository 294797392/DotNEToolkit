using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.DataModels
{
    /// <summary>
    /// ID是int类型的数据模型
    /// </summary>
    public class ModelBaseInt
    {
        /// <summary>
        /// 唯一编号
        /// </summary>
        [JsonProperty("id")]
        public int ID { get; set; }

        /// <summary>
        /// 名字
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [JsonProperty("creationTime")]
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// 创建者
        /// </summary>
        [JsonProperty("creator")]
        public string Creator { get; set; }
    }
}
