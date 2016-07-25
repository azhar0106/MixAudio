using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MixAudio
{
    class WaveSinkFilter : IOutputFilter
    {
        int m_desiredLatencyInMs = 5000;
        int m_bytesPerSample = 4;
        WaveProvider m_waveProvider;
        WaveOut m_waveOutPlayer;


        private ISample m_inSample;
        public ISample InSample
        {
            get { return m_inSample; }
            set { m_inSample = value; }
        }

        private ISample m_outSample;
        public ISample OutSample
        {
            get { return m_outSample; }
            set { m_outSample = value; }
        }

        IFormat m_outFormat;
        public IFormat OutFormat
        {
            private set { m_outFormat = value; }
            get { return m_outFormat; }
        }

        bool m_isOpen;
        public bool IsOpen
        {
            private set { m_isOpen = value; }
            get { return m_isOpen; }
        }

        long m_latency;
        public long Latency
        {
            private set { m_latency = value; }
            get { return m_latency; }
        }

        public double Position
        {
            get
            {
                double position = Convert.ToDouble(((WaveOut)m_waveOutPlayer).GetPosition());
                position = position / OutFormat.Channels / OutFormat.BytesPerSample
                    / OutFormat.SampleRate;
                return position;
            }
        }

        public event Func<bool> DataRequired;

        public event Action PlaybackStopped;


        public WaveSinkFilter()
        {
            m_waveOutPlayer = null;
            m_waveProvider = null;
        }


        public void Open()
        {
            if (InSample == null)
            {
                throw new InvalidOperationException("In Sample is not set.");
            }
            if (DataRequired == null)
            {
                throw new InvalidOperationException("DataRequired event is not subscribed to.");
            }

            Format outFormat = new Format(
                InSample.SampleRate,
                4,
                InSample.Channels);
            OutSample = null;
            OutFormat = outFormat;
            
            m_waveProvider = new WaveProvider(DataRequired, this);

            m_waveOutPlayer = new WaveOut();
            m_waveOutPlayer.NumberOfBuffers = 2;
            m_waveOutPlayer.DesiredLatency = m_desiredLatencyInMs;
            m_waveOutPlayer.PlaybackStopped += WaveOut_PlaybackStopped;

            Latency = m_desiredLatencyInMs *
                m_bytesPerSample *
                OutFormat.Channels *
                OutFormat.SampleRate /
                1000;
            
            m_waveOutPlayer.Init(m_waveProvider);

            IsOpen = true;
        }

        public void Close()
        {
            m_waveOutPlayer.PlaybackStopped -= WaveOut_PlaybackStopped;

            InSample = null;
            OutSample = null;

            if (m_waveOutPlayer != null)
            {
                m_waveOutPlayer.Dispose();
                m_waveOutPlayer = null;
            }

            if (m_waveProvider != null)
            {
                m_waveProvider = null;
            }

            IsOpen = false;
        }

        public bool FilterData()
        {
            return true;
        }

        public void Play()
        {
            if (!IsOpen)
            {
                throw new InvalidOperationException("Filter has not been opened for playback yet.");
            }

            m_waveOutPlayer.Play();
        }

        public void Stop()
        {
            m_waveOutPlayer.Stop();
            //m_stopEvent.WaitOne();
        }
        
        
        private void WaveOut_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            Debug.WriteLine("OUT: Playback stopped.");
            m_waveProvider.NoMoreData = false;
            PlaybackStopped?.Invoke();
        }


        public class WaveProvider : IWaveProvider
        {
            WaveFormat m_waveFormat;
            WaveSinkFilter m_filter;
            Func<bool> m_getData;
            int m_bytesWritten = 0;
            int m_totalBytesWritten;
            int m_blockSize;


            public bool NoMoreData { get; set; }


            public WaveProvider(Func<bool> getData, WaveSinkFilter filter)
            {
                int channels = filter.OutFormat.Channels;
                int bytesPerSample = filter.OutFormat.BytesPerSample;
                int sampleRate = filter.OutFormat.SampleRate;

                m_waveFormat = WaveFormat.CreateCustomFormat(
                    WaveFormatEncoding.Pcm,
                    sampleRate,
                    channels,
                    channels * bytesPerSample * sampleRate,
                    channels * bytesPerSample,
                    bytesPerSample * 8
                    );
                m_waveFormat = new WaveFormat(
                    sampleRate,
                    bytesPerSample * 8,
                    channels
                    );

                m_getData = getData;
                m_filter = filter;

                m_blockSize = m_filter.OutFormat.Channels * m_filter.OutFormat.BytesPerSample;
            }

            public WaveFormat WaveFormat
            {
                get
                {
                    return m_waveFormat;
                }
            }

            public int Read(byte[] buffer, int offset, int bufferSize)
            {
                m_bytesWritten = 0;

                while (m_bytesWritten < (bufferSize - offset) && !NoMoreData)
                {
                    if (false == m_getData())
                    {
                        Debug.Write($"OUT: No more data.\n");
                        NoMoreData = true;
                        //m_filter.WaveOut_PlaybackStopped(m_filter, null);
                        break;
                    }

                    if (!m_filter.InSample.WriteBlockToBuffer(
                        buffer,
                        offset + m_bytesWritten,
                        m_filter.OutFormat
                        ))
                    {
                        throw new InvalidOperationException("Could not write block to buffer.");
                    }
                    //Debug.Write("w");

                    m_bytesWritten += m_blockSize;
                    m_totalBytesWritten += m_blockSize;
                }
                Debug.Write($"OUT: Bytes written: {m_bytesWritten}\n");
                Debug.Write($"OUT: Total bytes written: {m_totalBytesWritten}\n");
                Debug.Write($"OUT: Position: {m_filter.Position}\n");

                return m_bytesWritten;
            }
        }
    }
}
