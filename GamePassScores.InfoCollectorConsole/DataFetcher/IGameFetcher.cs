using GamePassScores.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamePassScores.InfoCollectorConsole.DataFetcher
{
    internal interface IGameFetcher
    {
        public Task<List<Game>> GetGamesAsync(List<Game> existedGames = null);
    }
}
