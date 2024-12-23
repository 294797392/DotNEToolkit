using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit
{
    public enum WriteOptions
    {
        /// <summary>
        /// 创建一个新文件并写入
        /// 如果源文件存在，那么覆盖源文件
        /// </summary>
        CreateNew,

        /// <summary>
        /// 向源文件追加新文件
        /// </summary>
        Append,
    }
}
