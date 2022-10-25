using DotNEToolkit.DirectSound;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace DotNEToolkit.Media
{
    /// <summary>
    /// 使用waveIn API开发的录音程序
    /// </summary>
    public class WaveAPIRecord : AudioRecord
    {
        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("WaveAPIAudioRecord");

        #endregion

        #region 实例变量

        private waveIn.waveInProcDlg waveInProcDlg;

        private IntPtr free_pwfx;
        private IntPtr free_pAudioData;
        /// <summary>
        /// WaveHeader指针
        /// </summary>
        private IntPtr free_pwh;
        private int whSize;

        /// <summary>
        /// WaveIn Hwnd
        /// </summary>
        private IntPtr hwi;

        private bool isRunning = true;

        #endregion

        #region AudioRecord

        protected override int OnInitialize()
        {
            #region waveInOpen

            WAVEFORMATEX wfx = new WAVEFORMATEX()
            {
                nChannels = (short)this.Channel,
                nSamplesPerSec = this.SamplesPerSec,
                wBitsPerSample = (short)this.BitsPerSample,
                nBlockAlign = this.BlockAlign,
                nAvgBytesPerSec = this.BytesPerSec,
                cbSize = 0,
                wFormatTag = 1
            };
            this.free_pwfx = Marshals.CreateStructurePointer(wfx);
            this.waveInProcDlg = new waveIn.waveInProcDlg(this.waveInProc);
            int code = waveIn.waveInOpen(out this.hwi, waveIn.WAVE_MAPPER, this.free_pwfx, this.waveInProcDlg, 0, waveIn.WAVE_FORMAT_DIRECT | waveIn.CALLBACK_FUNCTION);
            if (code != MMSYSERR.MMSYSERR_NOERROR)
            {
                logger.ErrorFormat("waveInOpen失败, MMSYSERROR = {0}", code);
                return DotNETCode.FAILED;
            }

            #endregion

            #region waveInPrepareHeader

            waveIn.wavehdr_tag wh = new waveIn.wavehdr_tag()
            {
                lpData = this.free_pAudioData = Marshal.AllocHGlobal((int)(BlockAlign * SamplesPerSec)),
                dwBufferLength = (uint)(BlockAlign * SamplesPerSec),
                dwFlags = 0x00000002
            };
            this.whSize = Marshal.SizeOf(typeof(waveIn.wavehdr_tag));
            this.free_pwh = Marshals.CreateStructurePointer(wh);
            code = waveIn.waveInPrepareHeader(hwi, this.free_pwh, (uint)this.whSize);
            if (code != MMSYSERR.MMSYSERR_NOERROR)
            {
                logger.ErrorFormat("waveInPrepareHeader失败, MMSYSERROR = {0}", code);
                return DotNETCode.FAILED;
            }

            #endregion

            #region waveInAddBuffer

            if ((code = waveIn.waveInAddBuffer(hwi, this.free_pwh, (uint)this.whSize)) != MMSYSERR.MMSYSERR_NOERROR)
            {
                logger.ErrorFormat("waveInAddBuffer失败, MMSYSERROR = {0}", code);
                return DotNETCode.FAILED;
            }

            #endregion

            return DotNETCode.SUCCESS;
        }

        protected override void OnRelease()
        {
            waveIn.waveInUnprepareHeader(this.hwi, this.free_pwh, (uint)this.whSize);
            waveIn.waveInReset(this.hwi);
            waveIn.waveInClose(this.hwi);

            Marshal.FreeHGlobal(this.free_pAudioData);
            Marshal.FreeHGlobal(this.free_pwfx);
            Marshal.FreeHGlobal(this.free_pwh);
        }

        public override int Start()
        {
            int code = base.Start();

            if ((code = waveIn.waveInStart(this.hwi)) != MMSYSERR.MMSYSERR_NOERROR)
            {
                logger.ErrorFormat("waveInStart失败, MMSYSERROR = {0}", code);
                return DotNETCode.SYS_ERROR;
            }

            this.isRunning = true;

            return DotNETCode.SUCCESS;
        }

        public override void Stop()
        {
            this.isRunning = false;

            int code = waveIn.waveInStop(this.hwi);
            if (code != MMSYSERR.MMSYSERR_NOERROR)
            {
                logger.ErrorFormat("waveInStop失败, MMSYSERROR = {0}", code);
            }

            base.Stop();
        }

        public override List<AudioDevice> GetAudioDevices()
        {
            throw new NotImplementedException();
        }

        #endregion

        private void waveInProc(IntPtr hwi, uint uMsg, uint dwInstance, uint dwParam1, uint dwParam2)
        {
            switch (uMsg)
            {
                case (uint)waveIn.uMsgEnum.WIM_OPEN:
                    logger.InfoFormat("OPEN");
                    break;

                case (uint)waveIn.uMsgEnum.WIM_DATA:
                    if (!this.isRunning)
                    {
                        break;
                    }

                    waveIn.wavehdr_tag hdr = (waveIn.wavehdr_tag)Marshal.PtrToStructure(this.free_pwh, typeof(waveIn.wavehdr_tag));

                    // 处理音频数据
                    {
                        byte[] buffer = new byte[hdr.dwBytesRecorded];
                        Marshal.Copy(hdr.lpData, buffer, 0, buffer.Length);

                        this.NotifyDataReceived(buffer);
                    }

                    int code = waveIn.waveInAddBuffer(hwi, this.free_pwh, (uint)this.whSize);
                    if (code != MMSYSERR.MMSYSERR_NOERROR)
                    {
                        logger.ErrorFormat("waveInAddBuffer失败, MMSYSERROR = {0}", code);
                    }

                    break;

                case (uint)waveIn.uMsgEnum.WIM_CLOSE:
                    logger.InfoFormat("CLOSE");
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
