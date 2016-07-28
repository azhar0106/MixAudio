using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MixAudio
{
    public class Playlist : IPlaylist
    {
        Player m_player;
        List<string> m_mediaList;
        int m_currentMedia;
        bool m_isStopInitiatedByUser;


        public Playlist()
        {
            m_player = new Player();
            m_player.PlaybackStopped += M_player_PlaybackStopped;

            m_mediaList = new List<string>();
            m_currentMedia = -1;
        }


        public PlaybackState State {
            get
            {
                return m_player.State;
            }
        }

        public void Play()
        {
            m_player.CurrentMedia = CurrentMedia;
            m_player.Play();
        }

        public void Pause()
        {
            m_player.Pause();
        }

        public void Stop()
        {
            m_isStopInitiatedByUser = true;
            m_player.Stop();
        }

        public event Action PlaybackStopped;

        public double Position
        {
            get
            {
                return m_player.Position;
            }
            set
            {
                m_player.Position = value;
            }
        }
        
        public double Length
        {
            get
            {
                return m_player.Length;
            }
        }
        
        public string CurrentMedia
        {
            get
            {
                return m_mediaList[CurrentMediaIndex];
            }
            set
            {
                throw new NotSupportedException();
            }
        }
        
        public int CurrentMediaIndex
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
            if (State == PlaybackState.Playing)
            {
                Stop();
                MoveToNextMedia();
                Play();
            }
            else
            {
                Stop();
                MoveToNextMedia();
            }
        }

        public void Previous()
        {
            if (State == PlaybackState.Playing)
            {
                Stop();
                MoveToPreviousMedia();
                Play();
            }
            else
            {
                Stop();
                MoveToPreviousMedia();
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

            if (m_mediaList.Count == 1)
            {
                m_currentMedia = 0;
            }
        }

        public void RemoveMedia(int index)
        {
            m_mediaList.RemoveAt(index);

            if (m_mediaList.Count == 0)
            {
                m_currentMedia = -1;
            }
        }

        public string GetMedia(int index)
        {
            return m_mediaList[index];
        }


        private void M_player_PlaybackStopped()
        {
            PlaybackStopped?.Invoke();
            if (m_isStopInitiatedByUser)
            {
                m_isStopInitiatedByUser = false;
                return;
            }
            Next();
            Play();
        }


        private void MoveToNextMedia()
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

        private void MoveToPreviousMedia()
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
    }
}
