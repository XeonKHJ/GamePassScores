using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

                GetReviewsFromMetacriticAsync(gameViewModel.MetacriticUrl.AbsoluteUri);
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
                    var matchSection = document.DocumentNode.SelectSingleNode("//ol[@class='reviews critic_reviews']");
                    matchSection.SetParent(null);
                    var matchNodes = matchSection.SelectNodes("//div[@class='review_content']");
                    //将每个节点都转换成ReviewViewModel
                    foreach (var matchNode in matchNodes)
                    {
                        var scoreNode = matchNode.SelectSingleNode("//div[@class='review_grade']");
                        int score = int.Parse(scoreNode.InnerText);

                        var reviewNode = matchNode.SelectSingleNode("//div[@class='review_body']");
                        string review = reviewNode.InnerText.Trim();

                        var sourceNode = matchNode.SelectSingleNode("//div[@class='source']");
                        string source = sourceNode.InnerText.Trim();

                        //获取链接
                        var reviewActionNode = matchNode.SelectSingleNode("//li[@class='review_action full_review']");
                        var href = reviewActionNode.FirstChild.Attributes["href"].Value;

                        //获取日期
                        var dateNode = matchNode.SelectSingleNode("//div[@class='date']");
                        var date = DateTime.Parse(dateNode.InnerText.Trim());

                        ReviewViewModel reviewViewModel = new ReviewViewModel()
                        {
                            MediaName = source,
                            PublishDate = date,
                            Description = review,
                            Score = score,
                        };
                        Reviews.Add(reviewViewModel);
                    }
                    
                }
                else
                {

                }

            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine("网络不好，无法更新。");
                //_isRefreshing = false;
            }
        }
    }
}
