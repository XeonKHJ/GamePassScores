using GamePassScores.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamePassScores.InfoCollectorConsole.DataSaver
{
    internal interface IDataSaver
    {
        public Task SaveAsync(IList<Game> games);
    }
}
