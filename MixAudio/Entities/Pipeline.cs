using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MixAudio
{
    public class Pipeline : IPipeline
    {
        IInputFilter m_inputFilter;
        IOutputFilter m_outputFilter;
        List<IFilter> m_filters = new List<IFilter>();

        
        public int FilterCount
        {
            get { return m_filters.Count; }
        }
        
        public Pipeline()
        {

        }

        public void AddFilter(IFilter filter, int position) //TODO: remove position parameter
        {
            if (m_filters.Contains(filter))
            {
                return;
            }

            if (position == 0)
            {
                if (m_filters.Count != 0) // Non-empty filter list
                {
                    throw new InvalidOperationException("Filters should be added in first to last order.");
                    //m_filters[0].InSample = filter.OutSample;
                }
            }
            else
            {
                filter.InSample = m_filters[position - 1].OutSample;

                if (m_filters.Count != position) // Not a last filter
                {
                    throw new InvalidOperationException("Filters should be added in first to last order.");
                    //m_filters[position].InSample = filter.OutSample;
                }

            }

            filter.Open();

            m_filters.Insert(position, filter);
        }

        public void RemoveFilter(IFilter filter)
        {
            if (!m_filters.Contains(filter))
            {
                return;
            }

            m_filters.Remove(filter);
        }

        public void AddInputOutputFilters(IInputFilter inputFilter, IOutputFilter outputFilter)
        {
            if (inputFilter == null)
            {
                throw new ArgumentNullException(nameof(inputFilter));
            }
            if (outputFilter == null)
            {
                throw new ArgumentNullException(nameof(outputFilter));
            }


            m_inputFilter = inputFilter;
            m_outputFilter = outputFilter;


            m_inputFilter.Open();

            m_outputFilter.InSample = m_inputFilter.OutSample;
            m_outputFilter.DataRequired += Filter;
            m_outputFilter.Open();
        }
        
        private bool Filter()
        {
            if (!m_inputFilter.FilterData())
            {
                return false;
            }

            for (int fIndex = 0; fIndex < FilterCount; fIndex++)
            {
                var filter = m_filters[fIndex];

                if (!filter.FilterData())
                {
                    return false;
                }
            }

            if (!m_outputFilter.FilterData())
            {
                return false;
            }

            return true;
        }
    }
}
