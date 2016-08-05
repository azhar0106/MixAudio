using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MixAudio
{
    public class Playlist : IPlaylist
    {
        Player m_player;
        ObservableCollection<string> m_mediaList;
        int m_currentMediaIndex;
        bool m_isStopInitiatedByUser;


        public Playlist()
        {
            m_player = new Player();
            m_player.StateChanged += M_player_StateChanged;
            m_player.PlaybackStopped += M_player_PlaybackStopped;

            m_mediaList = new ObservableCollection<string>();
            m_mediaList.CollectionChanged += M_mediaList_CollectionChanged;
            m_currentMediaIndex = -1;
        }


        public PlaybackState State {
            get
            {
                return m_player.State;
            }
        }
        public event Action StateChanged;

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
            InternalStop();
        }
        private void InternalStop()
        {
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
                return m_currentMediaIndex;
            }
            set
            {
                InternalStop();
                m_currentMediaIndex = value;
                Play();
            }
        }

        public void Next()
        {
            if (State == PlaybackState.Playing)
            {
                InternalStop();
                MoveToNextMedia();
                Play();
            }
            else
            {
                InternalStop();
                MoveToNextMedia();
            }
        }

        public void Previous()
        {
            if (State == PlaybackState.Playing)
            {
                InternalStop();
                MoveToPreviousMedia();
                Play();
            }
            else
            {
                InternalStop();
                MoveToPreviousMedia();
            }
        }
        
        public ObservableCollection<string> MediaList
        {
            get
            {
                return m_mediaList;
            }
        }


        private void M_mediaList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (MediaList.Count == 0)
            {
                m_currentMediaIndex = -1;
            }
            else
            {
                if (m_currentMediaIndex == -1)
                {
                    m_currentMediaIndex = 0;
                }
            }
        }

        private void M_player_StateChanged()
        {
            StateChanged?.Invoke();
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
            if (m_currentMediaIndex == m_mediaList.Count - 1)
            {
                m_currentMediaIndex = 0;
            }
            else
            {
                m_currentMediaIndex++;
            }
        }

        private void MoveToPreviousMedia()
        {
            if (m_currentMediaIndex == 0)
            {
                m_currentMediaIndex = m_mediaList.Count - 1;
            }
            else
            {
                m_currentMediaIndex--;
            }
        }
    }
}
