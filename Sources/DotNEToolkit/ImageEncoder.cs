using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit
{
    /// <summary>
    /// 一个图像编/解码器
    /// </summary>
    public abstract class ImageCodec
    {
        /// <summary>
        /// BMP文件格式编解码器
        /// </summary>
        public static readonly BMPImageCodec BMPCodec = new BMPImageCodec();

        public enum Codecs
        {
            /// <summary>
            /// BMP格式
            /// </summary>
            BMP,

            /// <summary>
            /// PNG压缩格式
            /// </summary>
            PNG,

            /// <summary>
            /// JPEG压缩格式
            /// </summary>
            JPEG
        }

        /// <summary>
        /// 定义图片的像素格式
        /// </summary>
        public enum PixelFormats
        {
            /// <summary>
            /// 单色，8位索引图
            /// 图像数据指向调色板的索引序号
            /// </summary>
            Gray8
        }

        /// <summary>
        /// 编码器所支持的格式
        /// </summary>
        public abstract Codecs Type { get; }

        /// <summary>
        /// 对一段图像原始数据进行编码
        /// </summary>
        /// <param name="buffer">
        /// 要编码到的内存缓冲区
        /// 注意：外部要开批足够的缓冲区，Encode内部不检查缓冲区的长度
        /// </param>
        /// <param name="rawData">图像原始数据</param>
        /// <param name="width">图像宽度</param>
        /// <param name="height">图像高度</param>
        /// <param name="rawFormat">原始数据的像素格式</param>
        /// <returns>返回大于0表示编码后的图片文件的大小，小于0表示错误码</returns>
        public abstract int Encode(byte[] buffer, byte[] rawData, int width, int height, PixelFormats rawFormat);
    }

    public class BMPImageCodec : ImageCodec
    {
        #region 类变量

        /// <summary>
        /// 8位索引图使用的调色板信息
        /// </summary>
        private static byte[] Gray8ColorPlate = new byte[256 * 4];

        #endregion

        #region 静态构造方法

        static BMPImageCodec()
        {
            for (int i = 0; i <= 255; i++)
            {
                Gray8ColorPlate[i * 4 + 0] = (byte)i;
                Gray8ColorPlate[i * 4 + 1] = (byte)i;
                Gray8ColorPlate[i * 4 + 2] = (byte)i;
                Gray8ColorPlate[i * 4 + 3] = 0;
            }
        }

        #endregion

        public override Codecs Type => Codecs.BMP;

        /// <summary>
        /// 获取某种格式里每个像素的位数
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        private short GetBitsPerPixel(PixelFormats format)
        {
            switch (format)
            {
                case PixelFormats.Gray8: return 8;
                default:
                    throw new NotImplementedException(string.Format("GetBitsPerPixel Not impl, {0}", format));
            }
        }

        /// <summary>
        /// 获取某个格式使用的调色板信息
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        private byte[] GetColorPlate(PixelFormats format)
        {
            switch (format)
            {
                case PixelFormats.Gray8: return Gray8ColorPlate;
                default:
                    throw new NotImplementedException(string.Format("GetColorPlate Not impl, {0}", format));
            }
        }

        public override int Encode(byte[] buffer, byte[] rawData, int width, int height, PixelFormats rawFormat)
        {
            #region 文件头

            // 2字节：BMP魔数
            buffer[0x00] = (byte)'B';
            buffer[0x01] = (byte)'M';
            // 4字节：用字节表示的整个文件大小，到最后再写
            // 4字节：Reserved
            buffer[0x06] = 0;
            buffer[0x07] = 0;
            buffer[0x08] = 0;
            buffer[0x09] = 0;

            // 4字节：从文件头开始到实际的图像数据之间的字节偏移量

            #endregion

            #region 位图信息头

            // 4字节：BITMAPINFOHEADER结构所需要的字节数
            buffer[0x0E] = 0x28;
            buffer[0x0F] = 0;
            buffer[0x10] = 0;
            buffer[0x11] = 0;

            // 4字节：图像宽度
            byte[] widthBytes = BitConverter.GetBytes(width);
            buffer[0x12] = widthBytes[0];
            buffer[0x13] = widthBytes[1];
            buffer[0x14] = widthBytes[2];
            buffer[0x15] = widthBytes[3];

            // 4字节：图像高度
            byte[] heightBytes = BitConverter.GetBytes(height);
            buffer[0x16] = heightBytes[0];
            buffer[0x17] = heightBytes[1];
            buffer[0x18] = heightBytes[2];
            buffer[0x19] = heightBytes[3];

            // 2字节：颜色平面数，该值总为1
            buffer[0x1A] = 0x01;
            buffer[0x1B] = 0x00;

            // 2字节：biBitCount, 一个像素是多少位
            byte[] bitsPerPixel = BitConverter.GetBytes(this.GetBitsPerPixel(rawFormat));
            buffer[0x1C] = bitsPerPixel[0];
            buffer[0x1D] = bitsPerPixel[1];

            // 4字节：biCompression, 图像的压缩类型
            buffer[0x1E] = 0x00;
            buffer[0x1F] = 0x00;
            buffer[0x20] = 0x00;
            buffer[0x21] = 0x00;

            // 4字节：biSizeImage, 图像原始数据的长度
            byte[] imageSize = BitConverter.GetBytes(rawData.Length);
            buffer[0x22] = imageSize[0];
            buffer[0x23] = imageSize[1];
            buffer[0x24] = imageSize[2];
            buffer[0x25] = imageSize[3];

            // 4字节：X方向分辨率（像素/米）
            buffer[0x26] = 0x00;
            buffer[0x27] = 0x00;
            buffer[0x28] = 0;
            buffer[0x29] = 0;

            // 4字节：Y方向分辨率（像素/米）
            buffer[0x2A] = 0x00;
            buffer[0x2B] = 0x00;
            buffer[0x2C] = 0;
            buffer[0x2D] = 0;

            // 4字节：说明位图实际使用的彩色表中的颜色索引数
            buffer[0x2E] = 0;
            buffer[0x2F] = 0x00;
            buffer[0x30] = 0;
            buffer[0x31] = 0;

            /**************
             * 4字节：说明对位图有重要影响的颜色索引的数目
             * 在早期的计算机中，显卡相对比较落后，不一定能保证显示所有颜色，所以在调色板中的颜色数据应尽可能将图像中主要的颜色按顺序排列在前面
             * 位图信息头的biClrImportant字段指出了有多少种颜色是重要的
             * 每个调色板的大小为4字节，按蓝、绿、红、Alpha存储一个颜色值。
             *****************/
            buffer[0x32] = 0;
            buffer[0x33] = 0x00;
            buffer[0x34] = 0;
            buffer[0x35] = 0;

            #endregion

            #region 调色板数据

            /*******************************************************************************************************************************************
             * 果图像是单色、16色和256色，则紧跟着调色板后面的是位图数据，每个字节的位图数据指向调色板的索引序号。
             * 如果位图是16位、24位和32位色，则图像文件中不保留调色板，即不存在调色板，图像的颜色直接在位图数据中给出。
             * 16位图像使用2字节保存颜色值，常见有两种格式：5位红5位绿5位蓝和5位红6位绿5位蓝，即555格式和565格式。555格式只使用了15位，最后一位保留，设为0。
             * 24位图像使用3字节保存颜色值，每一个字节代表一种颜色，按红、绿、蓝排列。
             * 32位图像使用4字节保存颜色值，每一个字节代表一种颜色，除了原来的红、绿、蓝，还有Alpha通道，即透明色。
             *******************************************************************************************************************************************/

            // 如果要编码的格式带有颜色板数据，那么写入颜色板数据
            byte[] colorPlate = this.GetColorPlate(rawFormat);
            if (colorPlate != null)
            {
                Buffer.BlockCopy(colorPlate, 0, buffer, 0x36, colorPlate.Length);
            }

            // 最后是原始像素数据
            Buffer.BlockCopy(rawData, 0, buffer, 0x36 + colorPlate.Length, rawData.Length);

            #endregion

            // 最后写文件头里的整个文件大小字段
            int totalSize = 0x36 + colorPlate.Length + rawData.Length;
            byte[] totalSizeBytes = BitConverter.GetBytes(totalSize);
            buffer[0x02] = totalSizeBytes[0];
            buffer[0x03] = totalSizeBytes[1];
            buffer[0x04] = totalSizeBytes[2];
            buffer[0x05] = totalSizeBytes[3];

            // 再写文件头开始到实际的图像数据之间的字节偏移量
            int offset = 0x36 + colorPlate.Length;
            byte[] offsetBytes = BitConverter.GetBytes(offset);
            buffer[0x0A] = offsetBytes[0];
            buffer[0x0B] = offsetBytes[1];
            buffer[0x0C] = offsetBytes[2];
            buffer[0x0D] = offsetBytes[3];

            return totalSize;
        }
    }
}




