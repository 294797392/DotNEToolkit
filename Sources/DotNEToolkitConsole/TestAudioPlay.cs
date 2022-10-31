using DotNEToolkit.Media;
using DotNEToolkit.Media.Video;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkitConsole
{
    public static class TestAudioPlay
    {
        public static void libvlcPlay()
        {
            List<string> media_options = new List<string>() 
            {
                "rawaud-channels=2",
                "rawaud-samplerate=44100"
            };

            VideoPlay videoPlay = VideoPlayFactory.Create(VideoPlayType.libvlc);
            videoPlay.SetInputValue<AVFormats>("format", AVFormats.PCM);
            videoPlay.SetInputObject("media_options", media_options);
            videoPlay.Initialize();
            videoPlay.Start();

            Task.Factory.StartNew(() =>
            {
                FileStream pcmStream = new FileStream("pcm", FileMode.Open, FileAccess.Read);

                while (true)
                {
                    byte[] buffer = new byte[8192];
                    int len = pcmStream.Read(buffer, 0, buffer.Length);
                    if (len == 0)
                    {
                        break;
                    }
                    videoPlay.Write(buffer);
                    System.Threading.Thread.Sleep(50);
                }
            });
        }
    }
}
