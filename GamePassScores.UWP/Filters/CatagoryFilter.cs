using GamePassScores.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamePassScores.UWP.Filters
{
    internal class CatagoryFilter : IGameFilter
    {
        IGameFilter _nextFilter = null;
        public CatagoryFilter(IGameFilter nextFilter)
        {

        }
        public List<Game> Filter()
        {
            return null;
        }

        public IGameFilter NextFilter()
        {
            throw new NotImplementedException();
        }
    }
}
