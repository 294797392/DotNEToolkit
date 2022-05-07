using DotNEToolkit.DirectSound;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static DotNEToolkit.Win32API;

namespace DotNEToolkit.Media
{
    /// <summary>
    /// DirectSound录音出来的数据是小尾端的数据
    /// 无符号16位数据
    /// 大尾端就是高位在低地址，小尾端就是低位在低地址。
    /// </summary>
    public class DirectSoundRecord : AudioRecord
    {
        internal class DirectSoundAudioDevice : AudioDevice
        {
            /// <summary>
            /// 设备的GUID
            /// </summary>
            public GUID ID { get; internal set; }

            public override string ToString()
            {
                return this.Name;
            }
        }

        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("DirectSoundCapture");

        /// <summary>
        /// 音频缓冲区通知对象的个数
        /// </summary>
        public int NotifyEvents = 2;

        #endregion

        #region 实例变量

        // DirectSound对象指针
        /// <summary>
        /// IDirectSoundCapture8
        /// </summary>
        private IDirectSoundCapture8 dsc8;
        private IntPtr pdsc8;

        /// <summary>
        /// IDirectSoundCaptureBuffer8
        /// </summary>
        private IDirectSoundCaptureBuffer8 dscb8;
        private IntPtr pdscb8;

        // 通知对象句柄
        private IntPtr[] notifyHandle_close;

        // 音频结构
        private WAVEFORMATEX wfx;
        private IntPtr pwfx_free;
        private DSCBUFFERDESC dsbd;

        // 当前是否正在录音
        private bool isRunning = false;

        #endregion

        #region AudioRecord

        protected override int OnInitialize()
        {
            if (!this.CreateIDirectSoundCapture8())
            {
                return DotNETCode.FAILED;
            }

            if (!this.CreateCaptureBuffer())
            {
                return DotNETCode.FAILED;
            }

            if (!this.CreateBufferNotifications())
            {
                return DotNETCode.FAILED;
            }

            logger.InfoFormat("DirectSoundRecord初始化成功");

            return DotNETCode.SUCCESS;
        }

        protected override void OnRelease()
        {
            Marshal.FreeHGlobal(this.pwfx_free);
            Marshal.Release(this.pdscb8);
            Marshal.Release(this.pdsc8);
            //Marshal.ReleaseComObject(this.dscb8);
            //Marshal.ReleaseComObject(this.dsc8);
            Marshal.FinalReleaseComObject(this.dscb8);
            Marshal.FinalReleaseComObject(this.dsc8);

            this.pwfx_free = IntPtr.Zero;

            this.pdscb8 = IntPtr.Zero;
            this.pdsc8 = IntPtr.Zero;

            this.dscb8 = null;
            this.dsc8 = null;

            foreach (var handle in this.notifyHandle_close)
            {
                Win32API.CloseHandle(handle);
            }
            this.notifyHandle_close = null;
        }

        public override int Start()
        {
            base.Start();

            uint error = this.dscb8.Start(Win32API.DSCBSTART_LOOPING);
            if (error != DSERR.DS_OK)
            {
                logger.ErrorFormat("开始录音失败, DSERROR = {0}", error);
                return DotNETCode.FAILED;
            }

            this.isRunning = true;

            Task.Factory.StartNew((state) =>
            {
                while (this.isRunning)
                {
                    // 这里需要实时获取通知对象的指针, 因为这个指针的值每隔一段时间会改变。。。暂时没找到原因
                    IntPtr lpHandles = Marshal.UnsafeAddrOfPinnedArrayElement(this.notifyHandle_close, 0);

                    // DSLibNatives.WaitForSingleObject(this.close_notifyHwnd[0], DSLibNatives.INFINITE);
                    switch (Win32API.WaitForMultipleObjects(NotifyEvents, lpHandles, false, Win32API.INFINITE))
                    {
                        case Win32API.WAIT_OBJECT_0:
                            {
                                byte[] audioData = null;
                                if (this.RecordCapturedData(0, (uint)this.wfx.nAvgBytesPerSec, out audioData) == DSERR.DS_OK)
                                {
                                    this.NotifyDataReceived(audioData);
                                }

                                Win32API.ResetEvent(this.notifyHandle_close[0]);
                            }
                            break;

                        case Win32API.WAIT_OBJECT_0 + 1:
                            {
                                // 录音结束
                                Win32API.ResetEvent(this.notifyHandle_close[1]);

                                this.isRunning = false;
                            }
                            break;

                        case Win32API.WAIT_FAILED:
                            {
                                int win32Error = Marshal.GetLastWin32Error();

                                // 失败, 句柄已经被销毁
                                logger.ErrorFormat("WAIT_FAILED, LastWin32Error = {0}", win32Error);

                                this.isRunning = false;

                                this.Stop();

                                this.NotifyFailed(win32Error);
                            }
                            break;
                    }
                }

            }, SynchronizationContext.Current);

            return DotNETCode.SUCCESS;
        }

        public override void Stop()
        {
            uint error = this.dscb8.Stop();
            if (error != DSERR.DS_OK)
            {
                logger.ErrorFormat("停止录音失败, DSERR = {0}", error);
            }

            base.Stop();
        }

        public override List<AudioDevice> GetAudioDevices()
        {
            List<AudioDevice> devices = new List<AudioDevice>();
            Win32API.DirectSoundCaptureEnumerate(this.EnumerateAudioDeviceCallback, devices);
            return devices;
        }

        #endregion

        #region 实例方法

        private bool EnumerateAudioDeviceCallback(IntPtr lpGuid, string lpcstrDescription, string lpcstrModule, object lpContext)
        {
            if (lpGuid == IntPtr.Zero)
            {

            }
            else
            {
                List<AudioDevice> devices = lpContext as List<AudioDevice>;
                GUID guid = (GUID)Marshal.PtrToStructure(lpGuid, typeof(GUID));
                DirectSoundAudioDevice device = new DirectSoundAudioDevice()
                {
                    ID = guid,
                    Name = lpcstrDescription
                };
                devices.Add(device);
            }
            return true;
        }

        private bool CreateIDirectSoundCapture8()
        {
            uint error = DSERR.DS_OK;

            if ((error = Win32API.DirectSoundCaptureCreate8(IntPtr.Zero, out this.pdsc8, IntPtr.Zero)) != DSERR.DS_OK)
            {
                logger.ErrorFormat("DirectSoundCaptureCreate8失败, DSERROR = {0}", error);
                return false;
            }

            this.dsc8 = Marshal.GetObjectForIUnknown(this.pdsc8) as IDirectSoundCapture8;

            return true;
        }

        private bool CreateCaptureBuffer()
        {
            uint error = DSERR.DS_OK;

            #region 创建默认音频流格式

            this.wfx = new WAVEFORMATEX()
            {
                nChannels = Channel,
                nSamplesPerSec = SamplesPerSec,
                wBitsPerSample = BitsPerSample,
                nBlockAlign = BlockAlign,
                nAvgBytesPerSec = BytesPerSec,
                cbSize = 0,
                wFormatTag = Win32API.WAVE_FORMAT_PCM
            };

            this.pwfx_free = Marshals.CreateStructurePointer(this.wfx);

            this.dsbd = new DSCBUFFERDESC()
            {
                dwFlags = 0,
                dwSize = Marshal.SizeOf(typeof(DSCBUFFERDESC)),
                dwReserved = 0,
                dwFXCount = 0,
                dwBufferBytes = BufferSize,
                lpwfxFormat = this.pwfx_free,
                lpDSCFXDesc = IntPtr.Zero
            };

            #endregion

            IntPtr pdscb;
            Guid iid_dscb8;
            if ((error = this.dsc8.CreateCaptureBuffer(ref this.dsbd, out pdscb, IntPtr.Zero)) == DSERR.DS_OK)
            {
                // 获取IDirectSoundCaptureBuffer8接口实例
                iid_dscb8 = new Guid(InterfaceID.IID_IDirectSoundCaptureBuffer8);
                Marshal.QueryInterface(pdscb, ref iid_dscb8, out this.pdscb8);
                Marshal.Release(pdscb);
                this.dscb8 = Marshal.GetObjectForIUnknown(this.pdscb8) as IDirectSoundCaptureBuffer8;
            }
            else
            {
                logger.ErrorFormat("CreateCaptureBuffer失败, DSERROR = {0}", error);
                return false;
            }

            return true;
        }

        private bool CreateBufferNotifications()
        {
            uint error = DSERR.DS_OK;

            // 获取IDirectSoundNotify8接口
            Guid iid_dsNotify8 = new Guid(InterfaceID.IID_IDirectSoundNotify8);
            IntPtr pdsNotify8;
            IDirectSoundNotify8 dsNotify8;
            Marshal.QueryInterface(this.pdscb8, ref iid_dsNotify8, out pdsNotify8);
            dsNotify8 = Marshal.GetObjectForIUnknown(pdsNotify8) as IDirectSoundNotify8;

            try
            {
                WAVEFORMATEX wfx;
                int pdwSizeWritten;
                if ((error = this.dscb8.GetFormat(out wfx, Marshal.SizeOf(typeof(WAVEFORMATEX)), out pdwSizeWritten)) != DSERR.DS_OK)
                {
                    logger.ErrorFormat("GetFormat失败, DSERROR = {0}", error);
                    return false;
                }

                DSBPOSITIONNOTIFY[] rgdsbpn = new DSBPOSITIONNOTIFY[this.NotifyEvents];
                this.notifyHandle_close = new IntPtr[this.NotifyEvents];
                for (int i = 0; i < this.NotifyEvents; i++)
                {
                    this.notifyHandle_close[i] = Win32API.CreateEvent(IntPtr.Zero, true, false, null);
                }

                rgdsbpn[0].dwOffset = (uint)(wfx.nAvgBytesPerSec - 1);
                rgdsbpn[0].hEventNotify = this.notifyHandle_close[0];

                rgdsbpn[1].dwOffset = Win32API.DSBPN_OFFSETSTOP;
                rgdsbpn[1].hEventNotify = this.notifyHandle_close[1];

                if ((error = dsNotify8.SetNotificationPositions(this.NotifyEvents, Marshal.UnsafeAddrOfPinnedArrayElement(rgdsbpn, 0))) != DSERR.DS_OK)
                {
                    logger.ErrorFormat("SetNotificationPositions失败, DSERROR = {0}", error);
                    return false;
                }
            }
            finally
            {
                Marshal.Release(pdsNotify8);
            }

            return true;
        }

        private int RecordCapturedData(uint offset, uint dataSize, out byte[] audioData)
        {
            audioData = null;
            IntPtr pbCaptureData;
            int dwCaptureLength;
            IntPtr pbCaptureData2;
            int dwCaptureLength2;
            uint error = DSERR.DS_OK;

            if ((error = this.dscb8.Lock(offset, dataSize, out pbCaptureData, out dwCaptureLength, out pbCaptureData2, out dwCaptureLength2, 0)) != DSERR.DS_OK)
            {
                logger.ErrorFormat("Lock失败, DSERROR = {0}", error);
                return DotNETCode.FAILED;
            }

            // Unlock the capture buffer.
            this.dscb8.Unlock(pbCaptureData, dwCaptureLength, pbCaptureData2, dwCaptureLength2);

            // 拷贝音频数据
            int audioLength = dwCaptureLength + dwCaptureLength2;
            audioData = new byte[audioLength];
            Marshal.Copy(pbCaptureData, audioData, 0, dwCaptureLength);
            if (pbCaptureData2 != IntPtr.Zero)
            {
                Marshal.Copy(pbCaptureData2, audioData, dwCaptureLength, dwCaptureLength2);
            }

            return DotNETCode.SUCCESS;
        }

        #endregion
    }
}
