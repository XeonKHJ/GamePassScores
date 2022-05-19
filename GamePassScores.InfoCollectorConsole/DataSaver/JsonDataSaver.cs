using GamePassScores.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamePassScores.InfoCollectorConsole.DataSaver
{
    internal class JsonDataSaver : IDataSaver
    {
        private string _saveFilePath = string.Empty;
        public JsonDataSaver(string savePath)
        {
            _saveFilePath = savePath;
        }
        public Task SaveAsync(IList<Game> games)
        {
            var serializeGames = JsonConvert.SerializeObject(games);
           

            return null;
        }
    }
}
