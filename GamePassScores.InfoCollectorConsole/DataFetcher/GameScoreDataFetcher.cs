using GamePassScores.InfoCollectorConsole.AppException;
using GamePassScores.InfoCollectorConsole.RawModel;
using GamePassScores.Models;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GamePassScores.InfoCollectorConsole
{
    internal class GameScoreDataFetcher
    {
        // Need a stucture to identify the platform priority.
        private static string baseUrlString = "https://www.metacritic.com/game/";

        /// <summary>
        /// 代表用于获取数据时的优先级，如果
        /// </summary>
        List<Platform> priorities = new List<Platform> { Platform.Unknown, Platform.XboxSeriesX, Platform.XboxOne, Platform.PC };
        public async Task FetchScoresAsync(IList<Game> games)
        {
            foreach(var game in games)
            {
                await UpdateMetascoreByPrioritesAsync(game);
            }
        }

        
        static async Task GetMetacriticScoresAsync(List<Game> games)
        {
            Semaphore semaphore = new Semaphore(3, 3);
            await Task.Run(async () =>
            {
                foreach (var game in games)
                {
                    foreach (var platform in game.OriginalPlatforms)
                    {
                        string platformString = platform == Platform.Unknown ? "pc" : PlatformToMetacriticPathString(platform);
                        string gameString = game.MetaCriticPathName;
                        string requestUrlString = baseUrlString + platformString + "/" + gameString + "/";
                        await GetMetacriticScore(game, platform, requestUrlString, semaphore);
                    }
                }
                System.Diagnostics.Debug.WriteLine(totalC);
                while (abcde != 0) ;
                System.Diagnostics.Debug.WriteLine("Fuck yeah!");
                System.Diagnostics.Debug.WriteLine(totalC);
            });
        }
        static int abcde = 0;
        static int totalC = 0;
        static async Task GetMetacriticScore(Game game, Platform platform, string requestUrlString, Semaphore semaphore)
        {
            semaphore.WaitOne();
            abcde++;
            while (abcde > 2)
            {
                System.Diagnostics.Debug.WriteLine("abcde > 2");
            }
            Console.WriteLine("发送MTC请求");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrlString);
            HttpClient httpClient = new HttpClient();
            var response = await httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var responseContentString = await response.Content.ReadAsStringAsync();
                HtmlDocument document = new HtmlDocument();

                if (!string.IsNullOrEmpty(responseContentString))
                {
                    document.LoadHtml(responseContentString);
                    var matchNode = document.DocumentNode.SelectSingleNode("//script[@type='application/ld+json']");

                    var jsonText = matchNode.InnerText;
                    var metacriticScoreModel = JsonConvert.DeserializeObject<MetacriticScoreModel>(jsonText);

                    lock (game)
                    {
                        game.IsMetacriticInfoExist = true;
                        game.IsMetacriticInfoCorrect = true;
                        if (metacriticScoreModel.aggregateRating != null)
                        {
                            game.MetaScore[platform] = int.Parse(metacriticScoreModel.aggregateRating.ratingValue);
                            game.MetacriticUrls[platform] = new Uri(requestUrlString);
                        }
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("游戏的Metacritic信息正确，http回应却是空。");
                }
            }
            else
            {
                lock (game)
                {
                    game.IsMetacriticInfoCorrect = false;
                }

                switch (response.StatusCode)
                {
                    case HttpStatusCode.TooManyRequests:
                        System.Diagnostics.Debug.WriteLine("Meta Too Many Request!");
                        break;
                }
            }
            Console.WriteLine("结束MTC请求");

            lock (semaphore)
            {
                semaphore.Release();
            }

            totalC++;
            abcde--;
        }
        static async Task UpdateMetacriticScoresAsync(List<Game> games, Platform specifyPlatform = Platform.Unknown)
        {
            abcde = 0;

            Semaphore semaphore = new Semaphore(2, 2);
            string baseUrlString = "https://www.metacritic.com/game/";
            foreach (var game in games)
            {
                foreach (var platform in game.OriginalPlatforms)
                {
                    string platformString = string.Empty;
                    bool isPlaformSpecified = !(platform != Platform.Unknown && specifyPlatform == Platform.Unknown);
                    platformString = PlatformToMetacriticPathString(isPlaformSpecified ? specifyPlatform : platform);

                    string gameString = game.MetaCriticPathName;
                    string requestUrlString = baseUrlString + platformString + "/" + gameString + "/";

                    await GetMetacriticScore(game, platform, requestUrlString, semaphore);
                }
            }

            while (abcde != 0) ;

            Console.WriteLine("成功更新Metacritic信息。");
        }
        
        private async Task UpdateMetascoreByPrioritesAsync(Game game)
        {
            string gameString = game.MetaCriticPathName;
            
            foreach (var platform in priorities)
            {
                string platformString = PlatformToMetacriticPathString(platform);
                string requestUrlString = baseUrlString + platformString + "/" + gameString + "/";

                try
                {
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrlString);
                    HttpClient httpClient = new HttpClient();
                    var response = await httpClient.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContentString = await response.Content.ReadAsStringAsync();
                        HtmlDocument document = new HtmlDocument();

                        if (!string.IsNullOrEmpty(responseContentString))
                        {
                            document.LoadHtml(responseContentString);
                            var matchNode = document.DocumentNode.SelectSingleNode("//script[@type='application/ld+json']");

                            var jsonText = matchNode.InnerText;
                            var metacriticScoreModel = JsonConvert.DeserializeObject<MetacriticScoreModel>(jsonText);

                            lock (game)
                            {
                                game.IsMetacriticInfoExist = true;
                                game.IsMetacriticInfoCorrect = true;
                                if (metacriticScoreModel.aggregateRating != null)
                                {
                                    game.MetaScore[platform] = int.Parse(metacriticScoreModel.aggregateRating.ratingValue);
                                    game.MetacriticUrls[platform] = new Uri(requestUrlString);
                                }
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("游戏的Metacritic信息正确，http回应却是空。");
                            throw new ScoreNotFoundException();
                        }
                    }
                    else
                    {
                        lock (game)
                        {
                            game.IsMetacriticInfoCorrect = false;
                        }

                        switch (response.StatusCode)
                        {
                            case HttpStatusCode.TooManyRequests:
                                System.Diagnostics.Debug.WriteLine("Meta Too Many Request!");
                                break;
                            case HttpStatusCode.NotFound:
                                throw new MetacriticInfoIncorrectException();
                        }
                    }
                    Console.WriteLine("结束MTC请求");
                }
                catch (ScoreNotFoundException ex)
                {
                    // if error occurs, searching for the score for next platform.
                    continue;
                }
                catch (FatalException ex)
                {
                    throw ex;
                }

                // If score is fetched or MetacriticInfoIncorrectException is thrown, finishing the search by breaking the loop.
                break;
            }
        }

        private static string PlatformToMetacriticPathString(Platform platform)
        {
            string platformString = string.Empty;
            switch (platform)
            {
                case Platform.XboxOne:
                case Platform.XboxOneX:
                    platformString = "xbox-one";
                    break;
                case Platform.PC:
                    platformString = "pc";
                    break;
                case Platform.Xbox360:
                    platformString = "xbox-360";
                    break;
                case Platform.Xbox:
                    platformString = "xbox";
                    break;
                case Platform.XboxSeriesX:
                case Platform.XboxSeriesS:
                    platformString = "xbox-series-x";
                    break;
            }
            return platformString;
        }
    
    }
}
