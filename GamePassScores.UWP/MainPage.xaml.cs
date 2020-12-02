﻿using GamePassScores.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Windows.Storage;
using System.Collections.ObjectModel;
using MUXC = Microsoft.UI.Xaml.Controls;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Windows.UI.Xaml.Media.Animation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace GamePassScores.UWP
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            //从json文件读取游戏信息
            ReadGamesFromJson();
        }

        private async void ReadGamesFromJson()
        {
            var jsonFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/games.json"));
            var jsonString = await FileIO.ReadTextAsync(jsonFile);
            var games = JsonConvert.DeserializeObject<List<Game>>(jsonString);
            foreach(var game in games)
            {
                GamesViewModel.Add(new GameViewModel(game));
                if (game.MetaScore.Count == 0)
                {
                    game.MetaScore.Add(Platform.Unknown, -1);
                }
            }
            Games = games;
        }

        public List<Game> Games = new List<Game>();
        public ObservableCollection<GameViewModel> GamesViewModel { get; set; } = new ObservableCollection<GameViewModel>();

        private void OrderByScoreAscendItem_Click(object sender, RoutedEventArgs e)
        {
            Games = Games.OrderBy(g => g.MetaScore.First().Value).ToList();
            //SearchBox_TextChanged
            //GamesViewModel.Clear();
            //foreach(var g in Games)
            //{
            //    GamesViewModel.Add(new GameViewModel(g));
            //}
            SearchBox_TextChanged(SearchBox, null);
        }

        private void OrderByScoreDescendItem_Click(object sender, RoutedEventArgs e)
        {
            Games = Games.OrderByDescending(g => g.MetaScore.First().Value).ToList();

            //GamesViewModel.Clear();
            //foreach (var g in Games)
            //{
            //    GamesViewModel.Add(new GameViewModel(g));
            //}
            SearchBox_TextChanged(SearchBox, null);
        }

        UIElement animatingElement;
        private void GamesView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var container = GamesView.ContainerFromItem(e.ClickedItem) as GridViewItem;
            if (container != null)
            {
                //find the image
                var root = (FrameworkElement)container.ContentTemplateRoot;
                var image = (UIElement)root.FindName("PosterImage");
                animatingElement = image;
                //prepare the animation
                ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ForwardConnectedAnimation", image);
            }

            //ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ForwardConnectedAnimation", sender);
            Frame.Navigate(typeof(GameDetailPage), e.ClickedItem);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if(animatingElement != null)
            {
                var anim = ConnectedAnimationService.GetForCurrentView().GetAnimation("BackwardConnectedAnimation");
                if (anim != null)
                {
                    anim.TryStart(animatingElement);
                }
                animatingElement = null;
            }
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            AboutTip.IsOpen = true;
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            //SearchBox.TextChanged += SearchBox_TextChanged;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox searchBlock = sender as TextBox;
            string text = searchBlock.Text;
            if(text.Trim() != string.Empty)
            {
                var games = (from g in Games
                            where g.Title.First().Value.ToLower().Contains(text.ToLower().Trim())
                            select new GameViewModel(g)).ToArray();
               

                bool isViewModelChanged = false;
                if(games.Length == GamesViewModel.Count)
                {
                    for(int i = 0; i < games.Length; ++i)
                    {
                        if(games[i].ID != GamesViewModel[i].ID)
                        {
                            isViewModelChanged = true;
                            break;
                        }
                    }
                }
                else
                {
                    isViewModelChanged = true;
                }

                if(isViewModelChanged)
                {
                    GamesViewModel.Clear();
                    foreach (var g in games)
                    {
                        GamesViewModel.Add(g);
                    }
                }

            }
            else
            {
                GamesViewModel.Clear();
                foreach (var game in Games)
                {
                    GamesViewModel.Add(new GameViewModel(game));
                }
            }
        }

        private void PosterImage_ImageOpened(object sender, RoutedEventArgs e)
        {
            Image image = sender as Image;

            if(image != null && image.Tag != null)
            {
                var match = (from g in GamesViewModel where g.ID == image.Tag.ToString() select g).ToList();
                if (match.Count != 0)
                {
                    match.First().IsImageLoaded = Visibility.Collapsed;
                }
            }
        }

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void OrderByNameAscendItem_Click(object sender, RoutedEventArgs e)
        {
            Games = Games.OrderBy(g => g.Title.First().Value).ToList();

            //GamesViewModel.Clear();
            //foreach (var g in Games)
            //{
            //    GamesViewModel.Add(new GameViewModel(g));
            //}
            SearchBox_TextChanged(SearchBox, null);
        }

        private void OrderByNameDescendItem_Click(object sender, RoutedEventArgs e)
        {
            Games = Games.OrderByDescending(g => g.Title.First().Value).ToList();

            //GamesViewModel.Clear();
            //foreach (var g in Games)
            //{
            //    GamesViewModel.Add(new GameViewModel(g));
            //}
            SearchBox_TextChanged(SearchBox, null);
        }
    }
}