using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.Media.Video
{
    public class libvideoVideoDecode : VideoDecode
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct VideoDecodeOptions
        {
            public int AVFormat;
            public int VideoWidth;
            public int VideoHeight;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VideoDecodeInput
        {
            public IntPtr VideoData;
            public int VideoDataSize;
            public IntPtr DecodeData;
            public int DecodeDataSize;
            public int VideoWidth;
            public int VideoHeight;
        }

        [DllImport("video.dll")]
        public static extern IntPtr VideoDecodeCreate(IntPtr options);

        [DllImport("video.dll")]
        public static extern void VideoDecodeDelete(IntPtr decode);

        [DllImport("video.dll")]
        public static extern int VideoDecodeInitialize(IntPtr decode);

        [DllImport("video.dll")]
        public static extern void VideoDecodeRelease(IntPtr decode);

        [DllImport("video.dll")]
        public static extern int VideoDecodeDecode(IntPtr decode, ref VideoDecodeInput decodeInput);

        #region 实例变量

        private VideoDecodeOptions decodeOptions;
        private IntPtr decodeOptionsPtr;
        private IntPtr decodePtr;

        #endregion

        #region 公开接口

        protected override int OnInitialize()
        {
            this.decodeOptions = new VideoDecodeOptions()
            {
                AVFormat = (int)AVFormats.AV_FORMAT_H264,
                VideoWidth = 1920,
                VideoHeight = 1080
            };

            this.decodeOptionsPtr = MarshalUtils.CreateStructurePointer(this.decodeOptions);
            this.decodePtr = VideoDecodeCreate(this.decodeOptionsPtr);
            VideoDecodeInitialize(this.decodePtr);

            return DotNETCode.SUCCESS;
        }

        protected override void OnRelease()
        {
            VideoDecodeRelease(this.decodePtr);
            MarshalUtils.FreeStructurePointer(this.decodeOptionsPtr);
            VideoDecodeDelete(this.decodePtr);
            this.decodePtr = IntPtr.Zero;
        }

        public override int Decode(byte[] videoData, out byte[] decodeData, out int videoWidth, out int videoHeight)
        {
            decodeData = null;
            videoWidth = 0;
            videoHeight = 0;

            VideoDecodeInput decodeInput = new VideoDecodeInput()
            {
                VideoData = Marshal.UnsafeAddrOfPinnedArrayElement(videoData, 0),
                VideoDataSize = videoData.Length
            };
            int code = VideoDecodeDecode(this.decodePtr, ref decodeInput);
            if (code != DotNETCode.SUCCESS)
            {
                return code;
            }

            decodeData = new byte[decodeInput.DecodeDataSize];
            Marshal.Copy(decodeInput.DecodeData, decodeData, 0, decodeData.Length);
            videoWidth = decodeInput.VideoWidth;
            videoHeight = decodeInput.VideoHeight;

            return code;
        }

        public override int Decode(byte[] videoData, out IntPtr decodeData, out int decodeDataSize, out int videoWidth, out int videoHeight)
        {
            decodeData = IntPtr.Zero;
            decodeDataSize = 0;
            videoWidth = 0;
            videoHeight = 0;

            VideoDecodeInput decodeInput = new VideoDecodeInput()
            {
                VideoData = Marshal.UnsafeAddrOfPinnedArrayElement(videoData, 0),
                VideoDataSize = videoData.Length
            };

            int code = VideoDecodeDecode(this.decodePtr, ref decodeInput);
            if (code != DotNETCode.SUCCESS)
            {
                return code;
            }

            decodeData = decodeInput.DecodeData;
            decodeDataSize = decodeInput.DecodeDataSize;
            videoWidth = decodeInput.VideoWidth;
            videoHeight = decodeInput.VideoHeight;

            return DotNETCode.SUCCESS;
        }

        #endregion
    }
}
