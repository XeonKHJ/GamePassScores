using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamePassScores.InfoCollectorConsole
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class AggregateRating
    {
        [JsonProperty("@type")]
        public string Type { get; set; }
        public string bestRating { get; set; }
        public string worstRating { get; set; }
        public string ratingValue { get; set; }
        public string ratingCount { get; set; }
    }

    public class Trailer
    {
        [JsonProperty("@type")]
        public string Type { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string thumbnailUrl { get; set; }
        public string uploadDate { get; set; }
    }

    public class Publisher
    {
        [JsonProperty("@type")]
        public string Type { get; set; }
        public string name { get; set; }
        public string url { get; set; }
    }

    public class MetacriticScoreModel
    {
        [JsonProperty("@context")]
        public string Context { get; set; }
        [JsonProperty("@type")]
        public string Type { get; set; }
        public string applicationCategory { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string datePublished { get; set; }
        public string url { get; set; }
        public string image { get; set; }
        public AggregateRating aggregateRating { get; set; }
        public string contentRating { get; set; }
        public string gamePlatform { get; set; }
        public string operatingSystem { get; set; }
        public Trailer trailer { get; set; }
        public List<Publisher> publisher { get; set; }
        public List<string> genre { get; set; }
    }


}
