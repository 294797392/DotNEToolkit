using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Modular
{
    public interface IConfigReader
    {
        /// <summary>
        /// 初始化Reader
        /// </summary>
        /// <param name="configPath"></param>
        void Initialize(string configPath);

        /// <summary>
        /// 读取配置项
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        object ReadValue(string key);
    }
}
