using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace MixAudio
{
    public class PlayerViewModel : INotifyPropertyChanged
    {
        IPlayer m_player;
        DispatcherTimer m_timer;
        bool m_seekStarted;


        private string m_fileLocation;
        public string FileLocation
        {
            get { return m_fileLocation; }
            set
            {
                if (value == m_fileLocation)
                {
                    return;
                }
                if (!CanChangeFileLocation)
                {
                    return;
                }

                m_fileLocation = value;
                m_player.MediaSource = value;
                RaiseCommandCanExecuteChanged();

                NotifyPropertyChanged(nameof(this.FileLocation));
            }
        }

        private bool m_canChangeFileLocation;
        public bool CanChangeFileLocation
        {
            get { return m_canChangeFileLocation; }
            set
            {
                if (value == m_canChangeFileLocation)
                {
                    return;
                }

                m_canChangeFileLocation = value;

                NotifyPropertyChanged(nameof(this.CanChangeFileLocation));
            }
        }
        
        private int m_seekMin;
        public int SeekMin
        {
            get { return m_seekMin; }
            set
            {
                m_seekMin = value;
                NotifyPropertyChanged(nameof(this.SeekMin));
            }
        }

        private int m_seekMax;
        public int SeekMax
        {
            get { return m_seekMax; }
            set
            {
                m_seekMax = value;
                NotifyPropertyChanged(nameof(this.SeekMax));
            }
        }

        private int m_seekValue;
        public int SeekValue
        {
            get { return m_seekValue; }
            set
            {
                m_seekValue = value;
                NotifyPropertyChanged(nameof(this.SeekValue));
            }
        }

        private string m_message;
        public string Message
        {
            get { return m_message; }
            set
            {
                m_message = value;
                NotifyPropertyChanged(nameof(this.Message));
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;


        public double InputFilterPosition
        {
            get { return ((Player)m_player).InputFilterPosition; }
        }

        public double OutputFilterPosition
        {
            get { return ((Player)m_player).OutputFilterPosition; }
        }

        public double PlayPosition
        {
            get { return m_player.Position; }
        }


        public DelegateCommand PlayCommand { get; set; }
        public DelegateCommand PauseCommand { get; set; }
        public DelegateCommand StopCommand { get; set; }
        public DelegateCommand SeekStartedCommand { get; set; }
        public DelegateCommand SeekStoppedCommand { get; set; }


        public PlayerViewModel()
        {
            m_player = new Player();
            m_player.PlaybackStopped += M_player_PlaybackStopped;
            InitializeCommands();
            InitializeTimer();
            CanChangeFileLocation = true;
            FileLocation = @"C:\Users\Azhar\Desktop\Songs\Trance\A Sky Full Of Stars (Extended Mix).mp3";
            SeekMin = 0;
            SeekMax = 1;
            SeekValue = 0;
        }


        private void InitializeCommands()
        {
            PlayCommand = new DelegateCommand(new Action<object>(Play), new Predicate<object>(CanPlay));

            PauseCommand = new DelegateCommand(new Action<object>(Pause), new Predicate<object>(CanPause));

            StopCommand = new DelegateCommand(new Action<object>(Stop), new Predicate<object>(CanStop));

            SeekStartedCommand = new DelegateCommand(new Action<object>(SeekStarted));

            SeekStoppedCommand = new DelegateCommand(new Action<object>(SeekStopped));
        }

        private void RaiseCommandCanExecuteChanged()
        {
            PlayCommand.RaiseCanExecuteChanged();
            PauseCommand.RaiseCanExecuteChanged();
            StopCommand.RaiseCanExecuteChanged();
        }

        private void InitializeTimer()
        {
            m_timer = new DispatcherTimer();
            m_timer.Interval = TimeSpan.FromMilliseconds(100);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start();
        }

        private void Play(object parameter)
        {
            Call(() =>
            {
                m_player.Play();
                RaiseCommandCanExecuteChanged();
                SeekMin = 0;
                CanChangeFileLocation = false;
            });
        }
        private bool CanPlay(object parameter)
        {
            if (m_player.State == PlaybackState.Stopped &&
                string.IsNullOrWhiteSpace(FileLocation))
            {
                return false;
            }
            if (m_player.State == PlaybackState.Stopped ||
                m_player.State == PlaybackState.Paused)
            {
                return true;
            }

            return false;
        }

        private void Pause(object parameter)
        {
            Call(() =>
            {
                m_player.Pause();
                RaiseCommandCanExecuteChanged();
            });
        }
        private bool CanPause(object parameter)
        {
            if (m_player.State == PlaybackState.Playing)
            {
                return true;
            }

            return false;
        }

        private void Stop(object parameter)
        {
            Call(() =>
            {
                m_player.Stop();
                RaiseCommandCanExecuteChanged();
                SeekMin = 0;
                SeekMax = 1;
                SeekValue = 0;
            });
        }
        private bool CanStop(object parameter)
        {
            if (m_player.State == PlaybackState.Playing || m_player.State == PlaybackState.Paused)
            {
                return true;
            }

            return false;
        }

        private void SeekStarted(object parameter)
        {
            m_seekStarted = true;
        }

        private void SeekStopped(object parameter)
        {
            if (m_player.State == PlaybackState.Stopped)
            {
                m_seekStarted = false;
                return;
            }

            m_player.SeekTo(SeekValue);
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
            NotifyPropertyChanged(nameof(InputFilterPosition));
            NotifyPropertyChanged(nameof(OutputFilterPosition));
            NotifyPropertyChanged(nameof(PlayPosition));

            if (m_player.Length == 0)
            {
                SeekMin = 0;
                SeekMax = 1;
                m_seekValue = 0;
            }
            else
            {
                if (!m_seekStarted)
                {
                    SeekMax = (int)m_player.Length;
                    m_seekValue = (int)m_player.Position;
                }
            }
            NotifyPropertyChanged(nameof(SeekValue));
        }

        private void M_player_PlaybackStopped()
        {
            SeekMin = 0;
            SeekMax = 1;
            SeekValue = 0;
            CanChangeFileLocation = true;
            RaiseCommandCanExecuteChanged();
        }


        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
