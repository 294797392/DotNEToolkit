using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace DotNEToolkit.SDK
{
    using libvlc_media_t = IntPtr;
    using libvlc_instance_t = IntPtr;
    using libvlc_media_player_t = IntPtr;

    public static class libvlc
    {
        private const string libvlcDll = "libvlc.dll";

        #region 委托

        /// <summary>
        /// 
        /// </summary>
        /// <param name="opaque">private pointer as passed to libvlc_media_new_callbacks()</param>
        /// <param name="datap">storage space for a private data pointer [OUT]</param>
        /// <param name="sizep">byte length of the bitstream or UINT64_MAX if unknown [OUT]</param>
        /// <returns>
        /// 0 on success, non-zero on error. In case of failure, the other
        /// callbacks will not be invoked and any value stored in *datap and *sizep is
        /// discarded.
        /// </returns>
        public delegate int libvlc_media_open_cb(IntPtr opaque, out IntPtr datap, out ulong sizep);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="opaque">private pointer as set by the @ref libvlc_media_open_cb callback</param>
        /// <param name="buf">start address of the buffer to read data into</param>
        /// <param name="len">bytes length of the buffer</param>
        /// <remarks>
        /// If no data is immediately available, then the callback should sleep.
        /// </remarks>
        /// <returns>
        /// strictly positive number of bytes read, 0 on end-of-stream, or -1 on non-recoverable error
        /// </returns>
        public delegate ulong libvlc_media_read_cb(IntPtr opaque, IntPtr buf, int len);

        /// <summary>
        /// Callback prototype to seek a custom bitstream input media.
        /// </summary>
        /// <param name="opaque">private pointer as set by the @ref libvlc_media_open_cb callback</param>
        /// <param name="offset">absolute byte offset to seek to</param>
        /// <returns>0 on success, -1 on error.</returns>
        public delegate int libvlc_media_seek_cb(IntPtr opaque, ulong offset);

        /// <summary>
        /// Callback prototype to seek a custom bitstream input media.
        /// </summary>
        /// <param name="opaque">private pointer as set by the @ref libvlc_media_open_cb callback</param>
        public delegate void libvlc_media_close_cb(IntPtr opaque);

        #endregion

        public enum libvlc_state_t
        {
            libvlc_NothingSpecial = 0,
            libvlc_Opening,
            libvlc_Buffering,
            libvlc_Playing,
            libvlc_Paused,
            libvlc_Stopped,
            libvlc_Ended,
            libvlc_Error
        }

        [DllImport(libvlcDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern libvlc_state_t libvlc_media_get_state(IntPtr p_md);

        [DllImport(libvlcDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern libvlc_instance_t libvlc_new(int argc, IntPtr argv);

        [DllImport(libvlcDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_release(libvlc_instance_t p_instnace);

        [DllImport(libvlcDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_media_new_location(IntPtr vlcptr, string url);

        [DllImport(libvlcDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_media_release(IntPtr mediaPtr);

        [DllImport(libvlcDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_media_player_new_from_media(libvlc_media_t linvlc_media_t);

        [DllImport(libvlcDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_media_player_release(IntPtr mediaPlayerPtr);

        [DllImport(libvlcDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_media_player_play(IntPtr player_ptr);

        [DllImport(libvlcDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_media_player_stop(IntPtr player_ptr);

        [DllImport(libvlcDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_media_player_new(IntPtr vlcPtr);

        [DllImport(libvlcDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_player_set_hwnd(IntPtr player_ptr, IntPtr drawable);

        [DllImport(libvlcDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_media_new_path(IntPtr vlcPtr, string uri);

        [DllImport(libvlcDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_media_player_set_media(IntPtr mediaPlayerPtr, IntPtr mediaPtr);

        [DllImport(libvlcDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_video_set_mouse_input(IntPtr mediaPlayerPtr, uint on);

        [DllImport(libvlcDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_add_option(libvlc_media_t libvlc_media_t, string psz_options);

        [DllImport(libvlcDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern libvlc_media_t libvlc_media_new_callbacks(libvlc_instance_t instance,
                                                                        libvlc_media_open_cb open_cb,
                                                                        libvlc_media_read_cb read_cb,
                                                                        libvlc_media_seek_cb seek_cb,
                                                                        libvlc_media_close_cb close_cb,
                                                                        IntPtr opaque);

        /// <summary>
        /// Set the video scaling factor。
        /// Zero is a special value; it will adjust the video to the output
        /// window/drawable(in windowed mode) or the entire screen.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="f_factor"></param>
        [DllImport(libvlcDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_video_set_scale(libvlc_media_player_t player, float f_factor);

        /// <summary>
        /// Get the current video scaling factor.
        /// </summary>
        /// <param name="player"></param>
        /// <returns>
        /// the currently configured zoom factor, or 0. if the video is set
        /// to fit to the output window/drawable automatically.
        /// </returns>
        [DllImport(libvlcDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern float libvlc_video_get_scale(libvlc_media_player_t player);

        /// <summary>
        /// 封装libvlc的快速播放逻辑，传递一个窗口句柄即可
        /// 带自动重连功能
        /// </summary>
        /// <param name="drawable"></param>
        public static void QuickPlay(string uri, IntPtr drawable)
        {
            IntPtr vlcPtr = libvlc.libvlc_new(0, IntPtr.Zero);

            IntPtr vlcMediaPtr = libvlc.libvlc_media_new_location(vlcPtr, uri);
            IntPtr vlcMediaPlayerPtr = libvlc.libvlc_media_player_new_from_media(vlcMediaPtr);
            libvlc.libvlc_media_player_set_hwnd(vlcMediaPlayerPtr, drawable);

            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    libvlc_state_t state = libvlc_media_get_state(vlcMediaPtr);

                    //Console.WriteLine("vlc_state = {0}", state);

                    switch (state)
                    {
                        case libvlc_state_t.libvlc_Stopped:
                        case libvlc_state_t.libvlc_NothingSpecial:
                        case libvlc_state_t.libvlc_Error:
                            {
                                libvlc_media_player_stop(vlcMediaPlayerPtr);
                                libvlc.libvlc_media_player_play(vlcMediaPlayerPtr);
                                break;
                            }

                        case libvlc_state_t.libvlc_Ended:
                            {
                                // 停止和刷新控件
                                libvlc_media_player_stop(vlcMediaPlayerPtr);
                                Win32API.UpdateWindow(drawable);
                                break;
                            }

                        default:
                            {
                                break;
                            }
                    }

                    Thread.Sleep(1000);
                }
            });
        }
    }
}
