using DotNEToolkit.Media;
using DotNEToolkit.Media.Video;
using DotNEToolkit.SDK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DotNEToolkitDemo.Forms
{
    public partial class libVLCForm : Form
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger("libVLCForm");

        private VideoPlay videoPlay;

        private FileStream h264Stream;
        private libvlc.libvlc_media_open_cb libvlc_media_open_func;
        private libvlc.libvlc_media_close_cb libvlc_media_close_func;
        private libvlc.libvlc_media_read_cb libvlc_media_read_func;
        private libvlc.libvlc_media_seek_cb libvlc_media_seek_func;

        public libVLCForm()
        {
            InitializeComponent();

            this.InitializeForm();
        }

        private void InitializeForm()
        {
            Dictionary<string, object> settings = new Dictionary<string, object>();

            this.videoPlay = VideoPlayFactory.Create(VideoPlayType.libvlc);
            this.videoPlay.Hwnd = this.Handle;
            this.videoPlay.Initialize();
            this.videoPlay.Start();

            this.h264Stream = new FileStream("h264", FileMode.Open, FileAccess.Read);

            Task.Factory.StartNew(() => 
            {
                while (true)
                {
                    byte[] buffer = new byte[163840];
                    int len = this.h264Stream.Read(buffer, 0, buffer.Length);
                    if (len == 0)
                    {
                        break;
                    }
                    this.videoPlay.Write(buffer);
                }
            });

            //this.libvlc_media_open_func = new libvlc.libvlc_media_open_cb(this.libvlc_media_open_cb);
            //this.libvlc_media_close_func = new libvlc.libvlc_media_close_cb(this.libvlc_media_close_cb);
            //this.libvlc_media_read_func = new libvlc.libvlc_media_read_cb(this.libvlc_media_read_cb);
            //this.libvlc_media_seek_func = new libvlc.libvlc_media_seek_cb(this.libvlc_media_seek_cb);

            //IntPtr hwnd = this.Handle;
            //IntPtr libvlc_instance_t = libvlc.libvlc_new(0, IntPtr.Zero);
            //IntPtr libvlc_media_t = libvlc.libvlc_media_new_callbacks(libvlc_instance_t, this.libvlc_media_open_func, this.libvlc_media_read_func, this.libvlc_media_seek_func, this.libvlc_media_close_func, IntPtr.Zero);
            //IntPtr libvlc_media_player_t = libvlc.libvlc_media_player_new_from_media(libvlc_media_t);
            //libvlc.libvlc_media_player_set_hwnd(libvlc_media_player_t, hwnd);
            //libvlc.libvlc_media_player_play(libvlc_media_player_t);
        }

        #region 事件处理器

        public int libvlc_media_open_cb(IntPtr opaque, out IntPtr datap, out ulong sizep)
        {
            logger.Info("libvlc_media_open_cb");
            datap = IntPtr.Zero;
            sizep = UInt64.MaxValue;// (ulong)this.h264Stream.Length;
            return 0;
        }

        public ulong libvlc_media_read_cb(IntPtr opaque, IntPtr buf, int len)
        {
            logger.InfoFormat("libvlc_media_read_cb, byteSize = {0}", len);
            byte[] buffer = new byte[len];
            this.h264Stream.Read(buffer, 0, buffer.Length);
            Marshal.Copy(buffer, 0, buf, buffer.Length);
            return (ulong)len;
        }

        public int libvlc_media_seek_cb(IntPtr opaque, ulong offset)
        {
            logger.Info("libvlc_media_seek_cb");
            this.h264Stream.Seek((long)offset, SeekOrigin.Begin);
            return 0;
        }

        public void libvlc_media_close_cb(IntPtr opaque)
        {
            logger.Info("libvlc_media_close_cb");
        }

        #endregion
    }
}
