using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MixAudio
{
    public interface IOutputFilter : IFilter
    {
        IFormat OutFormat { get; }
        double Position { get; }
        void Play();
        void Stop();
        long Latency { get; }

        event Func<bool> DataRequired;

        event Action PlaybackStopped;
    }
}
