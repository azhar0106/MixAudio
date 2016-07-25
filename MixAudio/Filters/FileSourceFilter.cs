using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MixAudio
{
    public class FileSourceFilter : IInputFilter
    {
        int m_bufferSizeInMs = 5000;
        int m_bufferSize;
        int m_writtenBufferLength;
        int m_blockLength;
        byte[] m_buffer;
        int m_bufferPos;
        MediaFoundationReader m_fileReader;
        string m_mediaSource;
        bool m_isOpen;


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
        
        IFormat m_inFormat;
        public IFormat InFormat
        {
            private set { m_inFormat = value; }
            get { return m_inFormat; }
        }
        
        public bool IsOpen
        {
            private set { m_isOpen = value; }
            get { return m_isOpen; }
        }
        
        public string MediaSource
        {
            get
            {
                return m_mediaSource;
            }

            set
            {
                if (m_isOpen)
                {
                    throw new InvalidOperationException("Cannot set Media Source while filter is open.");
                }

                m_mediaSource = value;
            }
        }
        
        public double BufferLength
        {
            get
            {
                return ((double)m_bufferSize) / m_blockLength / InFormat.SampleRate;
            }
        }
        
        public double Length
        {
            get
            {
                if (!m_isOpen)
                {
                    throw new InvalidOperationException("Filter is not open.");
                }

                double length = (double)m_fileReader.Length /
                    InFormat.SampleRate /
                    InFormat.BytesPerSample /
                    InFormat.Channels;
                return length;
            }
        }

        public double Position
        {
            get
            {
                if (!m_isOpen)
                {
                    throw new InvalidOperationException("Filter is not open.");
                }

                double position = Convert.ToDouble(m_fileReader.Position) /
                    InFormat.Channels /
                    InFormat.BytesPerSample /
                    InFormat.SampleRate;

                return position;
            }

            set
            {
                if (!m_isOpen)
                {
                    throw new InvalidOperationException("Filter is not open.");
                }

                Flush();
                long position = Convert.ToInt64(value * InFormat.Channels * InFormat.BytesPerSample
                    * InFormat.SampleRate);
                m_fileReader.Position = position;
            }
        }


        public FileSourceFilter()
        {
        }
        

        public void Open()
        {
            if (MediaSource == null)
            {
                throw new InvalidOperationException("Media Source is not specified.");
            }
            
            m_fileReader = new MediaFoundationReader(MediaSource);

            InFormat = new Format(
                m_fileReader.WaveFormat.SampleRate,
                m_fileReader.WaveFormat.BitsPerSample / 8,
                m_fileReader.WaveFormat.Channels
                );

            m_bufferSize =
                (m_bufferSizeInMs * InFormat.SampleRate / 1000) *
                InFormat.Channels *
                InFormat.BytesPerSample
                ;

            m_writtenBufferLength = m_bufferSize;

            m_blockLength = InFormat.Channels * InFormat.BytesPerSample;
            
            m_buffer = new byte[m_bufferSize];

            m_bufferPos = m_bufferSize;
            
            OutSample = new Sample();
            OutSample.SetSampleParams(InFormat);

            IsOpen = true;
        }

        public void Close()
        {
            InSample = null;
            OutSample = null;
            
            m_fileReader.Dispose();
            m_fileReader = null;
            m_bufferSize = 0;
            m_blockLength = 0;
            m_buffer = null;
            m_bufferPos = 0;

            IsOpen = false;
        }
        
        public bool FilterData()
        {
            if (m_bufferPos == m_writtenBufferLength)
            {
                m_writtenBufferLength = m_fileReader.Read(m_buffer, 0, m_bufferSize);
                Debug.WriteLine($"IN: Position: {Position}");

                if (m_writtenBufferLength == 0)
                {
                    Debug.WriteLine("IN: No more data.");
                    return false;
                }
                else if (m_writtenBufferLength < m_bufferSize)
                {
                    Debug.WriteLine($"IN: Bytes Partially Read: {m_writtenBufferLength}");
                    m_bufferPos = 0;
                }
                else //if (m_writtenBufferLength == m_bufferSize)
                {
                    Debug.WriteLine($"IN: Bytes Fully Read: {m_writtenBufferLength}");
                    m_bufferPos = 0;
                }
            }

            if (!OutSample.WriteBlockFromBuffer(m_buffer, m_bufferPos, InFormat))
            {
                Debug.WriteLine($"IN: m_writtenBufferLength: {m_writtenBufferLength}");
                Debug.WriteLine($"IN: m_buffer.Length: {m_buffer.Length}");
                Debug.WriteLine($"IN: m_bufferPos: {m_bufferPos}");
                Debug.WriteLine($"IN: Position: {Position}");
                throw new InvalidOperationException("Could not write block from buffer.");
            }
            //Debug.Write("f");

            m_bufferPos += m_blockLength;

            return true;
        }

        public void Flush()
        {
            m_bufferPos = m_writtenBufferLength;
        }
    }
}
