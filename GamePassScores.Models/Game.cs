using System;
using System.Collections.Generic;
using System.Globalization;

namespace GamePassScores.Models
{
    public class Game
    {
        public string ID { set; get; } = string.Empty;

        public Dictionary<string, string> Title { set; get; } = new Dictionary<string, string>();

        public Dictionary<string, string> Description { set; get; } = new Dictionary<string, string>();

        public List<SubscriptionServices> Affirmations { set; get; } = new List<SubscriptionServices>();
    }
}
