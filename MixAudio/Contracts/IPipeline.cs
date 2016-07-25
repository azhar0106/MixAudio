using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MixAudio
{
    public interface IPipeline
    {
        int FilterCount { get; }
        void AddFilter(IFilter filter, int position);
        void RemoveFilter(IFilter filter);
        void AddInputOutputFilters(IInputFilter inputFilter, IOutputFilter outputFilter);
    }
}
