using GamePassScores.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamePassScores.UWP.InfoLoader
{
    internal class GameInfoLoader : IInfoLoader
    {
        private string _url;
        public GameInfoLoader(string url)
        {
            _url = url;
        }
        public Task<List<Game>> LoadAsync()
        {
            throw new NotImplementedException();
        }

        public Task RefreshAsync()
        {
            throw new NotImplementedException();
        }
    }
}
