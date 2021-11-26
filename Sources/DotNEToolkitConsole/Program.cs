using DotNEToolkit.Crypto;
using DotNEToolkit.Expressions;
using DotNEToolkit.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DotNEToolkitConsole
{
    class Program
    {
        private static string ExternalLog4netConfig = "log4net.xml";

        private static void InitializeLog4net()
        {
            try
            {
                FileInfo configFile = new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ExternalLog4netConfig));
                if (configFile.Exists)
                {
                    log4net.Config.XmlConfigurator.ConfigureAndWatch(configFile);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("初始化日志异常, {0}", ex);
            }
        }

        static void Main(string[] args)
        {
            InitializeLog4net();

            ExpressionParser parser = new ExpressionParser();

            DateTime start = DateTime.Now;
            Expression expression = parser.BuildExpressionTree("b('1','2'), c('3'), d(e('1'))");
            double time = (DateTime.Now - start).TotalMilliseconds;
            Console.WriteLine(time);

            Console.ReadLine();

            //byte[] data = CRC.CRC16(new byte[] { 0x01, 0x02, 0x00, 0x00, 0x00, 0x04 });

            //Console.WriteLine(Convert.ToString(data[0], 16));
            //Console.WriteLine(Convert.ToString(data[1], 16));

            //Console.ReadLine();

            //Console.WriteLine(20 * Math.Log10((double)1 / (double)32767));

            //Console.ReadLine();

            //double maxDb = 20 * Math.Log10(32767);
            //Console.WriteLine(maxDb);

            //Console.ReadLine();

            //string data = string.Empty;

            //for (int i = 1; i < 65535; i++)
            //{
            //    double maxDb = 20 * Math.Log10(i);
            //    data += maxDb.ToString() + Environment.NewLine;
            //}

            //File.WriteAllText("1.txt", data);

            Dictionary<string, object> parameters = new Dictionary<string, object>();

            //DirectSoundPlay Player = new DirectSoundPlay();
            //int code = Player.Initialize(parameters);
            //if (code != DotNEToolkit.DotNETCode.SUCCESS)
            //{
            //    Console.WriteLine("初始化DirectSoundPlay失败");
            //}
            //while (true)
            //{
            //    Player.PlayFile("out.pcm");
            //    System.Threading.Thread.Sleep(1000);
            //}

            //Dictionary<string, object> parameters1 = new Dictionary<string, object>();
            ////parameters1["Channel"] = 1;
            ////parameters1["SamplesPerSec"] = 16000;
            //DirectSoundRecord Record = new DirectSoundRecord();
            //Record.Initialize(parameters1);
            //Record.SetRecordFile("out.pcm");
            //Record.Start();

            byte[] alaw = File.ReadAllBytes("audio");
            byte[] pcm = G711.Alaw2PCM(alaw);
            File.WriteAllBytes("pcm", pcm);
            Console.WriteLine("转换成功");

            Console.ReadLine();
        }

        private static void Record_DataReceived(AudioRecord record, byte[] audioData)
        {
            List<double> db;
            Audio.CalculateDb(audioData, 2, PCMFormats.S16_LE, out db);

            Console.WriteLine("左声道 = {0}%, 右声道 = {1}%", db[0], db[1]);
        }
    }
}
