using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.Modular
{
    /// <summary>
    /// 在Module基础上实现一个ModularApp里的Module
    /// </summary>
    /// <typeparam name="TApp"></typeparam>
    /// <typeparam name="TManifest"></typeparam>
    public abstract class AppModule<TManifest> : ModuleBase
        where TManifest : AppManifest
    {
        /// <summary>
        /// 该Module所属的AppManifest
        /// </summary>
        public TManifest AppManifest { get; internal set; }

        /// <summary>
        /// 当App初始化完毕（Initialize方法执行结束）的时候触发
        /// </summary>
        /// <returns></returns>
        public abstract int OnAppInitialized();
    }
}
