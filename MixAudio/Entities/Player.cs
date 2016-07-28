using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace MixAudio
{
    public class Player : IPlayer, INotifyPropertyChanged
    {
        SynchronizationContext m_syncContext;
        Thread m_threadSta;
        Dispatcher m_threadDispatcher;
        DispatcherFrame m_dispatcherFrame;
        IPipeline m_pipeline;
        IInputFilter m_inputFilter;
        IOutputFilter m_outputFilter;
        double m_positionAtLastPause;
        

        private string m_currentMedia;
        public string CurrentMedia
        {
            get
            {
                return m_currentMedia;
            }

            set
            {
                if (State != PlaybackState.Stopped)
                {
                    throw new InvalidOperationException("Source can be set when Player is in stopped state.");
                }

                if (m_currentMedia == value)
                {
                    return;    
                }

                m_currentMedia = value;

                NotifyPropertyChanged(nameof(this.CurrentMedia));
            }
        }

        public double Position
        {
            get
            {
                if (State == PlaybackState.Stopped)
                {
                    return 0.0;
                }

                if (m_outputFilter != null)
                {
                    if (m_outputFilter.IsOpen)
                    {
                        return m_positionAtLastPause + m_outputFilter.Position;
                    }
                    else
                    {
                        return 0.0;
                    }
                }
                else
                {
                    return 0.0;
                }
            }
            set
            {
                SeekTo(value);
            }
        }

        public double Length
        {
            get
            {
                if (m_inputFilter == null)
                {
                    return 0.0;
                }
                else
                {
                    if (m_inputFilter.IsOpen)
                    {
                        return m_inputFilter.Length;
                    }
                    else
                    {
                        return 0.0;
                    }
                }
            }
        }

        public double InputFilterPosition
        {
            get
            {
                if (m_inputFilter != null)
                {
                    if (m_inputFilter.IsOpen)
                    {
                        return m_inputFilter.Position;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    return 0;
                }
            }
        }

        public double OutputFilterPosition
        {
            get
            {
                if (m_outputFilter != null)
                {
                    if (m_outputFilter.IsOpen)
                    {
                        return m_outputFilter.Position;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    return 0;
                }
            }
        }


        private PlaybackState m_state;
        public PlaybackState State
        {
            get
            {
                return m_state;
            }

            private set
            {
                if (value == m_state)
                {
                    return;
                }

                m_state = value;
                NotifyPropertyChanged(nameof(this.State));

                if (StateChanged != null)
                {
                    if (m_syncContext != null)
                    {
                        m_syncContext.Post(new SendOrPostCallback((o) =>
                        {
                            StateChanged();
                        }), null);
                    }
                    else
                    {
                        StateChanged();
                    }
                }
            }
        }
        public event Action StateChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        public event Action PlaybackStopped;


        public Player()
        {
            m_state = PlaybackState.Stopped;

            m_syncContext = SynchronizationContext.Current;
            StartThread();
        }


        public void Play()
        {
            Debug.WriteLine("PL:M: Play");

            if (String.IsNullOrWhiteSpace(CurrentMedia))
            {
                throw new InvalidOperationException("Media Source is not specified.");
            }

            Action playAction;

            if (State == PlaybackState.Playing || State == PlaybackState.Pausing)
            {
                return;
            }
            else if (State == PlaybackState.Paused)
            {
                playAction = new Action(() =>
                {
                    m_outputFilter.Play();
                    State = PlaybackState.Playing;
                });
            }
            else// if (State == State.Stopped)
            {
                playAction = new Action(() =>
                {
                    m_pipeline = new Pipeline();

                    m_inputFilter = new FileSourceFilter();
                    m_inputFilter.MediaSource = this.CurrentMedia;
                    
                    m_outputFilter = new WaveSinkFilter();
                    m_outputFilter.PlaybackStopped += M_outputFilter_Stopped;

                    //m_pipeline.AddFilter(m_inputFilter, 0);
                    //m_pipeline.AddFilter(m_outputFilter, 1);
                    m_pipeline.AddInputOutputFilters(m_inputFilter, m_outputFilter);

                    m_positionAtLastPause = 0.0;

                    NotifyPropertyChanged(nameof(Length));

                    m_outputFilter.Play();
                    State = PlaybackState.Playing;
                });
            }

            m_threadDispatcher.Invoke(playAction);
        }

        public void Pause()
        {
            Debug.WriteLine("PL:M: Pause");

            if (State == PlaybackState.Stopped || State == PlaybackState.Paused || State == PlaybackState.Pausing)
            {
                return;
            }

            State = PlaybackState.Pausing;
            m_threadDispatcher.Invoke(new Action(() =>
            {
                double outputPosition = m_outputFilter.Position;

                m_outputFilter.Stop();
                m_dispatcherFrame = new DispatcherFrame(true);
                Dispatcher.PushFrame(m_dispatcherFrame);

                double currentPosition = 
                    m_positionAtLastPause + 
                    outputPosition;

                m_inputFilter.Position = currentPosition;

                m_positionAtLastPause = currentPosition;
                State = PlaybackState.Paused;
            }));
            
        }

        public void Stop()
        {
            Debug.WriteLine("PL:M: Stop");

            if (State == PlaybackState.Stopped || State == PlaybackState.Pausing)
            {
                return;
            }

            m_threadDispatcher.Invoke(new Action(() => 
            {
                m_outputFilter.Stop();
                m_dispatcherFrame = new DispatcherFrame(true);
                Dispatcher.PushFrame(m_dispatcherFrame);

                m_positionAtLastPause = 0.0;
                m_outputFilter.PlaybackStopped -= M_outputFilter_Stopped;

                State = PlaybackState.Stopped;
            }));
        }

        private void SeekTo(double position)
        {
            Debug.WriteLine("PL:M: SeekTo");

            if (State == PlaybackState.Stopped || State == PlaybackState.Pausing)
            {
                return;
            }

            State = PlaybackState.Seeking;
            m_threadDispatcher.Invoke(new Action(() =>
            {
                //Stop Output and wait for the Stopped event
                //Debug.WriteLine("Stopping Output...");
                m_outputFilter.Stop();
                m_dispatcherFrame = new DispatcherFrame(true);
                Dispatcher.PushFrame(m_dispatcherFrame);

                m_inputFilter.Position = position;
                m_positionAtLastPause = position;
                //Debug.WriteLine("Playing Output...");
                m_outputFilter.Play();
                State = PlaybackState.Playing;
            }));
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                if (m_syncContext != null)
                {
                    m_syncContext.Post(new SendOrPostCallback((o) =>
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                    }), null);
                }
                else
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }
        }
        
        private void M_outputFilter_Stopped()
        {
            Debug.WriteLine("PL:M: M_outputFilter_Stopped");
            // Signals wait at Stop in Seeking
            if (m_dispatcherFrame != null)
            {
                m_dispatcherFrame.Continue = false;
                m_dispatcherFrame = null;
            }

            if (State == PlaybackState.Pausing || State == PlaybackState.Seeking)
            {
                return;
            }

            State = PlaybackState.Stopped;


            if (PlaybackStopped != null)
            {
                if (m_syncContext != null)
                {
                    m_syncContext.Post(new SendOrPostCallback((o) =>
                    {
                        PlaybackStopped();
                    }), null);
                }
                else
                {
                    PlaybackStopped();
                }
            }
        }

        private void StartThread()
        {
            m_threadSta = new Thread(new ThreadStart(() => 
            {
                m_threadDispatcher = Dispatcher.CurrentDispatcher;
                Dispatcher.Run();
            }));
            m_threadSta.SetApartmentState(ApartmentState.STA);
            m_threadSta.IsBackground = true;
            m_threadSta.Start();
        }
    }
}
