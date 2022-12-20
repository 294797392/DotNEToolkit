using DotNEToolkit.Modular;
using DotNEToolkit.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotNEToolkit.Media.Video
{
    /// <summary>
    /// 封装vlc视频播放器
    /// 调用libvlc_media_player_stop的时候libvlc_media_read_cb必须返回-1，不然会发生死锁现象
    /// </summary>
    public class libvlcPlay : VideoPlay
    {
        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("libvlcPlay");

        #endregion

        #region 实例变量

        private IntPtr libvlc_instance_t;
        private IntPtr libvlc_media_t;
        private IntPtr libvlc_media_player_t;

        private libvlc.libvlc_media_open_cb libvlc_media_open_func;
        private libvlc.libvlc_media_close_cb libvlc_media_close_func;
        private libvlc.libvlc_media_read_cb libvlc_media_read_func;
        private libvlc.libvlc_media_seek_cb libvlc_media_seek_func;

        #endregion

        public IntPtr libvlc_media_player
        {
            get { return this.libvlc_media_player_t; }
        }

        #region VideoPlay

        protected override int OnInitialize()
        {
            base.OnInitialize();

            this.libvlc_media_open_func = new libvlc.libvlc_media_open_cb(this.libvlc_media_open_cb);
            this.libvlc_media_close_func = new libvlc.libvlc_media_close_cb(this.libvlc_media_close_cb);
            this.libvlc_media_read_func = new libvlc.libvlc_media_read_cb(this.libvlc_media_read_cb);
            this.libvlc_media_seek_func = new libvlc.libvlc_media_seek_cb(this.libvlc_media_seek_cb);

            return DotNETCode.SUCCESS;
        }

        protected override void OnRelease()
        {
            base.OnRelease();
        }

        public override int Start()
        {
            this.videoStream.IsClosed = false;
            this.libvlc_instance_t = libvlc.libvlc_new(0, IntPtr.Zero);
            this.libvlc_media_t = libvlc.libvlc_media_new_callbacks(libvlc_instance_t, this.libvlc_media_open_func, this.libvlc_media_read_func, this.libvlc_media_seek_func, this.libvlc_media_close_func, IntPtr.Zero);
            this.AddVlcOptions(this.libvlc_media_t);
            this.libvlc_media_player_t = libvlc.libvlc_media_player_new_from_media(libvlc_media_t);
            libvlc.libvlc_media_player_set_hwnd(libvlc_media_player_t, this.Hwnd);
            libvlc.libvlc_media_player_play(libvlc_media_player_t);

            return DotNETCode.SUCCESS;
        }

        public override void Stop()
        {
            this.videoStream.IsClosed = true;
            libvlc.libvlc_media_release(this.libvlc_media_t);
            this.libvlc_media_t = IntPtr.Zero;
            libvlc.libvlc_media_player_stop(this.libvlc_media_player_t);
            libvlc.libvlc_media_player_release(this.libvlc_media_player_t);
            this.libvlc_media_player_t = IntPtr.Zero;
            libvlc.libvlc_release(this.libvlc_instance_t);
            this.libvlc_instance_t = IntPtr.Zero;
        }

        #endregion

        #region 实例方法

        private void AddVlcOptions(IntPtr libvlc_media_t)
        {
            switch (this.format)
            {
                case AVFormats.Unkown:
                    {
                        break;
                    }

                case AVFormats.H264:
                    {
                        libvlc.libvlc_media_add_option(libvlc_media_t, "demux=h264");
                        break;
                    }

                // libvlc 支持直接播放pcm数据
                // 音视频都可以播
                case AVFormats.PCM:
                    {
                        // 播放pcm格式的音频需要指定demux=rawaud，
                        // 同时需要使用--rawaud-channels和--rawaud-samplerate来指定PCM的通道数和采样频率
                        // 例：./vlc --demux=rawaud --rawaud-channels 2 --rawaud-samplerate 44100 pcm
                        libvlc.libvlc_media_add_option(libvlc_media_t, "demux=rawaud");
                        break;
                    }

                default:
                    throw new NotImplementedException();
            }

            List<string> media_options = this.GetParameter<List<string>>("media_options", null);
            if (media_options != null)
            {
                foreach (string option in media_options)
                {
                    libvlc.libvlc_media_add_option(libvlc_media_t, option);
                }
            }
        }

        #endregion

        #region 事件处理器

        private int libvlc_media_open_cb(IntPtr opaque, out IntPtr datap, out ulong sizep)
        {
            logger.Info("libvlc_media_open_cb");
            datap = IntPtr.Zero;

            // sizep赋值为MaxValue，告诉vlc不知道数据流有多长
            // 用来播放在线视频
            // 同时libvlc_media_seek_cb也就不会被调用了
            sizep = UInt64.MaxValue;
            return 0;
        }

        /**
         * Callback prototype to read data from a custom bitstream input media.
         *
         * \param opaque private pointer as set by the @ref libvlc_media_open_cb
         *               callback
         * \param buf start address of the buffer to read data into
         * \param len bytes length of the buffer
         *
         * \return strictly positive number of bytes read, 0 on end-of-stream,
         *         or -1 on non-recoverable error
         *
         * \note If no data is immediately available, then the callback should sleep.
         * \warning The application is responsible for avoiding deadlock situations.
         * In particular, the callback should return an error if playback is stopped;
         * if it does not return, then libvlc_media_player_stop() will never return.
         * 
         * 调用libvlc_media_player_stop的时候libvlc_media_read_cb必须返回-1，不然会发生死锁现象
         */
        private long libvlc_media_read_cb(IntPtr opaque, IntPtr buf, int len)
        {
            //logger.InfoFormat("libvlc_media_read_cb, byteSize = {0}", len);

            byte[] videoBytes;
            if (!this.videoStream.Read(len, this.timeout, out videoBytes))
            {
                // 返回0表示end-of-stream
                //this.NotifyStatusChanged(MediaPlayStatus.Timeout);
                logger.InfoFormat("libvlc_media_read_cb 返回 -1, Read timeout");
                return -1;
            }

            Marshal.Copy(videoBytes, 0, buf, videoBytes.Length);
            return videoBytes.Length;
        }

        private int libvlc_media_seek_cb(IntPtr opaque, ulong offset)
        {
            logger.InfoFormat("libvlc_media_seek_cb, offset = {0}", offset);
            return 0;
        }

        private void libvlc_media_close_cb(IntPtr opaque)
        {
            logger.Info("libvlc_media_close_cb");
        }

        #endregion
    }
}
