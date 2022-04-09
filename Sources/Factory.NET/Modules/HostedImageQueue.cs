using DotNEToolkit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
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
    public abstract class Image
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

        /// <summary>
        /// 图片宽度
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// 图片高度
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// 把Image转换成Bitmap
        /// </summary>
        /// <returns></returns>
        public abstract Bitmap ConvertBitmap();
    }

    /// <summary>
    /// 实现一个在独立的进程中运行的图片保存模块
    /// </summary>
    public class HostedImageQueue : DotNEToolkit.Modular.AbstractHostedModule
    {
        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("ModuleHostImageQueue");

        private const string KEY_QUEUE_SIZE = "queueSize";
        private const int DefaultQueueSize = 16384;

        #endregion

        #region 实例变量

        private BufferQueue<Image> imageQueue;
        private Thread imageQueueThread;
        private int queueSize;

        #endregion

        #region AbstractHostedModule

        protected override int OnInitialize(IDictionary parameters)
        {
            this.queueSize = parameters.GetValue<int>(KEY_QUEUE_SIZE, DefaultQueueSize);
            this.imageQueue = new BufferQueue<Image>(this.queueSize);
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
            logger.InfoFormat("收到客户端请求, {0}", cmdParam);
            //Image image = cmdParam as Image;
            //this.imageQueue.Enqueue(image);
        }

        #endregion

        #region 事件处理器

        private void ImageQueueThreadProc()
        {
            logger.InfoFormat("图片保存队列启动成功, 队列长度 = {0}", this.queueSize);

            while (true)
            {
                Image image = this.imageQueue.Dequeue();

                logger.InfoFormat("开始保存图片");
            }
        }

        #endregion
    }
}
