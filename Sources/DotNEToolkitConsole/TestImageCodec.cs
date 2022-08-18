using DotNEToolkit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkitConsole
{
    public class TestImageCodec
    {
        public static void BMPEncode()
        {
            byte[] buffer = new byte[9999999];
            byte[] fileBytes = File.ReadAllBytes("image");
            int fileSize = ImageCodec.BMPCodec.Encode(buffer, fileBytes, 1920, 1200, ImageCodec.PixelFormats.Gray8);
            using (FileStream fs = new FileStream("test.bmp", FileMode.Create, FileAccess.ReadWrite))
            {
                fs.Write(buffer, 0, fileSize);
            }
        }

        public static void TestSpeed()
        {
            byte[] fileBytes = File.ReadAllBytes("image");
            byte[] buffer = new byte[9999999];

            DateTime start = DateTime.Now;
            for (int i = 0; i < 10000; i++)
            {
                int fileSize = ImageCodec.BMPCodec.Encode(buffer, fileBytes, 1920, 1200, ImageCodec.PixelFormats.Gray8);
            }

            Console.WriteLine("耗时:{0}ms", (DateTime.Now - start).TotalMilliseconds);
        }
    }
}
