using DotNEToolkit.Modular;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.Media.Video
{
    public abstract class VideoDecode : ModuleBase
    {
        public abstract int Decode(byte[] videoData, out byte[] decodeData);
    }

    public class DefaultVideoDecode : VideoDecode
    {
        public struct VideoDecodeOptions
        {
            public int CodecType;
            public int VideoWidth;
            public int VideoHeight;
        }

        [DllImport("video.dll")]
        public static extern IntPtr VideoDecodeCreate(IntPtr options);

        [DllImport("video.dll")]
        public static extern int VideoDecodeInitialize(IntPtr decode);

        [DllImport("video.dll")]
        public static extern void VideoDecodeRelease(IntPtr decode);

        [DllImport("video.dll")]
        public static extern int VideoDecodeDecode(IntPtr decode, byte[] videoData, int dataSize, out byte[] decodeData, out int size);

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
        }

        public override int Decode(byte[] videoData, out byte[] decodeData)
        {
            int decodeSize;
            return VideoDecodeDecode(this.decodePtr, videoData, videoData.Length, out decodeData, out decodeSize);
        }

        #endregion
    }
}
