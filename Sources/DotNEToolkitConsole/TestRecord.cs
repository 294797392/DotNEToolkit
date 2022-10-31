using DotNEToolkit.Media.Audio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkitConsole
{
    public static class TestRecord
    {
        public static void RecordAudio()
        {
            AudioRecord record = AudioRecordFactory.Create(AudioRecordType.WaveAPI);
            record.SetRecordFile("pcm");
            record.Initialize();
            record.Start();
        }
    }
}
