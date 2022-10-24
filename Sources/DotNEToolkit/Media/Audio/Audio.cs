using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Media
{
    /// <summary>
    /// 规定PCM数据的存储格式
    /// </summary>
    public enum PCMFormats
    {
        /// <summary>
        /// 有符号16位整数，小尾端
        /// DirectSound默认存储方式
        /// </summary>
        S16_LE,

        /// <summary>
        /// 有符号16位整数，大尾端
        /// </summary>
        S16_BE
    }

    /// <summary>
    /// 提供音频数据相关的工具函数
    /// 
    /// 分贝是一个相对量，相对于某一个标准值的倍数取对数，得到的就是贝尔Bell，分贝则是贝尔的十分之一。比如以10为标准，那么1000就是2Bell，也就是20dB。
    /// 计算分贝的公式：10 * log10(x / std)
    /// 
    /// PCM数据存储格式：
    /// 1. 8位单声道：
    /// 2. 8位双声道：
    /// 3. 16位单声道：
    /// 4. 16位双声道：
    /// </summary>
    public static class Audio
    {
        /// <summary>
        /// 计算音频数据的dbFS值
        /// </summary>
        /// <param name="channels">PCM数据</param>
        /// <param name="pcm">通道数</param>
        /// <param name="formats">PCM数据存储方式</param>
        /// <param name="volume">声音音量，百分比单位</param>
        /// <returns></returns>
        public static void CalculateDb(byte[] pcm, int channels, PCMFormats formats, out List<double> volumes)
        {
            volumes = new List<double>();

            int length = pcm.Length;

            if (channels == 2)
            {
                int sumLeft = 0;
                int sumRight = 0;
                int numSample = 0;      // 总共的采样点数量

                for (int i = 0; i < length; i += 4)
                {
                    int left = Math.Abs(BitConverter.ToInt16(pcm, i));            // 左声道数据
                    int right = Math.Abs(BitConverter.ToInt16(pcm, i + 2));       // 右声道数据

                    sumLeft += left;
                    sumRight += right;

                    numSample++;
                }

                // 计算每个声道的振幅平均值
                double avgLeft = sumLeft / numSample;
                double avgRight = sumRight / numSample;

                // 计算每个声道的dbFS
                // https://blog.csdn.net/u010538116/article/details/80762816
                // https://www.zhihu.com/question/22502766
                // db.Add(20 * Math.Log10(avgLeft / 32767));
                // db.Add(20 * Math.Log10(avgRight / 32767));
                double left_dbFS = 20 * Math.Log10(avgLeft / 32767);
                double right_dbFS = 20 * Math.Log10(avgRight / 32767);

                // 此处计算音量百分比
                double leftPercentage = (90.308733622834 - Math.Abs(left_dbFS)) / 90.308733622834;
                double rightPercentage = (90.308733622834 - Math.Abs(right_dbFS)) / 90.308733622834;

                volumes.Add(Numberics.FixedDecimal(leftPercentage * 100, 2));
                volumes.Add(Numberics.FixedDecimal(rightPercentage * 100, 2));

                //Console.WriteLine("{0}, {1}", leftPercentage, rightPercentage);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
