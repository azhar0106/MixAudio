using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MixAudio
{
    public class Format : IFormat
    {
        public int SampleRate { get; set; }
        public int BytesPerSample { get; set; }
        public int Channels { get; set; }

        public Format(int sampleRate, int bytesPerSample, int channels)
        {
            SampleRate = sampleRate;
            BytesPerSample = bytesPerSample;
            Channels = channels;
        }

    }
}
