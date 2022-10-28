using DotNEToolkit.Modular;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.Media
{
    public abstract class MediaPlay : EventableModule
    {
        protected override int OnInitialize()
        {
            return DotNETCode.SUCCESS;
        }

        protected override void OnRelease()
        {
        }

        /// <summary>
        /// 开始播放
        /// </summary>
        /// <returns></returns>
        public abstract int Start();

        /// <summary>
        /// 停止播放
        /// </summary>
        public abstract void Stop();
    }
}
