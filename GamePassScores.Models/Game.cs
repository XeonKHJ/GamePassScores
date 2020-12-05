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

        public string MetaCriticPathName { set; get; } = string.Empty;

        public List<Platform> OriginalPlatforms { set; get; } = new List<Platform>();

        public HashSet<string> Categories = new HashSet<string>();

        public Dictionary<Platform, Uri> MetacriticUrls = new Dictionary<Platform, Uri>();

        public Dictionary<Platform, int> MetaScore = new Dictionary<Platform, int>();
        public InVaultTime InVaultTime { set; get; } = InVaultTime.Normal;
        public string PosterUrl { set; get; } = string.Empty;
        public bool IsMetacriticInfoCorrect { set; get; } = false;
        public bool IsMetacriticInfoExist { set; get; } = true;

        public long ReleaseDate { set; get; }
        public Dictionary<string, long> DownloadSize { set; get; } = new Dictionary<string, long>();
    }
}
