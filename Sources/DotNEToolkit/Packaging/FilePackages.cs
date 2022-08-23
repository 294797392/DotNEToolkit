using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.Packaging
{
    /// <summary>
    /// 定义可以打包的文件类型
    /// </summary>
    public enum FilePackages
    {
        /// <summary>
        /// Zip压缩包格式的打包
        /// 后缀名.zip
        /// </summary>
        Zip,

        /// <summary>
        /// tar包
        /// </summary>
        TarArchive
    }
}
