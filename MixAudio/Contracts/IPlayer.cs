using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MixAudio
{
    public interface IPlayer
    {
        PlaybackState State { get; }
        void Play();
        void Pause();
        void Stop();
        void SeekTo(double position);
        double Position { get; }
        double Length { get; }
        string MediaSource { get; set; }

        event Action PlaybackStopped;
    }
}
