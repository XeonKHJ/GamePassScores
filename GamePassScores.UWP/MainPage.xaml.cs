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
                Games.Add(new GameViewModel(game));
            }
        }

        public ObservableCollection<GameViewModel> Games { get; set; } = new ObservableCollection<GameViewModel>();

        private void OrderByScoreAscendItem_Click(object sender, RoutedEventArgs e)
        {
            var games = Games.ToList();
            games = games.OrderBy(g => g.Metascore).ToList();

            Games.Clear();
            foreach(var g in games)
            {
                Games.Add(g);
            }
        }

        private void OrderByScoreDescendItem_Click(object sender, RoutedEventArgs e)
        {
            var games = Games.ToList();
            games = games.OrderByDescending(g => g.Metascore).ToList();

            Games.Clear();
            foreach (var g in games)
            {
                Games.Add(g);
            }
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
    }
}
