using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.Utility
{
    public static class ImageUtils
    {
        /// <summary>
        /// RGB24图像左右翻转
        /// </summary>
        /// <param name="rgbBytes"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public static void FlipY(byte[] rgbBytes, int width, int height)
        {
            int halfwidth = width / 2;

            for (int r = 0; r < height; r++)
            {
                for (int c = 0; c < halfwidth; c++)
                {
                    int indexR = 0 + r * width * 3 + c * 3;
                    int indexG = indexR + 1;
                    int indexB = indexR + 2;

                    int indexR3 = 0 + (r + 1) * width * 3 - (c + 1) * 3;
                    int indexG3 = indexR3 + 1;
                    int indexB3 = indexR3 + 2;

                    byte valueR = rgbBytes[indexR];
                    byte valueG = rgbBytes[indexG];
                    byte valueB = rgbBytes[indexB];

                    rgbBytes[indexR] = rgbBytes[indexR3];
                    rgbBytes[indexG] = rgbBytes[indexG3];
                    rgbBytes[indexB] = rgbBytes[indexB3];

                    rgbBytes[indexR3] = valueR;
                    rgbBytes[indexG3] = valueG;
                    rgbBytes[indexB3] = valueB;
                }
            }
        }

        /// <summary>
        /// RGB24图像上下翻转
        /// </summary>
        /// <param name="rgbBytes"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public static void FlipX(byte[] rgbBytes, int width, int height)
        {
            int halfheight = height / 2;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < halfheight; j++)
                {
                    int indexR = 0 + j * width * 3 + i * 3;
                    int indexG = indexR + 1;
                    int indexB = indexR + 2;

                    int indexR2 = 0 + (height - 1 - j) * width * 3 + i * 3;
                    int indexG2 = indexR2 + 1;
                    int indexB2 = indexR2 + 2;

                    byte valueR = rgbBytes[indexR];
                    byte valueG = rgbBytes[indexG];
                    byte valueB = rgbBytes[indexB];

                    rgbBytes[indexR] = rgbBytes[indexR2];
                    rgbBytes[indexG] = rgbBytes[indexG2];
                    rgbBytes[indexB] = rgbBytes[indexB2];

                    rgbBytes[indexR2] = valueR;
                    rgbBytes[indexG2] = valueG;
                    rgbBytes[indexB2] = valueB;
                }
            }
        }
    }
}
