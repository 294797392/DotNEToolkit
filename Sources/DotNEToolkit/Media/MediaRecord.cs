using DotNEToolkit.Modular;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.Media
{
    public abstract class MediaRecord : ModuleBase
    {
        /// <summary>
        /// 开始录制
        /// </summary>
        /// <returns></returns>
        public abstract int Start();

        /// <summary>
        /// 结束录制
        /// </summary>
        public abstract void Stop();
    }
}
