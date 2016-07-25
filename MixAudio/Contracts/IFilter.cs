using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MixAudio
{
    public interface IFilter : IConfigurable
    {
        ISample InSample { get; set; }
        ISample OutSample { get; set; }
        bool IsOpen { get; }
        void Open();
        void Close();
        bool FilterData();
    }
}
