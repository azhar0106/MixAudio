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
        event Action StateChanged;
        void Play();
        void Pause();
        void Stop();
        event Action PlaybackStopped;
        double Position { get; set; }
        double Length { get; }
        string CurrentMedia { get; set; }
    }
}
