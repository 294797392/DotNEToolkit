using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.DatabaseSvc
{
    /// <summary>
    /// 数据库服务配置
    /// </summary>
    public sealed class DatabaseSVConfig
    {
        /// <summary>
        /// 数据库配置
        /// </summary>
        [JsonProperty("db_config")]
        public Dictionary<string, object> DatabaseConfig { get; set; }

        /// <summary>
        /// 服务配置
        /// </summary>
        [JsonProperty("svc_config")]
        public Dictionary<string, object> ServiceConfig { get; set; }

        /// <summary>
        /// 存放数据库模型的命名空间
        /// </summary>
        public string[] Namespaces { get; set; }

        /// <summary>
        /// 服务的根目录
        /// </summary>
        public string RootPath { get; set; }

        /// <summary>
        /// 默认路径和自定义路径的映射关系
        /// </summary>
        public Dictionary<string, string> PathMapping { get; set; }
    }
}