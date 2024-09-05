using DotNEToolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Factory.NET
{
    /// <summary>
    /// 产测软件使用的App
    /// 封装测试流程逻辑
    /// </summary>
    public abstract class FactoryApp<TFactoryApp, TFactoryAppManifest> : ModularApp<TFactoryApp, TFactoryAppManifest>
        where TFactoryApp : class
        where TFactoryAppManifest : AppManifest
    {
        protected override int OnInitialized()
        {
            return ResponseCode.SUCCESS;
        }

        protected override void OnRelease()
        {
        }
    }
}
