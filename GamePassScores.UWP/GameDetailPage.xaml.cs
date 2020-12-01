using System;
using System.Collections.Generic;
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
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            game = (GameViewModel)e.Parameter;
            var imageSource = ((GameViewModel)e.Parameter).PosterUrl;
            BitmapImage a = new BitmapImage(new Uri(imageSource));
            PosterImage.Source = a;
            PosterView.Source = a;
            TitleBlock.Text = game.Title;
            DescriptionBlock.Text = game.Description;
            ScoreGrid.Visibility = game.IsScoreAvaliable;
            ScoreGrid.Background = new SolidColorBrush(game.ScoreColor);
            ScoreBlock.Text = game.Metascore.ToString();
            var anim = ConnectedAnimationService.GetForCurrentView().GetAnimation("ForwardConnectedAnimation");
            if (anim != null)
            {
                anim.TryStart(PosterImage);
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
    }
}
