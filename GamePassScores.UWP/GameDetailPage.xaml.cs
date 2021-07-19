using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.System.Profile;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Microsoft.UI.Xaml;

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
            ScreenshotsView.IsEnabled = false;

            //如果是Xbox版本，边界要小一点。
            switch (AnalyticsInfo.VersionInfo.DeviceFamily)
            {
                case "Windows.Xbox":
                    ContentViewer.Padding = new Thickness(30, 0, 30, 0);
                    NavigationBackButton.Visibility = Visibility.Collapsed;
                    break;
            }

            game = (GameViewModel)e.Parameter;
            game.PropertyChanged += Game_PropertyChanged;

            if (game.IsPosterCached)
            {
                var posterSource = new BitmapImage(new Uri(game.PosterPath));
                PosterImage.Source = posterSource;
                PosterView.Source = posterSource;
            }
            TitleBlock.Text = game.Title;
            DescriptionBlock.Text = game.Description;
            ScoreButton.Visibility = game.IsScoreAvaliable;
            ScoreGrid.Background = new SolidColorBrush(game.ScoreColor);
            ScoreBlock.Text = game.Metascore.ToString();
            ReleaseDateBlock.Text = string.Format("{0}: {1}", LocalizationResource.GetReleaseDateString(), game.ReleaseDate);


            Screenshots.Clear();
            foreach (var g in game.Screenshots)
            {
                Screenshots.Add(new ScreenshotViewModel { ScreenshotUrl = g });
            }

            if (game.DownloadSize.Count > 0)
            {
                SizeBlock.Visibility = Visibility.Visible;
                SizeBlock.Text = string.Format("{0}: {1}GB", LocalizationResource.GetEstimatedDownloadSizeString(),(((double)game.DownloadSize.First().Value) / 1024 / 1024 / 1024).ToString("0.##"));
            }
            else
            {
                SizeBlock.Visibility = Visibility.Collapsed;
            }

            if (game.Categories.Count > 0)
            {
                CategoriesBlock.Visibility = Visibility.Visible;
                string categoriesString = string.Format("{0}: ", LocalizationResource.GetCategoryString());

                if (game.Categories.Count > 1)
                {
                    categoriesString = string.Format( "{0}: ", LocalizationResource.GetCategoriesString());
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
            if (e.PropertyName == "PosterPath")
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
            if (game != null)
            {
                string baseUri = "ms-windows-store://pdp/?ProductId=" + game.ID;
                Uri uri = new Uri(baseUri);
                await Launcher.LaunchUriAsync(uri);
            }

        }

        private async void ScoreButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            //if(game.MetacriticUrl != null)
            //{
            //    await Launcher.LaunchUriAsync(game.MetacriticUrl);
            //}    

            //换成进入另一个页面
            this.Frame.Navigate(typeof(ReviewsPage), game);
        }

        private void Image_ImageOpened(object sender, RoutedEventArgs e)
        {
            ScreenshotsView.IsEnabled = true;

            switch (AnalyticsInfo.VersionInfo.DeviceFamily)
            {
                case "Windows.Xbox":
                    ScreenshotsView.MaxHeight = 200;
                    break;
                default:
                    ScreenshotsView.MaxHeight = 400;
                    break;
            }

            ScreenshotsView.Margin = new Thickness(0, 10, 10, 0);
            ScreenshotsView.Height = double.NaN;


            //for(int i = 0; i < Screenshots.Count; ++i)
            //{
            //    var viewModel = Screenshots[i];
            //    var uiElement = ScreenshotsView.ContainerFromItem(viewModel) as Grid;
            //    if(uiElement != null)
            //    {
            //        if(i == 0)
            //        {
            //            if(NavigationBackButton.Visibility == Visibility.Visible)
            //            {
            //                var viewBox = uiElement.Children.First() as Viewbox;
            //                var image = viewBox.Child as Image;
            //                viewBox.xy
            //            }
            //        }
            //    }
            //}
        }

        UIElement animatingElement;
        private void ScreenshotsView_ItemClick(object sender, ScreenshotViewModel e)
        {
            //var container = ScreenshotsView.ContainerFromItem(e.ClickedItem) as ListViewItem;
            //if (container != null)
            //{
            //    //find the image
            //    var root = (FrameworkElement)container.ContentTemplateRoot;
            //    var image = (UIElement)root.FindName("ScreenshotImage");
            //    animatingElement = image;
            //    //prepare the animation
            //    ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ScreenshotForwardConnectedAnimation", image);
            //}

            //ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ForwardConnectedAnimation", sender);
            this.Frame.Navigate(typeof(SelectedImagePage), new Tuple<ObservableCollection<ScreenshotViewModel>, ScreenshotViewModel>(Screenshots, e));
        }

        private void ScreenshotsView_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ScreenshotsView_ItemClick(Screenshots, ScreenshotsView.SelectedItem as ScreenshotViewModel);
        }

        private void ScreenshotsView_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case VirtualKey.Space:
                    ScreenshotsView_ItemClick(Screenshots, ScreenshotsView.SelectedItem as ScreenshotViewModel);
                    break;
            }

        }
    }
}
