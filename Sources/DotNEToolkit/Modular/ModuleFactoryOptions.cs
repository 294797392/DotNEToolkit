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
        /// 是否使用异步加载模块
        /// 默认true
        /// </summary>
        public bool AsyncInitializing { get; set; }

        /// <summary>
        /// 要加载的模块列表
        /// </summary>
        public List<ModuleDefinition> ModuleList { get; set; }

        /// <summary>
        /// 当初始化某个模块失败的时候，会重新初始化
        /// 这个值指定重新初始化模块的间隔时间，单位是毫秒，默认2000
        /// </summary>
        public int ReInitializeInterval { get; set; }

        public ModuleFactoryOptions()
        {
            this.AsyncInitializing = true;
            this.ModuleList = new List<ModuleDefinition>();
            this.ReInitializeInterval = 2000;
        }
    }
}
