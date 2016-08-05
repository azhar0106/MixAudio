using MixAudioPlayer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace MixAudio
{
    public class PlayerViewModel : ViewModelBase
    {
        IPlaylist m_playlist;
        DispatcherTimer m_timer;
        bool m_seekStarted;

        
        public string CurrentMedia
        {
            get { return m_playlist.CurrentMedia; }

            //set
            //{
            //    if (value == m_currentMedia)
            //    {
            //        return;
            //    }
            //    if (!CanChangeFileLocation)
            //    {
            //        return;
            //    }

            //    m_currentMedia = value;
            //    m_playlist.CurrentMedia = value;
            //    RaiseCommandCanExecuteChanged();

            //    NotifyPropertyChanged(nameof(this.CurrentMedia));
            //}
        }
        
        private int m_seekMin;
        public int SeekMin
        {
            get { return m_seekMin; }
            set
            {
                m_seekMin = value;
                RaisePropertyChanged(nameof(this.SeekMin));
            }
        }

        private int m_seekMax;
        public int SeekMax
        {
            get { return m_seekMax; }
            set
            {
                m_seekMax = value;
                RaisePropertyChanged(nameof(this.SeekMax));
            }
        }

        private int m_seekValue;
        public int SeekValue
        {
            get { return m_seekValue; }
            set
            {
                m_seekValue = value;
                RaisePropertyChanged(nameof(this.SeekValue));
            }
        }

        private string m_message;
        public string Message
        {
            get { return m_message; }
            set
            {
                m_message = value;
                RaisePropertyChanged(nameof(this.Message));
            }
        }

        public ObservableCollection<string> MediaList { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;


        //public double InputFilterPosition
        //{
        //    get { return ((Player)m_playlist).InputFilterPosition; }
        //}

        //public double OutputFilterPosition
        //{
        //    get { return ((Player)m_playlist).OutputFilterPosition; }
        //}

        //public double PlayPosition
        //{
        //    get { return m_playlist.Position; }
        //}


        public DelegateCommand PlayCommand { get; set; }
        public DelegateCommand PauseCommand { get; set; }
        public DelegateCommand StopCommand { get; set; }
        public DelegateCommand NextCommand { get; set; }
        public DelegateCommand PreviousCommand { get; set; }
        public DelegateCommand SeekStartedCommand { get; set; }
        public DelegateCommand SeekStoppedCommand { get; set; }


        public PlayerViewModel()
        {
            m_playlist = new Playlist();
            m_playlist.StateChanged += M_playlist_StateChanged;
            m_playlist.PlaybackStopped += M_player_PlaybackStopped;
            MediaList = m_playlist.MediaList;

            InitializeCommands();
            InitializeTimer();
            SeekMin = 0;
            SeekMax = 1;
            SeekValue = 0;

            //m_playlist.AddMedia(0, @"C:\Users\Azhar\Desktop\Songs\Trance\A Sky Full Of Stars (Extended Mix).mp3");
            //m_playlist.AddMedia(1, @"C:\Users\Azhar\Desktop\Songs\Trance\A Sky Full Of Stars (Extended Mix).mp3");
            m_playlist.MediaList.Add(@"C:\Users\Azhar\Desktop\Songs\Katti Batti\Katti Batti - 01 - Sarfira.mp3");
            m_playlist.MediaList.Add(@"C:\Users\Azhar\Desktop\Songs\Katti Batti\Katti Batti - 02 - Sau Aasoon.mp3");
            m_playlist.MediaList.Add(@"C:\Users\Azhar\Desktop\Songs\Katti Batti\Katti Batti - 04 - Ove Janiya.mp3");
            RaiseCommandCanExecuteChanged();
        }


        private void InitializeCommands()
        {
            PlayCommand = new DelegateCommand(new Action(Play), new Func<bool>(CanPlay));

            PauseCommand = new DelegateCommand(new Action(Pause), new Func<bool>(CanPause));

            StopCommand = new DelegateCommand(new Action(Stop), new Func<bool>(CanStop));

            NextCommand = new DelegateCommand(new Action(Next), new Func<bool>(CanMoveToNext));

            PreviousCommand = new DelegateCommand(new Action(Previous), new Func<bool>(CanMoveToPrevious));

            SeekStartedCommand = new DelegateCommand(new Action(SeekStarted));

            SeekStoppedCommand = new DelegateCommand(new Action(SeekStopped));
        }

        private void RaiseCommandCanExecuteChanged()
        {
            PlayCommand.RaiseCanExecuteChanged();
            PauseCommand.RaiseCanExecuteChanged();
            StopCommand.RaiseCanExecuteChanged();
            NextCommand.RaiseCanExecuteChanged();
            PreviousCommand.RaiseCanExecuteChanged();
            RaisePropertyChanged(nameof(CurrentMedia));
        }

        private void InitializeTimer()
        {
            m_timer = new DispatcherTimer();
            m_timer.Interval = TimeSpan.FromMilliseconds(100);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start();
        }
        

        private void Play()
        {
            Call(() =>
            {
                m_playlist.Play();
                RaiseCommandCanExecuteChanged();
                SeekMin = 0;
                //CanChangeFileLocation = false;
            });
        }
        private bool CanPlay()
        {
            if (m_playlist.State == PlaybackState.Stopped &&
                string.IsNullOrWhiteSpace(CurrentMedia))
            {
                return false;
            }
            if (m_playlist.State == PlaybackState.Stopped ||
                m_playlist.State == PlaybackState.Paused)
            {
                return true;
            }

            return false;
        }

        private void Pause()
        {
            Call(() =>
            {
                m_playlist.Pause();
                RaiseCommandCanExecuteChanged();
            });
        }
        private bool CanPause()
        {
            if (m_playlist.State == PlaybackState.Playing)
            {
                return true;
            }

            return false;
        }

        private void Stop()
        {
            Call(() =>
            {
                m_playlist.Stop();
                RaiseCommandCanExecuteChanged();
                SeekMin = 0;
                SeekMax = 1;
                SeekValue = 0;
            });
        }
        private bool CanStop()
        {
            if (m_playlist.State == PlaybackState.Playing || m_playlist.State == PlaybackState.Paused)
            {
                return true;
            }

            return false;
        }

        private void Next()
        {
            m_playlist.Next();
            RaiseCommandCanExecuteChanged();
        }
        private bool CanMoveToNext()
        {
            if (m_playlist.MediaList.Count > 1)
            {
                return true;
            }

            return false;
        }

        private void Previous()
        {
            m_playlist.Previous();
            RaiseCommandCanExecuteChanged();
        }
        private bool CanMoveToPrevious()
        {
            if (m_playlist.MediaList.Count > 1)
            {
                return true;
            }

            return false;
        }

        private void SeekStarted()
        {
            m_seekStarted = true;
        }

        private void SeekStopped()
        {
            if (m_playlist.State == PlaybackState.Stopped)
            {
                m_seekStarted = false;
                return;
            }

            m_playlist.Position = SeekValue;
            m_seekStarted = false;
        }

        private void Call(Action method)
        {
            try
            {
                method?.Invoke();
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
        }


        private void M_timer_Tick(object sender, EventArgs e)
        {
            //NotifyPropertyChanged(nameof(InputFilterPosition));
            //NotifyPropertyChanged(nameof(OutputFilterPosition));
            //NotifyPropertyChanged(nameof(PlayPosition));

            if (m_playlist.Length == 0)
            {
                SeekMin = 0;
                SeekMax = 1;
                m_seekValue = 0;
            }
            else
            {
                if (!m_seekStarted)
                {
                    SeekMax = (int)m_playlist.Length;
                    m_seekValue = (int)m_playlist.Position;
                }
            }
            RaisePropertyChanged(nameof(SeekValue));
        }

        private void M_playlist_StateChanged()
        {
            RaiseCommandCanExecuteChanged();
        }

        private void M_player_PlaybackStopped()
        {
            SeekMin = 0;
            SeekMax = 1;
            SeekValue = 0;
            //CanChangeFileLocation = true;
            RaiseCommandCanExecuteChanged();
        }


    }
}
