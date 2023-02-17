using DotNEToolkit.Media;
using DotNEToolkit.Media.Audio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DotNEToolkitDemo.UserControls
{
    public partial class TestDirectSound : Form
    {
        private AudioPlay audioPlay;

        public TestDirectSound()
        {
            InitializeComponent();

            this.audioPlay = AudioPlayFactory.Create(AudioPlayType.DirectSound);
            this.audioPlay.SetParameter<int>("timeout", 1000000);
            this.audioPlay.SetParameter<short>("channel", 1);
            this.audioPlay.SetParameter<int>("sampleRate", 8000);
            this.audioPlay.SetParameter<int>("sampleSize", 16);
            this.audioPlay.SetParameter<AVFormats>("format", AVFormats.AV_FORMAT_PCM);
        }

        private void PlayAudioFile()
        {
            byte[] bytes = new byte[999999];

            while (true) 
            {
                this.audioPlay.Write(bytes);
                System.Threading.Thread.Sleep(1000);
            }
        }

        private void buttonInitialize_Click(object sender, EventArgs e)
        {
            this.audioPlay.Initialize();
            Task.Factory.StartNew(this.PlayAudioFile);
        }

        private void buttonPlay_Click(object sender, EventArgs e)
        {
            this.audioPlay.Start();
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            this.audioPlay.Stop();
        }

        private void Release_Click(object sender, EventArgs e)
        {
            this.audioPlay.Initialize();
        }
    }
}
