using DotNEToolkit.DirectSound;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotNEToolkit.Media
{
    public class DirectSoundPlay : AudioPlay
    {
        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("DirectSoundPlay");

        private const int NotifyEvents = 3;

        #endregion

        #region 实例变量

        private IntPtr pds8;
        private IDirectSound8 ds8;

        private IntPtr pdsb8;
        private IDirectSoundBuffer8 dsb8;

        private WAVEFORMATEX wfx;
        private IntPtr pwfx_free;

        private DSBUFFERDESC dsbd;

        // 通知句柄
        private DSBPOSITIONNOTIFY[] rgdsbpn;
        private IntPtr[] notifyHwnd_close;

        private Stream fileStream;

        #endregion

        #region AudioPlay

        public override int Initialize(IDictionary parameters)
        {
            base.Initialize(parameters);

            if ((this.CreateIDirectSound8() &&
                this.CreateSecondaryBuffer() &&
                this.CreateBufferNotifications()))
            {
                logger.InfoFormat("DirectSoundPlay初始化成功");
                return DotNETCode.SUCCESS;
            }

            return DotNETCode.FAILED;
        }

        public override void Release()
        {
            Marshal.FreeHGlobal(this.pwfx_free);
            Marshal.Release(this.pdsb8);
            Marshal.Release(this.pds8);
            //Marshal.ReleaseComObject(this.dsb8);
            //Marshal.ReleaseComObject(this.ds8);
            Marshal.FinalReleaseComObject(this.dsb8);
            Marshal.FinalReleaseComObject(this.ds8);

            this.pwfx_free = IntPtr.Zero;

            this.pdsb8 = IntPtr.Zero;
            this.pds8 = IntPtr.Zero;

            this.dsb8 = null;
            this.ds8 = null;

            foreach (var item in notifyHwnd_close)
            {
                Win32API.CloseHandle(item);
            }
            this.notifyHwnd_close = null;
            this.rgdsbpn = null;

            base.Release();
        }

        public override int PlayFile(string fileURI)
        {
            if (!File.Exists(fileURI))
            {
                logger.WarnFormat("文件不存在, {0}", fileURI);
                return DotNETCode.FILE_NOT_FOUND;
            }

            try
            {
                this.fileStream = File.Open(fileURI, FileMode.Open);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("打开文件失败", ex);
                return DotNETCode.OPEN_FILE_FAILED;
            }

            this.dsb8.SetCurrentPosition(0);
            uint error = this.dsb8.Play(0, 0, DSBPLAY.DSBPLAY_LOOPING);
            if (error != DSERR.DS_OK)
            {
                logger.ErrorFormat("Play失败, DSERR = {0}", error);
                return DotNETCode.SYS_ERROR;
            }

            uint offset = (uint)this.BufferSize;

            while (true)
            {
                byte[] buffer = new byte[this.BufferSize];
                if (this.fileStream.Read(buffer, 0, buffer.Length) == 0)
                {
                    this.CloseFile();
                    this.dsb8.Stop();
                    logger.InfoFormat("文件播放完毕, 退出");
                    return DotNETCode.SUCCESS;
                }

                IntPtr lpHandles = Marshal.UnsafeAddrOfPinnedArrayElement(this.notifyHwnd_close, 0);

                uint notifyIdx = Win32API.WaitForMultipleObjects(NotifyEvents, lpHandles, false, Win32API.INFINITE);
                if ((notifyIdx >= Win32API.WAIT_OBJECT_0) && (notifyIdx <= Win32API.WAIT_OBJECT_0 + NotifyEvents))
                {
                    if (this.WriteDataToBuffer(offset, buffer))
                    {
                    }

                    offset += (uint)this.BufferSize;
                    offset %= (uint)(this.BufferSize * NotifyEvents);

                    //Console.WriteLine("dwOffset = {0}, offset = {1}", this.rgdsbpn[notifyIdx].dwOffset, offset);
                }
                else if (notifyIdx == Win32API.WAIT_FAILED)
                {
                    int winErr = Marshal.GetLastWin32Error();

                    logger.ErrorFormat("等待信号失败, 退出播放, LastWin32Error = {0}", winErr);

                    this.dsb8.Stop();
                    this.CloseFile();

                    return DotNETCode.SYS_ERROR;
                }
            }
        }

        #endregion

        #region 实例方法

        private bool CreateIDirectSound8()
        {
            uint error = Win32API.DirectSoundCreate8(IntPtr.Zero, out this.pds8, IntPtr.Zero);
            if (error != DSERR.DS_OK)
            {
                logger.ErrorFormat("DirectSoundCreate8失败, DSERR = {0}", error);
                return false;
            }

            this.ds8 = Marshal.GetObjectForIUnknown(this.pds8) as IDirectSound8;

            if ((error = this.ds8.SetCooperativeLevel(Win32API.GetDesktopWindow(), DSSCL.DSSCL_NORMAL)) != DSERR.DS_OK)
            {
                logger.ErrorFormat("SetCooperativeLevel失败, DSERR = {0}", error);
                return false;
            }

            return true;
        }

        private bool CreateSecondaryBuffer()
        {
            uint error = DSERR.DS_OK;

            #region 创建默认音频流格式

            this.wfx = new WAVEFORMATEX()
            {
                nChannels = this.Channel,
                nSamplesPerSec = this.SamplesPerSec,
                wBitsPerSample = this.BitsPerSample,
                nBlockAlign = this.BlockAlign,
                nAvgBytesPerSec = this.BytesPerSec,
                cbSize = 0,
                wFormatTag = Win32API.WAVE_FORMAT_PCM
            };

            this.pwfx_free = Marshals.StructureToPtr(this.wfx);

            this.dsbd = new DSBUFFERDESC()
            {
                dwSize = Marshal.SizeOf(typeof(DSBUFFERDESC)),
                dwFlags = DSBCAPS.DSBCAPS_CTRLPOSITIONNOTIFY | DSBCAPS.DSBCAPS_GETCURRENTPOSITION2 | DSBCAPS.DSBCAPS_GLOBALFOCUS | DSBCAPS.DSBCAPS_CTRLVOLUME,
                lpwfxFormat = this.pwfx_free,
                guid3DAlgorithm = new Win32API.GUID(),
                dwBufferBytes = this.BufferSize * NotifyEvents,
                dwReserved = 0
            };

            #endregion

            IntPtr pdsb;
            if ((error = this.ds8.CreateSoundBuffer(ref this.dsbd, out pdsb, IntPtr.Zero)) != DSERR.DS_OK)
            {
                logger.ErrorFormat("CreateSoundBuffer失败, DSERR = {0}", error);
                return false;
            }

            Guid iid_dsb8 = new Guid(InterfaceID.IID_IDirectSoundBuffer8);
            Marshal.QueryInterface(pdsb, ref iid_dsb8, out this.pdsb8);
            Marshal.Release(pdsb);
            this.dsb8 = Marshal.GetObjectForIUnknown(this.pdsb8) as IDirectSoundBuffer8;

            return true;
        }

        private bool CreateBufferNotifications()
        {
            uint error = DSERR.DS_OK;

            // 获取IDirectSoundNotify8接口
            Guid iid_dsNotify8 = new Guid(InterfaceID.IID_IDirectSoundNotify8);
            IntPtr pdsNotify8;
            IDirectSoundNotify8 dsNotify8;
            Marshal.QueryInterface(this.pdsb8, ref iid_dsNotify8, out pdsNotify8);
            dsNotify8 = Marshal.GetObjectForIUnknown(pdsNotify8) as IDirectSoundNotify8;

            try
            {
                uint written;
                WAVEFORMATEX wfx;
                if ((error = this.dsb8.GetFormat(out wfx, (uint)Marshal.SizeOf(typeof(WAVEFORMATEX)), out written)) != DSERR.DS_OK)
                {
                    logger.ErrorFormat("GetFormat失败, DSERR = {0}", error);
                    return false;
                }

                this.rgdsbpn = new DSBPOSITIONNOTIFY[NotifyEvents];
                this.notifyHwnd_close = new IntPtr[NotifyEvents];
                for (int idx = 0; idx < NotifyEvents; idx++)
                {
                    IntPtr pHandle = Win32API.CreateEvent(IntPtr.Zero, false, false, null);
                    this.notifyHwnd_close[idx] = pHandle;
                    this.rgdsbpn[idx].dwOffset = (uint)(this.BufferSize * idx);
                    this.rgdsbpn[idx].hEventNotify = pHandle;
                }

                if ((error = dsNotify8.SetNotificationPositions(NotifyEvents, Marshal.UnsafeAddrOfPinnedArrayElement(rgdsbpn, 0))) != DSERR.DS_OK)
                {
                    logger.WarnFormat("SetNotificationPositions失败, DSERROR = {0}", error);
                    return false;
                }
            }
            finally
            {
                Marshal.Release(pdsNotify8);
            }

            return true;
        }

        private bool WriteDataToBuffer(uint offset, byte[] data)
        {
            IntPtr audioPtr1, audioPtr2;
            uint audioBytes1, audioBytes2, dataLength = (uint)data.Length;

            uint error = this.dsb8.Lock(offset, dataLength, out audioPtr1, out audioBytes1, out audioPtr2, out audioBytes2, 0);
            if (error == DSERR.DSERR_BUFFERLOST)
            {
                this.dsb8.Restore();
                if ((error = this.dsb8.Lock(offset, dataLength, out audioPtr1, out audioBytes1, out audioPtr2, out audioBytes2, 0)) != DSERR.DS_OK)
                {
                    logger.ErrorFormat("Lock失败, DSERR = {0}", error);
                    return false;
                }
            }

            if (data != null && data.Length > 0)
            {
                Marshal.Copy(data, 0, audioPtr1, (int)audioBytes1);
                if (audioBytes2 > 0 && audioPtr2 != IntPtr.Zero)
                {
                    Marshal.Copy(data, (int)audioBytes1, audioPtr2, (int)audioBytes2);
                }
            }
            else
            {
                // 填充空数据
                //DSLibNatives.memset(audioPtr1, 0, audioBytes1);
                //if (audioPtr2 != IntPtr.Zero)
                //{
                //    DSLibNatives.memset(audioPtr2, 0, audioBytes2);
                //}
            }

            error = this.dsb8.Unlock(audioPtr1, audioBytes1, audioPtr2, audioBytes2);
            if (error != DSERR.DS_OK)
            {
                logger.ErrorFormat("Unlock失败, DSERR = {0}", error);
                return false;
            }

            return true;
        }

        private void CloseFile()
        {
            if (this.fileStream != null)
            {
                try
                {
                    this.fileStream.Close();
                    this.fileStream.Dispose();
                }
                catch (Exception ex)
                {
                    logger.Error("关闭文件异常", ex);
                }
            }
        }

        private byte[] ReadFile(int size)
        {
            throw new NotImplementedException();
            //int remain = size;
            //int read = 0;
            //byte[] buffer = new byte[size];

            //while (remain > 0)
            //{
            //    this.fileStream.Read()
            //}
        }

        #endregion
    }
}
