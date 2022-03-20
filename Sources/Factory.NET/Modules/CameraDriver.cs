using DotNEToolkit.Modular;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Factory.NET.Modules
{
    public interface IPhoto
    {
    }

    /// <summary>
    /// 封装相机接口
    /// </summary>
    public abstract class CameraDriver : ModuleBase
    {
        /// <summary>
        /// 执行取图动作
        /// </summary>
        /// <param name="photo"></param>
        /// <returns></returns>
        public abstract int TakePhoto(out IPhoto photo);

        /// <summary>
        /// 释放图片
        /// </summary>
        /// <param name="photo"></param>
        public abstract void ReleasePhoto(IPhoto photo);

        /// <summary>
        /// 把CameraDriver拍出来的字节数组转换成Bitmap
        /// </summary>
        /// <param name="imageBytes"></param>
        /// <returns></returns>
        public abstract Bitmap ConvertBitmap(byte[] imageBytes);
    }
}
