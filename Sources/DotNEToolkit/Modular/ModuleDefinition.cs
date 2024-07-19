using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.Modular
{
    /// <summary>
    /// 存储模块的信息
    /// </summary>
    [JsonObject("Module")]
    public class ModuleDefinition
    {
        /// <summary>
        /// 模块ID
        /// </summary>
        [JsonProperty("ID")]
        public string ID { get; set; }

        /// <summary>
        /// 模块名字
        /// </summary>
        [JsonProperty("Name")]
        public string Name { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [JsonProperty("Description")]
        public string Description { get; set; }

        /// <summary>
        /// 组件类型ID
        /// </summary>
        [JsonProperty("MetadataID")]
        public string MetadataID { get; set; }

        /// <summary>
        /// 模块的完整类型名
        /// 如果该值不为空，那么优先根据该值去创建实例
        /// 如果该值为空，那么会根据MetadataID去创建实例
        /// </summary>
        [JsonProperty("ClassName")]
        public string ClassName { get; set; }

        /// <summary>
        /// 模块的标志位
        /// </summary>
        [JsonProperty("Flags")]
        public long Flags { get; set; }

        /// <summary>
        /// 输入配置参数
        /// 执行模块的构造方法的时候，会动态把所依赖的模块实例反射并赋值给该模块的属性
        /// 避免了每次都要调用LookupModule去查找模块，麻烦
        /// </summary>
        [JsonProperty("Inputs")]
        public IDictionary InputParameters { get; private set; }

        /// <summary>
        /// 是否异步加载该模块
        /// 异步加载的情况下，如果该模块加载失败则会重试加载
        /// </summary>
        [JsonProperty("AsyncInit")]
        public bool AsyncInit { get; set; }

        /// <summary>
        /// 当异步加载失败之后，重新加载的间隔时间，单位毫秒
        /// </summary>
        [JsonProperty("AsyncInitInterval")]
        public int AsyncInitInterval { get; set; }

        /// <summary>
        /// 该模块所依赖的其他模块ID
        /// 模块工厂会先初始化依赖的模块
        /// </summary>
        //[JsonProperty("References")]
        //public List<string> References { get; private set; }

        /// <summary>
        /// 构造方法
        /// </summary>
        public ModuleDefinition()
        {
            this.ID = Guid.NewGuid().ToString();
            this.InputParameters = new Dictionary<string, object>();
            this.AsyncInitInterval = 2000; // 默认2秒钟重新加载
            //this.References = new List<string>();
        }

        /// <summary>
        /// 判断该模块是否包含指定的选项
        /// </summary>
        /// <param name="flag"></param>
        /// <returns></returns>
        public bool HasFlag(int flag)
        {
            return (this.Flags & flag) == flag;
        }

        /// <summary>
        /// 设置该模块的选项
        /// </summary>
        /// <param name="flag"></param>
        public void SetFlag(int flag)
        {
            this.Flags |= (uint)flag;
        }

        /// <summary>
        /// 克隆一份输入参数
        /// </summary>
        /// <returns></returns>
        public IDictionary CloneInputParameters()
        {
            string json = JsonConvert.SerializeObject(this.InputParameters);
            return JsonConvert.DeserializeObject<IDictionary>(json);
        }
    }
}
