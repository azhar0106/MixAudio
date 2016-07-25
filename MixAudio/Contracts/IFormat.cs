using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MixAudio
{
    public interface IFormat
    {
        int SampleRate { get; set; }
        int BytesPerSample { get; set; }
        int Channels { get; set; }

    }
}
