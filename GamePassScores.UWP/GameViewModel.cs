using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using System.Net.Http;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using Windows.Storage;
using System.IO;
using Windows.System.Profile;

namespace GamePassScores.UWP
{
    public class GameViewModel : INotifyPropertyChanged
    {
        public Models.Game Game { set; get; } = new Models.Game();
        public string Title { set; get; }
        public string Description { set; get; }
        public string PosterUrl { set; get; }

        public Dictionary<string, long> DownloadSize { set; get; } = new Dictionary<string, long>();
        public DateTime ReleaseDate { set; get; }
        private string _posterPath;
        public string PosterPath
        {
            get
            {
                FileInfo posterCacheInfo = new FileInfo(ApplicationData.Current.LocalFolder.Path + "\\PostersCache\\" + ID + ".jpg");
                
                if (posterCacheInfo.Exists)
                {
                    IsPosterCached = true;
                    return posterCacheInfo.FullName;
                }
                else
                {
                    LoadImage();
                    return null;
                }
            }
        }
        public int Metascore { set; get; } = -1;
        public Uri MetacriticUrl { set; get; }
        public List<string> Categories { set; get; } = new List<string>();
        public string ID { set; get; }
        public GameViewModel(Models.Game game)
        {
            Game = game;
            Title = game.Title.First().Value;
            Description = game.Description.First().Value;
            PosterUrl = game.PosterUrl;
            ID = game.ID;
            Categories.AddRange(game.Categories);
            ReleaseDate = DateTime.FromBinary(game.ReleaseDate);
            if (game.MetaScore.Count != 0)
            {
                Metascore = game.MetaScore.First().Value;

                if (game.MetacriticUrls.Count != 0)
                {
                    MetacriticUrl = game.MetacriticUrls.First().Value;
                }

            }

            if(game.DownloadSize.Count != 0)
            {
                DownloadSize = game.DownloadSize;
            }
        }

        private Visibility _isImageLoaded = Visibility.Visible;
        public Visibility IsImageLoaded
        {
            set
            {
                _isImageLoaded = value;
                NotifyPropertyChanged();
            }
            get
            {
                return _isImageLoaded;
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
                if (Metascore > 75)
                {
                    return Color.FromArgb(0xff, 0x74, 0xcb, 0x2c);
                }
                else if (Metascore > 50)
                {
                    return Color.FromArgb(0xff, 0xFB, 0xCC, 0x21);
                }
                else
                {
                    return Color.FromArgb(0xff, 0xF5, 0x16, 0x00);
                }
            }
        }

        public bool IsPosterCached { set; get; } = false;
        private async void LoadImage()
        {
            var httpClient = new Windows.Web.Http.HttpClient();

            try
            {
                var buffer = await httpClient.GetBufferAsync(new Uri(PosterUrl));

                if (!IsPosterCached)
                {
                    try
                    {
                        var file = await App.CacheFolder.CreateFileAsync(ID + ".jpg");

                        await FileIO.WriteBufferAsync(file, buffer);
                    }
                    catch (Exception exception)
                    {
                        switch ((uint)exception.HResult)
                        {
                            case 0x800700B7:
                                System.Diagnostics.Debug.WriteLine("文件已经存在了，应该是在被并行写入，直接抛错就行。");
                                break;
                        }
                    }
                }

                NotifyPropertyChanged("PosterPath");

            }
            catch(Exception exception)
            {
                System.Diagnostics.Debug.WriteLine("网络不好，接收不到海报");
            }
        }

        static public double MaxWidth
        {
            get
            {
                double maxWidth = 202.5;
                switch (AnalyticsInfo.VersionInfo.DeviceFamily)
                {
                    case "Windows.Xbox":
                        maxWidth = 150;
                        break;
                }
                return maxWidth;
            }
        }

        static public double TitleFontSize
        {
            get
            {
                double titleSize = 20;
                switch (AnalyticsInfo.VersionInfo.DeviceFamily)
                {
                    case "Windows.Xbox":
                        titleSize = 13;
                        break;
                }
                return titleSize;

            }
        }


        static public double ItemHeight
        {
            get
            {
                double maxWidth = 303.75;
                switch (AnalyticsInfo.VersionInfo.DeviceFamily)
                {
                    case "Windows.Xbox":
                        maxWidth = 225;
                        break;
                }
                return maxWidth;
            }
        }
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
