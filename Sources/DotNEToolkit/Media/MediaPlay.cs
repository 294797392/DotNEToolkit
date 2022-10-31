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
        protected AVFormats format;

        /// <summary>
        /// 该播放器支持的格式
        /// </summary>
        public AVFormats Format { get { return this.format; } }

        protected override int OnInitialize()
        {
            this.format = this.GetInputValue<AVFormats>("format", AVFormats.Unkown);

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
