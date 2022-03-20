using DotNEToolkit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Factory.NET.Modules
{
    /// <summary>
    /// 表示一张要存储的图片
    /// </summary>
    public class Image
    {
        /// <summary>
        /// 保存图片的完整路径
        /// </summary>
        public string FullPath { get; set; }

        /// <summary>
        /// 要保存的图片的格式
        /// </summary>
        public ImageFormat Format { get; set; }

        /// <summary>
        /// 图片数据
        /// </summary>
        public byte[] ImageBytes { get; set; }
    }

    /// <summary>
    /// 实现一个在独立的进程中运行的图片保存模块
    /// </summary>
    public class ImageDisk : DotNEToolkit.Modular.AbstractHostedModule
    {
        #region 实例变量

        private BufferQueue<Image> imageQueue;
        private Thread imageQueueThread;

        #endregion

        protected override int OnInitialize(IDictionary parameters)
        {
            this.imageQueue = new BufferQueue<Image>();
            this.imageQueueThread = new Thread(this.ImageQueueThreadProc);
            this.imageQueueThread.IsBackground = true;
            this.imageQueueThread.Start();

            return DotNETCode.SUCCESS;
        }

        protected override void OnRelease()
        {
        }

        /// <summary>
        /// 当上位机发送保存图片的命令过来的时候触发
        /// </summary>
        /// <param name="cmdType"></param>
        /// <param name="cmdParam"></param>
        public override void OnClientDataReceived(int cmdType, object cmdParam)
        {
            Image image = cmdParam as Image;
            this.imageQueue.Enqueue(image);
        }

        #region 事件处理器

        private void ImageQueueThreadProc()
        {
            while (true)
            {
                Image image = this.imageQueue.Dequeue();

            }
        }

        #endregion
    }
}
