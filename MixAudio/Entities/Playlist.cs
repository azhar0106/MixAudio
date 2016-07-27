using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MixAudio.Entities
{
    public class Playlist : IPlaylist
    {
        Player m_player;
        List<string> m_mediaList;
        int m_currentMedia;


        public Playlist()
        {
            m_player = new Player();
            m_player.PlaybackStopped += M_player_PlaybackStopped;

            m_mediaList = new List<string>();
        }


        public PlaybackState State {
            get
            {
                return m_player.State;
            }
        }

        public void Play()
        {
            m_player.MediaSource = m_mediaList[CurrentMedia];
            m_player.Play();
        }

        public void Pause()
        {
            m_player.Pause();
        }

        public void Stop()
        {
            m_player.Stop();
        }

        public event Action PlaybackStopped;

        public int CurrentMedia
        {
            get
            {
                return m_currentMedia;
            }
            set
            {
                Stop();
                m_currentMedia = value;
                Play();
            }
        }

        public void Next()
        {
            if (m_currentMedia == m_mediaList.Count - 1)
            {
                m_currentMedia = 0;
            }
            else
            {
                m_currentMedia++;
            }
        }

        public void Previous()
        {
            if (m_currentMedia == 0)
            {
                m_currentMedia = m_mediaList.Count - 1;
            }
            else
            {
                m_currentMedia--;
            }
        }

        public int MediaCount
        {
            get
            {
                return m_mediaList.Count;
            }
        }

        public void AddMedia(int index, string mediaLocation)
        {
            m_mediaList.Insert(index, mediaLocation);
        }

        public void RemoveMedia(int index)
        {
            m_mediaList.RemoveAt(index);
        }

        public string GetMedia(int index)
        {
            return m_mediaList[index];
        }


        private void M_player_PlaybackStopped()
        {
            PlaybackStopped?.Invoke();
        }
    }
}
