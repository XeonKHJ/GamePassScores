using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace GamePassScores.UWP
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class GameDetailPage : Page
    {
        public GameDetailPage()
        {
            this.InitializeComponent();
        }

        string imageSource = string.Empty;
        GameViewModel game;
        ObservableCollection<ScreenshotViewModel> Screenshots { set; get; } = new ObservableCollection<ScreenshotViewModel>();
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            //先以某种方式隐藏截图列表
            ScreenshotsView.Height = 0;
            ScreenshotsView.Margin = new Thickness(0);

            game = (GameViewModel)e.Parameter;
            game.PropertyChanged += Game_PropertyChanged;

            if(game.IsPosterCached)
            {
                var posterSource = new BitmapImage(new Uri(game.PosterPath));
                PosterImage.Source = posterSource;
                PosterView.Source = posterSource;
            }
            TitleBlock.Text = game.Title;
            DescriptionBlock.Text = game.Description;
            ScoreGrid.Visibility = game.IsScoreAvaliable;
            ScoreGrid.Background = new SolidColorBrush(game.ScoreColor);
            ScoreBlock.Text = game.Metascore.ToString();
            ReleaseDateBlock.Text = string.Format("Release Date: {0}", game.ReleaseDate);

            
            Screenshots.Clear();
            foreach(var g in game.Screenshots)
            {
                Screenshots.Add(new ScreenshotViewModel { ScreenshotUrl = g});
            }

            if(game.DownloadSize.Count > 0)
            {
                SizeBlock.Visibility = Visibility.Visible;
                SizeBlock.Text = string.Format("Estimated Download Size: {0}GB", (((double)game.DownloadSize.First().Value) / 1024 / 1024 / 1024).ToString("0.##"));
            }
            else
            {
                SizeBlock.Visibility = Visibility.Collapsed;
            }

            if (game.Categories.Count > 0)
            {
                CategoriesBlock.Visibility = Visibility.Visible;
                string categoriesString = "Catagory: ";
                
                if(game.Categories.Count > 1)
                {
                    categoriesString = "Catagories: ";

                }

                foreach (var c in game.Categories)
                {
                    categoriesString += c + ", ";
                }
                //删掉最后的", "
                categoriesString = categoriesString.Remove(categoriesString.Length - 2);
                CategoriesBlock.Text = categoriesString;
            }
            else
            {
                CategoriesBlock.Visibility = Visibility.Collapsed;
            }

            var anim = ConnectedAnimationService.GetForCurrentView().GetAnimation("ForwardConnectedAnimation");
            if (anim != null)
            {
                anim.TryStart(PosterImage);
            }
        }

        private void Game_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "PosterPath")
            {
                var posterSource = new BitmapImage(new Uri(game.PosterPath));
                PosterImage.Source = posterSource;
                PosterView.Source = posterSource;
            }
        }

        private void NavigationBackButton_Click(object sender, RoutedEventArgs e)
        {
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("BackwardConnectedAnimation", PosterImage);
            Frame.GoBack();
        }

        private async void StoreButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            if(game != null)
            {
                string baseUri = "ms-windows-store://pdp/?ProductId=" + game.ID;
                Uri uri = new Uri(baseUri);
                await Launcher.LaunchUriAsync(uri);
            }

        }

        private async void ScoreButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            if(game.MetacriticUrl != null)
            {
                await Launcher.LaunchUriAsync(game.MetacriticUrl);
            }    
        }

        private void Image_ImageOpened(object sender, RoutedEventArgs e)
        {
            ScreenshotsView.Height = 200;
            ScreenshotsView.Margin = new Thickness(0);
            ScreenshotsView.Height = double.NaN;
        }
    }
}
