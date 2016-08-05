using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MixAudio
{
    public interface IPlaylist : IPlayer
    {
        int CurrentMediaIndex { get; set; }
        void Next();
        void Previous();
        ObservableCollection<string> MediaList { get; }
    }
}
