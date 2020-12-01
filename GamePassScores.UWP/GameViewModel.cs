using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;

namespace GamePassScores.UWP
{
    public class GameViewModel
    {
        public string Title { set; get; }
        public string Description { set; get; }
        public string PosterUrl { set; get; }
        public int Metascore { set; get; } = -1;
        public Uri MetacriticUrl { set; get; }
        public GameViewModel(Models.Game game)
        {
            Title = game.Title.First().Value;
            Description = game.Description.First().Value;
            PosterUrl = game.PosterUrl;

            if(game.MetaScore.Count != 0)
            {
                Metascore = game.MetaScore.First().Value;
                MetacriticUrl = game.MetacriticUrls.First().Value;
            }
        }

        public Visibility IsScoreAvaliable
        {
            get
            {
                return Metascore >= 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            
        }
        public Color ScoreColor
        {
            get
            {
                if(Metascore > 75)
                {
                    return Color.FromArgb(0xff, 0x74, 0xcb, 0x2c);
                }
                else if(Metascore > 50)
                {
                    return Color.FromArgb(0xff, 0xFB, 0xCC, 0x21);
                }
                else
                {
                    return Color.FromArgb(0xff, 0xF5, 0x16, 0x00);
                }
            }
        }
    }
}
