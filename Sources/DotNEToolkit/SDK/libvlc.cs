using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace DotNEToolkit.SDK
{
    public static class libvlc
    {
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

        [DllImport("libvlc.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern libvlc_state_t libvlc_media_get_state(IntPtr p_md);

        [DllImport("libvlc.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_new(int argc, IntPtr argv);

        [DllImport("libvlc.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_media_new_location(IntPtr vlcptr, string url);

        [DllImport("libvlc.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_media_release(IntPtr mediaPtr);

        [DllImport("libvlc.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_media_player_new_from_media(IntPtr media_ptr);

        [DllImport("libvlc.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_media_player_release(IntPtr mediaPlayerPtr);

        [DllImport("libvlc.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_media_player_play(IntPtr player_ptr);

        [DllImport("libvlc.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_media_player_stop(IntPtr player_ptr);

        [DllImport("libvlc.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_media_player_new(IntPtr vlcPtr);

        [DllImport("libvlc.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_player_set_hwnd(IntPtr player_ptr, IntPtr drawable);

        [DllImport("libvlc.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_media_new_path(IntPtr vlcPtr, string uri);

        [DllImport("libvlc.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_media_player_set_media(IntPtr mediaPlayerPtr, IntPtr mediaPtr);

        [DllImport("libvlc.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_video_set_mouse_input(IntPtr mediaPlayerPtr, uint on);



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
