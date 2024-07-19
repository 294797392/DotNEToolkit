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
    public abstract class AppModule : ModuleBase
    {
        /// <summary>
        /// 当App初始化完毕（Initialize方法执行结束）的时候触发
        /// </summary>
        /// <returns></returns>
        public abstract int OnAppInitialized();
    }
}
