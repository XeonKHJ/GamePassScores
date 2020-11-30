using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamePassScores.InfoCollectorConsole
{
    public class MyArray
    {
        public string siglId { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string requiresShuffling { get; set; }
        public string imageUrl { get; set; }
        public string id { get; set; }
    }

    public class GameListInfoModel
    {
        public List<MyArray> MyArray { get; set; }
    }
}
