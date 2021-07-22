using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GamePassScores.InfoCollectorConsole
{
    internal class InfoCollectorOption
    {
        public string OldInfoFilePath { set; get; }
        public List<RepoOption> RepoOptions { set; get; }
    }
    internal class ArgParser
    {
        public async static Task<InfoCollectorOption> ParseJsonAsync(string path)
        {
            var jsonString = await System.IO.File.ReadAllTextAsync(path);

            var options = JsonConvert.DeserializeObject<InfoCollectorOption>(jsonString);

            return options;
        }
    }
}
