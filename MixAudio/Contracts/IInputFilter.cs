using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MixAudio
{
    public interface IInputFilter : IFilter
    {
        IFormat InFormat { get; }
        string MediaSource { get; set; }
        double BufferLength { get; }
        double Position { get; set; }
        double Length { get; }
        void Flush();
    }
}
