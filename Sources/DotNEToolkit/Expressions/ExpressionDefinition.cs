using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Expressions
{
    /// <summary>
    /// 表达式类型定义
    /// </summary>
    public class ExpressionDefinition
    {
        /// <summary>
        /// 模块名字
        /// </summary>
        [JsonProperty("Name")]
        public string Name { get; set; }

        /// <summary>
        /// 表达式描述
        /// </summary>
        [JsonProperty("Description")]
        public string Description { get; set; }

        /// <summary>
        /// 表达式语法
        /// </summary>
        [JsonProperty("Syntax")]
        public string Syntax { get; set; }

        /// <summary>
        /// 入口类名
        /// </summary>
        [JsonProperty("ClassName")]
        public string ClassName { get; set; }

        /// <summary>
        /// 模块类别
        /// </summary>
        [JsonProperty("Category")]
        public ExpressionCategories Category { get; set; }

        /// <summary>
        /// 表达式最少要拥有多少个参数才能计算
        /// 如果在计算的时候小于这个值，那么就返回计算失败
        /// </summary>
        [JsonProperty("MinimalParameters")]
        public int MinimalParameters { get; set; }

        public override string ToString()
        {
            return string.Format("{0}:{1}:{2}:{3}", this.Name, this.Description, this.ClassName, this.Category);
        }
    }
}
