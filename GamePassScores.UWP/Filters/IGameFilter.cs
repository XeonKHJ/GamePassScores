using GamePassScores.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamePassScores.UWP.Filters
{
    internal interface IGameFilter
    {
        List<Game> Filter();
        IGameFilter NextFilter();
    }
}
