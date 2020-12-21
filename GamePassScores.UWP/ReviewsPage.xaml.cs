using HtmlAgilityPack;
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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace GamePassScores.UWP
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ReviewsPage : Page
    {
        public ReviewsPage()
        {
            this.InitializeComponent();
        }

        GameViewModel Game { set; get; }
        ObservableCollection<ReviewViewModel> Reviews = new ObservableCollection<ReviewViewModel>();
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var gameViewModel = e.Parameter as GameViewModel;
            if(gameViewModel != null)
            {
                Game = gameViewModel;
                if (Game.IsPosterCached)
                {
                    var posterSource = new BitmapImage(new Uri(Game.PosterPath));
                    PosterView.Source = posterSource;
                    PosterImage.Source = posterSource;
                }
                GetReviewsFromMetacriticAsync(gameViewModel.MetacriticUrl.AbsoluteUri);
                MetascoreBlock.Text = Game.Metascore.ToString();
            }

        }

        private async void GetReviewsFromMetacriticAsync(string gamePageUrl)
        {
            var reviewsPageUrl = gamePageUrl + "critic-reviews/";
            Reviews.Clear();
            var httpClient = new HttpClient();

            try
            {
                var response = await httpClient.GetAsync(new Uri(reviewsPageUrl));

                if(response.IsSuccessStatusCode)
                {
                    var message = await response.Content.ReadAsStringAsync();

                    //查找class为review_content的DIV标签
                    HtmlDocument document = new HtmlDocument();
                    document.LoadHtml(message);
                    var matchSection = document.DocumentNode.SelectSingleNode(".//ol[@class='reviews critic_reviews']");
                    matchSection.SetParent(null);
                    var matchNodes = matchSection.SelectNodes(".//div[@class='review_content']");
                    //将每个节点都转换成ReviewViewModel
                    foreach (var matchNode in matchNodes)
                    {
                        var scoreNode = matchNode.SelectSingleNode(".//div[@class='review_grade']");
                        int score = int.Parse(scoreNode.InnerText);

                        var reviewNode = matchNode.SelectSingleNode(".//div[@class='review_body']");
                        string review = reviewNode.InnerText.Trim().Replace(System.Environment.NewLine, " ");

                        var sourceNode = matchNode.SelectSingleNode(".//div[@class='source']");
                        string source = sourceNode.InnerText.Trim();

                        //获取链接
                        var reviewActionNode = matchNode.SelectSingleNode(".//li[@class='review_action full_review']");
                        var href = reviewActionNode == null ? string.Empty : reviewActionNode.FirstChild.Attributes["href"].Value;
                        
                        //获取日期
                        var dateNode = matchNode.SelectSingleNode(".//div[@class='date']");

                        DateTime date = DateTime.MinValue;
                        if(dateNode != null)
                        {
                            date = DateTime.Parse(dateNode.InnerText.Trim());
                        }

                        ReviewViewModel reviewViewModel = new ReviewViewModel()
                        {
                            MediaName = source,
                            PublishDate = date,
                            Description = review,
                            Score = score,
                            Url = href
                        };
                        Reviews.Add(reviewViewModel);
                    }
                    
                }
                else
                {

                }
                LoadingReviewsRing.IsActive = false;
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine("网络不好，无法更新。");
                //_isRefreshing = false;
            }
        }

        private async void UpdateScores()
        {
            
        }
        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {

        }

        private async void ReviewsView_ItemClickAsync(object sender, ItemClickEventArgs e)
        {
            var review = e.ClickedItem as ReviewViewModel;
            if(review != null)
            {
                if (!string.IsNullOrEmpty(review.Url))
                {
                    await Launcher.LaunchUriAsync(new Uri(review.Url));
                }
            }

        }

        private void NavigationBackButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private async void Button_ClickAsync(object sender, RoutedEventArgs e)
        {
            if (Game != null)
            {
                if (Game.MetacriticUrl != null)
                {
                    await Launcher.LaunchUriAsync(Game.MetacriticUrl);
                }
            }
        }
    }
}
