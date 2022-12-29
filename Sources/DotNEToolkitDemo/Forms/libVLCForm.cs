using DotNEToolkit;
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
using System.Threading;
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
            panel1.Padding = new Padding(20);
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

        private float factor;

        // 放大
        private void button1_Click(object sender, EventArgs e)
        {
            this.factor += 0.25F;
            libvlcPlay vlcPlay = this.videoPlay as libvlcPlay;
            libvlc.libvlc_video_set_scale(vlcPlay.libvlc_media_player, this.factor);
        }

        // 缩小
        private void button2_Click(object sender, EventArgs e)
        {
            this.factor -= 0.25F;
            libvlcPlay vlcPlay = this.videoPlay as libvlcPlay;
            libvlc.libvlc_video_set_scale(vlcPlay.libvlc_media_player, this.factor);
        }

        private bool isRunning = false;
        Task task = null;

        private void ButtonStart_Click(object sender, EventArgs e)
        {
            Dictionary<string, object> settings = new Dictionary<string, object>();

            string[] argv = new string[] { "verbose=2", "--verbose=2" };

            List<string> media_options = new List<string>()
                        {
                            "demux=h264",
                            "verbose=2"
                            //"h264-fps=5"
                            //"codec=ffmpeg"
                        };

            this.videoPlay = VideoPlayFactory.Create(VideoPlayType.libvlc);
            this.videoPlay.Hwnd = panel1.Handle;
            this.videoPlay.SetParameter<string[]>("argv", argv);
            this.videoPlay.SetParameter<AVFormats>("format", AVFormats.H264);
            this.videoPlay.SetParameter<int>("timeout", 999999);
            this.videoPlay.SetParameter<List<string>>("media_options", media_options);
            this.videoPlay.Initialize();
            this.videoPlay.Start();

            byte[] videoBytes = File.ReadAllBytes("test");
            this.isRunning = true;

            task = Task.Factory.StartNew(() =>
            {
                int i = 0;
                while (isRunning)
                {
                    this.videoPlay.Write(videoBytes, 0, videoBytes.Length);
                    Thread.Sleep(10000);
                    //i += 4096;
                    //if (i >= videoBytes.Length)
                    //{
                    //    i = 0;
                    //}
                    Console.WriteLine("写数据");
                }
            });
        }

        private void ButtonStop_Click(object sender, EventArgs e)
        {
            this.isRunning = false;
            task.Wait();
            this.videoPlay.Stop();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Bitmap bitmap = new Bitmap(200, 400);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.DrawString("123456", this.Font, Brushes.Red, PointF.Empty);
                bitmap.Save("5.png", System.Drawing.Imaging.ImageFormat.Png);
            }

            libvlcPlay vlcPlay = this.videoPlay as libvlcPlay;
            byte[] value = Encoding.ASCII.GetBytes("5.png");
            libvlc.libvlc_video_set_logo_string(vlcPlay.libvlc_media_player, (uint)libvlc.libvlc_video_logo_option_t.libvlc_logo_file, value);
            libvlc.libvlc_video_set_logo_int(vlcPlay.libvlc_media_player, (uint)libvlc.libvlc_video_logo_option_t.libvlc_logo_enable, 1);
            libvlc.libvlc_video_set_logo_int(vlcPlay.libvlc_media_player, (uint)libvlc.libvlc_video_logo_option_t.libvlc_logo_x, 100);
            libvlc.libvlc_video_set_logo_int(vlcPlay.libvlc_media_player, (uint)libvlc.libvlc_video_logo_option_t.libvlc_logo_y, 100);
        }

        private void panel1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("panel1被点击");
        }

        private void ButtonEnumChildWindow_Click(object sender, EventArgs e)
        {
            UserControl1 uc = new UserControl1() { Width = 100, Height = 100, Text = "阿苏京东会员奥斯卡接电话卡死华东科技啊是" };
            //Button uc = new Button() { Width = 100, Height = 100 };
            this.Controls.Add(uc);
            uc.BringToFront();
            uc.Location = new Point(300, 300);

            //label1.Location = new Point(300, 300);
            //label1.BackColor = Color.Transparent;

            //Label l = new Label();
            //l.Text = "hello vlcdwqewe空数据啊好的喀什的控件啊还是肯德基安徽省考就或多或少 好的卡就是华东科技啊是好";
            //l.ForeColor = Color.Red;
            //l.Size = new Size(200, 200);
            //panel1.Controls.Add(l);
            //l.BringToFront();

            //WinUser.SetParent(l.Handle, this.lastChild);
        }

        private void buttonSetMarquueText_Click(object sender, EventArgs e)
        {
            //string text = "123456";
            string text = "你好\r\n你好2\r\n你好3";

            libvlcPlay vlcPlay = this.videoPlay as libvlcPlay;
            libvlc.libvlc_video_set_marquee_string(vlcPlay.libvlc_media_player, (uint)libvlc.libvlc_video_marquee_option_t.libvlc_marquee_Text, Encoding.UTF8.GetBytes(text));
            libvlc.libvlc_video_set_marquee_int(vlcPlay.libvlc_media_player, (uint)libvlc.libvlc_video_marquee_option_t.libvlc_marquee_Enable, 1);
            libvlc.libvlc_video_set_marquee_int(vlcPlay.libvlc_media_player, (uint)libvlc.libvlc_video_marquee_option_t.libvlc_marquee_Size, 50);
            libvlc.libvlc_video_set_marquee_int(vlcPlay.libvlc_media_player, (uint)libvlc.libvlc_video_marquee_option_t.libvlc_marquee_Color, 0x4C9900);
        }
    }
}