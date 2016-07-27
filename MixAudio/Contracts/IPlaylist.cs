using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MixAudio
{
    public interface IPlaylist
    {
        PlaybackState State { get; }
        void Play();
        void Pause();
        void Stop();
        event Action PlaybackStopped;
        int CurrentMedia { get; set; }
        void Next();
        void Previous();
        void AddMedia(int index, string mediaLocation);
        void RemoveMedia(int index);
        string GetMedia(int index);
        int MediaCount { get; }
    }
}
