using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MixAudio
{
    public class Sample : ISample
    {
        public int Channels { get; set; }
        
        public double[] Block { get; set; }

        public int SampleRate { get; set; }
        
        public void SetSampleParams(IFormat format)
        {
            Block = new double[format.Channels];
            SampleRate = format.SampleRate;
            Channels = format.Channels;
        }

        public bool WriteBlockFromBuffer(byte[] buffer, int offset, IFormat format)
        {
            double amp = Math.Pow(2, (format.BytesPerSample * 8) - 1) - 1;
            int blockLength = format.Channels * format.BytesPerSample;
            if (buffer.Length - offset < blockLength)
            {
                return false;
            }

            for (int c = 0; c < format.Channels; c++)
            {
                //byte[] bytes = new byte[format.BytesPerSample];
                //for (int b = 0; b < bytes.Length; b++)
                //{
                //    bytes[b] = buffer[offset + (c * format.BytesPerSample) + b];
                //}

                if (format.BytesPerSample == 4)
                {
                    Int32 value = BitConverter.ToInt32(buffer, offset + (c * format.BytesPerSample));
                    Block[c] = Convert.ToDouble(value) / amp;
                }
                else if (format.BytesPerSample == 2)
                {
                    Int16 value = BitConverter.ToInt16(buffer, offset + (c * format.BytesPerSample));
                    Block[c] = Convert.ToDouble(value) / amp;
                }
            }

            return true;
        }

        public bool WriteBlockToBuffer(byte[] buffer, int offset, IFormat format)
        {
            double amp = Math.Pow(2, (format.BytesPerSample * 8) - 1) - 1;
            int blockLength = format.Channels * format.BytesPerSample;
            if (buffer.Length - offset < blockLength)
            {
                return false;
            }
            
            for (int c = 0; c < format.Channels; c++)
            {
                //byte[] bytes;

                if (format.BytesPerSample == 4)
                {
                    Int32 value = Convert.ToInt32(amp * Block[c]);
                    //bytes = BitConverter.GetBytes(value);

                    unsafe
                    {
                        fixed (byte* b = &buffer[offset + (c * format.BytesPerSample)])
                            *((Int32*)b) = value;
                    }
                }
                else if (format.BytesPerSample == 2)
                {
                    Int16 value = Convert.ToInt16(amp * Block[c]);
                    //bytes = BitConverter.GetBytes(value);

                    unsafe
                    {
                        fixed (byte* b = &buffer[offset + (c * format.BytesPerSample)])
                            *((Int16*)b) = value;
                    }
                }
                else
                {
                    throw new InvalidOperationException("Double to this integer is not supported");
                }

                //for (int b = 0; b < format.BytesPerSample; b++)
                //{
                //    buffer[offset + (c * format.BytesPerSample) + b] = bytes[b];
                //}
            }

            return true;
        }
    }
}
