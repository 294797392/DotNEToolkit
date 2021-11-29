using DotNEToolkit.Modular;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Expressions
{
    /// <summary>
    /// 表达式求值程序定义
    /// </summary>
    public class ExpressionDefinition : ModuleDefinition
    {
        /// <summary>
        /// 表达式语法
        /// </summary>
        [JsonProperty("Syntax")]
        public string Syntax { get; set; }

        /// <summary>
        /// 表达式最少要拥有多少个参数才能计算
        /// 如果在计算的时候小于这个值，那么就返回计算失败
        /// </summary>
        [JsonProperty("MinimalParameters")]
        public int MinimalParameters { get; set; }

        public override string ToString()
        {
            return string.Format("{0}:{1}:{2}", this.Name, this.Description, this.ClassName);
        }
    }
}
