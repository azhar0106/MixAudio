using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MixAudio
{
    public interface ISample
    {
        int Channels { get; set; }
        double[] Block { get; set; }
        int SampleRate { get; set; }
        void SetSampleParams(IFormat format);
        bool WriteBlockFromBuffer(byte[] buffer, int offset, IFormat format);
        bool WriteBlockToBuffer(byte[] buffer, int offset, IFormat format);
    }
}
