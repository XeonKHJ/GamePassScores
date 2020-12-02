﻿using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using GamePassScores.Models;
using System.Text;
using HtmlAgilityPack;
using System.Threading;
using System.Net;

namespace GamePassScores.InfoCollectorConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            #region 获取
            ////获取游戏列表
            //var gamelistInfo = await GetGameList();

            ////从游戏列表中获取游戏详细信息
            //var gameInfos = await GetGamesInfo(gamelistInfo);

            ////转换成为我们的对象
            //var games = ConvertToGames(gameInfos);

            ////获取Metascore
            //await GetMetacriticScoresAsync(games);

            ////打印一下
            //foreach (var game in games)
            //{
            //    Console.WriteLine(game.MetaCriticPathName);
            //}
            #endregion

            #region 更新
            //var jsonFile = System.IO.File.ReadAllText("./games.json");
            //var games = JsonConvert.DeserializeObject<List<Game>>(jsonFile);
            //var fileName = "newgames.json";
            //await UpdateMetacriticScoresAsync(games, Platform.PC);
            #endregion

            #region 更新类别
            var jsonFile = System.IO.File.ReadAllText("./games.json");
            var games = JsonConvert.DeserializeObject<List<Game>>(jsonFile);
            var fileName = "newgames.json";
            await UpdateMetacriticScoresAsync(games, Platform.PC);
            #endregion


            //序列化成底层数据模型
            var serializeGames = JsonConvert.SerializeObject(games);
            await System.IO.File.WriteAllTextAsync("./" + fileName, serializeGames);
        }

        static List<Game> ConvertToGames(ProductsModel gameInfos)
        {
            List<Game> games = new List<Game>();
            foreach (var gameInfo in gameInfos.Products)
            {
                var game = new Game();
                game.ID = gameInfo.ProductId;

                foreach (var l in gameInfo.LocalizedProperties)
                {
                    if (l.Language != null)
                    {
                        game.Title.Add(l.Language, l.ProductTitle);
                        game.Description.Add(l.Language, l.ProductDescription);
                    }

                    var es = l.EligibilityProperties;
                    if (es.Affirmations != null)
                    {
                        foreach (var a in es.Affirmations)
                        {
                            switch (a.AffirmationId)
                            {
                                case BasicInfo.EAPlayID:
                                    game.Affirmations.Add(SubscriptionServices.EAPlay);
                                    break;
                                case BasicInfo.GamePassID:
                                    game.Affirmations.Add(SubscriptionServices.XboxGamePass);
                                    break;
                            }
                        }
                    }

                    game.PosterUrl = "https:" + (from i in l.Images where i.ImagePurpose.ToLower() == "poster" select i.Uri).First();
                }

                var metaCriticPathName = game.Title.First().Value.ToLower().ToCharArray();
                for (int i = 0; i < metaCriticPathName.Length; ++i)
                {
                    var c = metaCriticPathName[i];
                    string matchString = "01234567890qwertyuiopasdfghjklzxcvbnm'";
                    if (!matchString.Contains(c))
                    {
                        metaCriticPathName[i] = ' ';
                    }
                }
                //去空格
                var newMetaCriticPathName = new string(metaCriticPathName);
                newMetaCriticPathName = newMetaCriticPathName.Trim();
                var newMetaCriticPathNameArray = newMetaCriticPathName.ToCharArray().ToList();

                bool isSpaceDetected = false;
                for (int i = 0; i < newMetaCriticPathNameArray.Count; ++i)
                {
                    if (isSpaceDetected)
                    {
                        if (newMetaCriticPathNameArray[i] == ' ')
                        {
                            newMetaCriticPathNameArray.RemoveAt(i--);
                        }
                        else
                        {
                            isSpaceDetected = false;
                        }
                    }
                    else
                    {
                        if (newMetaCriticPathNameArray[i] == ' ')
                        {
                            isSpaceDetected = true;
                            newMetaCriticPathNameArray[i] = '-';
                        }
                    }
                }
                while (newMetaCriticPathNameArray.Remove('\''));

                var finalMetaCriticPathName = new string(newMetaCriticPathNameArray.ToArray());
                game.MetaCriticPathName = finalMetaCriticPathName;

                //修改平台
                if (gameInfo.Properties.PackageFamilyName.Contains("Xbox360BackwardCompatibil"))
                {
                    game.OriginalPlatforms.Add(Platform.Xbox360);
                }
                else
                {
                    game.OriginalPlatforms.Add(Platform.XboxOne);
                }

                switch (game.OriginalPlatforms.First())
                {
                    case Platform.Xbox360:
                        {
                            string metainfo = "xbox-360/" + game.MetaCriticPathName;
                        }
                        break;
                    case Platform.XboxOne:
                        {
                            string metainfo = "xbox-one/" + game.MetaCriticPathName;
                        }
                        break;
                }


                games.Add(game);
            }

            return games;
        }


        static int totalGameFetch = 0;
        static async Task<ProductsModel> GetGamesInfo(string[] gamelistInfo)
        {
            ProductsModel gamePassProductsModel = new ProductsModel();
            ProductsModel eaPlayProductsModel = new ProductsModel();
            await Task.Run(() =>
            {
                Semaphore semaphore = new Semaphore(20, 20);
                #region 老老实实的并行请求
                for (int i = 0; i < gamelistInfo.Length; i = i + 20)
                {
                    int requestNum = 20;
                    if (i + 20 > gamelistInfo.Length)
                    {
                        requestNum = gamelistInfo.Length - i;
                    }
                    string requestProductsString = string.Empty;
                    for (int j = 0; j < requestNum; ++j)
                    {
                        requestProductsString += gamelistInfo[j + i];
                        if (j != requestNum - 1)
                        {
                            requestProductsString += ',';
                        }
                    }

                    GetGamesInfoSingleTime(requestProductsString, gamePassProductsModel, semaphore);
                }

                while (totalThread != 0) ;
            });

            int[] compareResult = new int[gamelistInfo.Length];
            List<int> stupidResult = new List<int>();
            for (int i = 0; i < gamelistInfo.Length; ++i)
            {
                foreach (var product in gamePassProductsModel.Products)
                {
                    if (gamelistInfo[i].ToLower() == product.ProductId.ToLower())
                    {
                        compareResult[i] = compareResult[i] + 1;
                    }
                }
            }

            for (int i = 0; i < compareResult.Length; ++i)
            {
                if (compareResult[i] != 1)
                {
                    stupidResult.Add(i);
                }
            }
            #endregion
            return gamePassProductsModel;
        }
        private static int totalThread = 0;
        static async void GetGamesInfoSingleTime(string requestProductsString, ProductsModel gamePassProductsModel, Semaphore semaphore)
        {
            semaphore.WaitOne();
            ++totalThread;
            string requestUriString = "https://displaycatalog.mp.microsoft.com/v7.0/products?";

            requestUriString += "bigIds=" + requestProductsString + "&" +
                            "market=US&" +
                            "languages=en-us&" +
                            "MS-CV=DGU1mcuYo0WMMp+F.1";

            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(requestUriString));
            var httpClient = new HttpClient();
            var response = await httpClient.SendAsync(request).ConfigureAwait(false);


            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var productsModel = JsonConvert.DeserializeObject<ProductsModel>(responseString);


                foreach (var p in productsModel.Products)
                {
                    bool isGamePassProduct = false;
                    bool isEaPlayProduct = false;
                    var localizeProperties = p.LocalizedProperties;
                    if (p.LocalizedProperties != null)
                    {
                        foreach (var l in localizeProperties)
                        {
                            if (l.EligibilityProperties.Affirmations != null)
                            {
                                var fs = l.EligibilityProperties.Affirmations;
                                foreach (var f in fs)
                                {
                                    if (f.AffirmationId == "9WNZS2ZC9L74" || f.AffirmationId == "9NC1XH2KD60Z" || f.AffirmationId == "9VP428G6BQ82")
                                    {
                                        isGamePassProduct = true;
                                    }
                                    if (f.AffirmationId == "B0HFJ7PW900M")
                                    {
                                        isEaPlayProduct = true;
                                    }
                                }
                            }

                            if (isGamePassProduct || isEaPlayProduct)
                            {
                                break;
                            }
                        }
                    }
                    lock (gamePassProductsModel)
                    {
                        if (isGamePassProduct || isEaPlayProduct)
                        {
                            gamePassProductsModel.Products.Add(p);
                        }
                    }
                }
                //Console.WriteLine(responseString);
            }
            else
            {
                Console.WriteLine("Fuck, 错了。");
            }

            var semaResult = semaphore.Release();
            --totalThread;
            //Console.WriteLine("{0} done!", gameCode);
        }
        static async Task<string[]> GetGameList()
        {
            string consoleGameListInfoUrl = "https://catalog.gamepass.com/sigls/v2?id=f6f1f99f-9b49-4ccd-b3bf-4d9767a77f5e&language=en-us&market=US";
            string pcGameListInfoUrl = "https://catalog.gamepass.com/sigls/v2?id=fdd9e2a7-0fee-49f6-ad69-4354098401ff&language=en-us&market=US";

            HttpClient httpClient = new HttpClient();
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, consoleGameListInfoUrl);
            var response = await httpClient.SendAsync(requestMessage);

            JArray gameListInfoJArray = null;
            List<string> gameCodes = new List<string>();
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                gameListInfoJArray = JsonConvert.DeserializeObject<JArray>(content);

                for (int i = 0; i < gameListInfoJArray.Count; ++i)
                {
                    if (gameListInfoJArray[i]["id"] != null)
                    {
                        gameCodes.Add(gameListInfoJArray[i]["id"].ToString());
                    }
                }
            }
            gameCodes = gameCodes.Distinct().ToList();

            return gameCodes.ToArray();
        }

        static async Task GetMetacriticScoresAsync(List<Game> games)
        {
            Semaphore semaphore = new Semaphore(3, 3);
            await Task.Run(() =>
            {
                foreach (var game in games)
                {

                    string baseUrlString = "https://www.metacritic.com/game/";


                    foreach (var platform in game.OriginalPlatforms)
                    {
                        string platformString = string.Empty;
                        if (platform != Platform.Unknown)
                        {
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
                        }

                        string gameString = game.MetaCriticPathName;
                        string requestUrlString = baseUrlString + platformString + "/" + gameString + "/";

                        GetMetacriticScore(game, platform, requestUrlString, semaphore);
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
        static async void GetMetacriticScore(Game game, Platform platform, string requestUrlString, Semaphore semaphore)
        {
            semaphore.WaitOne();
            abcde++;


            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrlString);
            HttpClient httpClient = new HttpClient();
            var response = await httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var responseContentString = await response.Content.ReadAsStringAsync();
                HtmlDocument document = new HtmlDocument();
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
                        game.MetaScore.Add(platform, int.Parse(metacriticScoreModel.aggregateRating.ratingValue));
                        game.MetacriticUrls.Add(platform, new Uri(requestUrlString));
                    }
                }
            }
            else
            {
                lock (game)
                {
                    game.IsMetacriticInfoCorrect = false;
                }

                switch(response.StatusCode)
                {
                    case HttpStatusCode.TooManyRequests:
                        System.Diagnostics.Debug.WriteLine("Meta Too Many Request!");
                    break;
                }
            }


            semaphore.Release();
            totalC++;
            abcde--;
        }

        static async Task UpdateMetacriticScoresAsync(List<Game> games, Platform specifyPlatform = Platform.Unknown)
        {
            await Task.Run(() =>
            {
                Semaphore semaphore = new Semaphore(2, 2);
                string baseUrlString = "https://www.metacritic.com/game/";
                foreach (var game in games)
                {
                    if (game.MetaScore.Count == 0)
                    {
                        foreach (var platform in game.OriginalPlatforms)
                        {
                            string platformString = string.Empty;
                            if (platform != Platform.Unknown && specifyPlatform == Platform.Unknown)
                            {
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
                            }
                            else
                            {
                                switch (specifyPlatform)
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
                            }

                            string gameString = game.MetaCriticPathName;
                            string requestUrlString = baseUrlString + platformString + "/" + gameString + "/";

                            GetMetacriticScore(game, platform, requestUrlString, semaphore);
                        }
                    }
                }
            });

        }
    }
}