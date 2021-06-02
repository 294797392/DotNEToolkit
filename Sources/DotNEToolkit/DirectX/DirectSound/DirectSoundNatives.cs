using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.DirectX.DirectSound
{
    public static class DirectSoundNatives
    {
        public const uint INFINITE = 0xFFFFFFFF;
        public const uint WAIT_ABANDONED_0 = 0x00000080;
        public const uint WAIT_OBJECT_0 = 0x00000000;
        /// <summary>
        /// 等待信号失败
        /// </summary>
        public const uint WAIT_FAILED = 0xFFFFFFFF;

        /// <summary>
        /// flags for wFormatTag field of WAVEFORMAT
        /// </summary>
        public const int WAVE_FORMAT_PCM = 1;

        public const uint DSBPN_OFFSETSTOP = 0xFFFFFFFF;
        public const int DSCBSTART_LOOPING = 0x00000001;

        #region DirectSound

        /// <summary>
        /// 创建一个DirectSoundCapture8接口
        /// </summary>
        /// <param name="pcGuidDevice"></param>
        /// <param name="ppDSC8"></param>
        /// <param name="pUnkOuter"></param>
        /// <returns></returns>
        [DllImport("dsound", CallingConvention = CallingConvention.StdCall)]
        public static extern uint DirectSoundCaptureCreate8(IntPtr pcGuidDevice, out IntPtr ppDSC8, IntPtr pUnkOuter);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lpcGuidDevice">
        /// Address of the GUID that identifies the sound device
        /// DSDEVID_DefaultPlayback : System-wide default audio playback device. Equivalent to NULL. 
        /// DSDEVID_DefaultVoicePlayback : Default voice playback device. 
        /// </param>
        /// <param name="ppDS8">Address of a variable to receive an IDirectSound8 interface pointer. </param>
        /// <param name="pUnkOuter"></param>
        /// <returns></returns>
        /// <remarks>
        /// 在创建IDirectSound8接口之后必须首先调用SetCooperativeLevel
        /// </remarks>
        [DllImport("dsound", CallingConvention = CallingConvention.StdCall)]
        public static extern int DirectSoundCreate8(IntPtr lpcGuidDevice, out IntPtr ppDS8, IntPtr pUnkOuter);

        #endregion

        #region Kernel32

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr CreateEvent(IntPtr lpEventAttributes, [MarshalAs(UnmanagedType.Bool)] bool bManualReset, [MarshalAs(UnmanagedType.Bool)] bool bInitialState, string lpName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nCount">指定列表中的句柄数量  最大值为MAXIMUM_WAIT_OBJECTS（64）</param>
        /// <param name="lpHandles">柄数组的指针。lpHandles为指定对象句柄组合中的第一个元素 HANDLE类型可以为（Event，Mutex，Process，Thread，Semaphore）数组</param>
        /// <param name="bWaitAll">如果为TRUE，表示除非对象都发出信号，否则就一直等待下去；如果FALSE，表示任何对象发出信号即可</param>
        /// <param name="dwMilliseconds">指定要等候的毫秒数。如设为零，表示立即返回。如指定常数INFINITE，则可根据实际情况无限等待下去</param>
        /// <returns>
        /// WAIT_ABANDONED_0：所有对象都发出消息，而且其中有一个或多个属于互斥体（一旦拥有它们的进程中止，就会发出信号）
        /// WAIT_TIMEOUT：对象保持未发信号的状态，但规定的等待超时时间已经超过
        /// WAIT_OBJECT_0：所有对象都发出信号，WAIT_OBJECT_0是微软定义的一个宏，你就把它看成一个数字就可以了。例如，WAIT_OBJECT_0 + 5的返回结果意味着列表中的第5个对象发出了信号
        /// WAIT_IO_COMPLETION：（仅适用于WaitForMultipleObjectsEx）由于一个I/O完成操作已作好准备执行，所以造成了函数的返回
        /// 返回WAIT_FAILED则表示函数执行失败，会设置GetLastError
        /// </returns>
        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall)]
        public static extern uint WaitForMultipleObjects(int nCount, IntPtr lpHandles, [MarshalAs(UnmanagedType.Bool)] bool bWaitAll, uint dwMilliseconds);

        /// <summary>
        /// 等待信号量
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="dwMilliseconds"></param>
        /// <returns></returns>
        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall)]
        public static extern uint WaitForSingleObject(IntPtr evt, uint dwMilliseconds);

        /// <summary>
        /// 重置信号量为无信号状态
        /// </summary>
        /// <param name="hEvent"></param>
        /// <returns></returns>
        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ResetEvent(IntPtr hEvent);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetEvent(IntPtr hEvent);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);

        #endregion
    }
}
