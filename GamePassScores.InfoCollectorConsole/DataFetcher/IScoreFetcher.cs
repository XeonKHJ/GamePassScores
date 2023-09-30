using GamePassScores.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamePassScores.InfoCollectorConsole.DataFetcher
{
    internal interface IScoreFetcher
    {
        public Task FetchScoresAsync(IList<Game> games);
    }
}
