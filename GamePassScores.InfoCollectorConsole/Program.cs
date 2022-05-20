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
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using System.IO;
using System.IO.Compression;
using GamePassScores.InfoCollectorConsole.RawModel;
using GamePassScores.InfoCollectorConsole.DataFetcher;

namespace GamePassScores.InfoCollectorConsole
{
    class Program
    {
        static string consoleGameListInfoUrl = "https://catalog.gamepass.com/sigls/v2?id=f6f1f99f-9b49-4ccd-b3bf-4d9767a77f5e&language=en-us&market=US";
        static string pcGameListInfoUrl = "https://catalog.gamepass.com/sigls/v2?id=fdd9e2a7-0fee-49f6-ad69-4354098401ff&language=en-us&market=US";
        static string recentlyAddConsoleGameListInfo = "https://catalog.gamepass.com/sigls/v2?id=f13cf6b4-57e6-4459-89df-6aec18cf0538&language=en-us&market=US";
        static string leavingSoonConsoleGameListInfo = "https://catalog.gamepass.com/sigls/v2?id=393f05bf-e596-4ef6-9487-6d4fa0eab987&language=en-us&market=US";
        static async Task Main(string[] args)
        {
            RawGameDataFetcher gameDataFetcher = new RawGameDataFetcher(20);
            GameScoreDataFetcher scoreDataFetcher = new GameScoreDataFetcher();
            string serializeGames = string.Empty;
            try
            {
                string arg = args[0];
                var options = await ArgParser.ParseJsonAsync(arg);
                IConfigBuilder configBuilder = new RegularConfigBuilder(arg);

                Console.WriteLine("There are {0} repos in the config files", options.RepoOptions.Count);

                if (args.Length == 2)
                {
                    if (args[1] == "debug")
                    {
                        serializeGames = System.IO.File.ReadAllText(options.OldInfoFilePath);
                    }
                }
                else
                {
                    List<Game> newGames = null;
                    if(options.OldInfoFilePath == null || options.OldInfoFilePath == string.Empty)
                    {

                    }
                    else
                    {
                        string oldGameInfoFile = options.OldInfoFilePath;
                        var jsonFile = File.ReadAllText(oldGameInfoFile);
                        List<Game> games = JsonConvert.DeserializeObject<List<Game>>(jsonFile);
                        newGames = await gameDataFetcher.GetGamesAsync(games);
                        await scoreDataFetcher.FetchScoresAsync(newGames);
                    }

                    await configBuilder.SaveAndPublishAsync(newGames);
                }

                
                // await UploadGameListAsync(options.RepoOptions, serializeGames);
            }
            catch (Exception exception)
            {
                Console.WriteLine("Exception! {0}", exception.Message);
            }
        }

        static async Task UploadGameListAsync(List<RepoOption> repoOptions, string fileContent)
        {
            foreach (var repoOption in repoOptions)
            {
                Console.WriteLine("Start processing repo.");
                var newInfoFilePath = Path.Combine(repoOption.RepoPath, repoOption.NewInfoFilePath);
                var newCompressedFilePath = Path.Combine(repoOption.RepoPath, repoOption.NewCompressedInfoFilePath);

                Console.WriteLine("New info file path is {0}", newInfoFilePath);
                Console.WriteLine("New compressed info file path is {0}", newCompressedFilePath);

                await File.WriteAllTextAsync(newInfoFilePath, fileContent);
                var compressedSerializeGames = Zip(fileContent);
                await File.WriteAllBytesAsync(newCompressedFilePath, compressedSerializeGames);

                Console.WriteLine("Files saved.");

                using (var repo = new Repository(repoOption.RepoPath))
                {
                    // Stage the file
                    repo.Index.Add(repoOption.NewInfoFilePath);
                    repo.Index.Add(repoOption.NewCompressedInfoFilePath);
                    repo.Index.Write();

                    // Create the committer's signature and commit
                    Signature author = new("GameInfo Collectors", "dont@me.please", DateTime.Now);
                    Signature committer = author;

                    // Commit to the repository
                    try
                    {
                        Commit commit = repo.Commit("Update games' info.", author, committer);

                        Console.WriteLine("Files commited.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                    }

                    try
                    {
                        PushOptions options = new PushOptions();


                        options.CredentialsProvider = new CredentialsHandler(
                            (url, usernameFromUrl, types) =>
                                new DefaultCredentials()
                                );


                        Console.WriteLine("Pushing repo {0}...", repoOption.RepoPath);

                        // This part requires Git installed.
                        Console.WriteLine("Executing command: git -C \"{0}\" push", repoOption.RepoPath);
                        System.Diagnostics.Process.Start("git", string.Format("-C \"{0}\" push", repoOption.RepoPath));
                        Console.WriteLine("Repo {0} is pushed.", repoOption.RepoPath);

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        System.Diagnostics.Debug.WriteLine("Push failed:{0}", ex.Message);
                    }
                }
            }

        }

        static async Task<List<Game>> ConvertToGames(ProductsModel gameInfos, string[] recentlyAddedList = null, string[] leavingSoonList = null)
        {
            List<Game> games = new List<Game>();
            foreach (var gameInfo in gameInfos.Products)
            {
                var game = new Game();
                game.ID = gameInfo.ProductId;

                game.Categories.Add(gameInfo.Properties.Category);
                if (gameInfo.Properties.Categories != null)
                {
                    foreach (var c in gameInfo.Properties.Categories)
                    {
                        game.Categories.Add(c);
                    }
                }

                if (recentlyAddedList != null)
                {
                    if (recentlyAddedList.Contains(game.ID))
                    {
                        game.InVaultTime = InVaultTime.RecentlyAdded;
                    }
                }

                if (leavingSoonList != null)
                {
                    if (leavingSoonList.Contains(game.ID))
                    {
                        game.InVaultTime = InVaultTime.LeavingSoon;
                    }
                }

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
                    game.ScreenShots = (from i in l.Images where (i.ImagePurpose.ToLower() == "screenshot") && (!i.ImagePositionInfo.ToLower().Contains("desktop")) select ("https:" + i.Uri)).ToList();
                    game.PosterUrl = "https:" + (from i in l.Images where i.ImagePurpose.ToLower() == "poster" select i.Uri).First();
                }

                //添加游戏发布日期
                game.ReleaseDate = gameInfo.MarketProperties.First().OriginalReleaseDate.ToBinary();

                var skuProperties = gameInfo.DisplaySkuAvailabilities.First().Sku.Properties;

                if (skuProperties.Packages.Count > 0)
                {
                    game.DownloadSize.Add("US", (long)gameInfo.DisplaySkuAvailabilities.First().Sku.Properties.Packages.First().MaxDownloadSizeInBytes);
                }
                else
                {
                    try
                    {
                        var bundleSkus = gameInfo.DisplaySkuAvailabilities.First().Sku.Properties.BundledSkus;
                        var bundleIds = from b in bundleSkus select b.BigId;
                        var bundleProducts = await GetGamesInfo(bundleIds.ToArray());

                        long maxBytes = 0;
                        foreach (var bp in bundleProducts.Products)
                        {
                            var bytes = (long)bp.DisplaySkuAvailabilities.First().Sku.Properties.Packages.First().MaxDownloadSizeInBytes;
                            maxBytes = bytes > maxBytes ? bytes : maxBytes;
                        }

                        game.DownloadSize.Add("US", maxBytes);
                    }
                    catch
                    {
                        Console.WriteLine("这个捆绑包没大小");
                        System.Diagnostics.Debug.WriteLine("这个捆绑包没大小");
                    }
                }

                var metaCriticPathName = game.Title.First().Value.ToLower().ToCharArray();
                for (int i = 0; i < metaCriticPathName.Length; ++i)
                {
                    var c = metaCriticPathName[i];
                    string matchString = "01234567890qwertyuiopasdfghjklzxcvbnm-!'";
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
                while (newMetaCriticPathNameArray.Remove('\'')) ;

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
            Console.WriteLine("成功转换成老子的模型。");
            return games;
        }


        static int totalGameFetch = 0;
        static int expectedParallelNum = 20;
        static int expectedThreadNum = 10;
        static async Task<ProductsModel> GetGamesInfo(string[] gamelistInfo)
        {
            ProductsModel gamePassProductsModel = new ProductsModel();
            ProductsModel eaPlayProductsModel = new ProductsModel();
            await Task.Run(() =>
            {
                Semaphore semaphore = new Semaphore(expectedThreadNum, expectedThreadNum);
                #region 老老实实的并行请求
                for (int i = 0; i < gamelistInfo.Length; i = i + expectedParallelNum)
                {
                    int requestNum = expectedParallelNum;
                    if (i + expectedParallelNum > gamelistInfo.Length)
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

            Console.WriteLine("成功更新游戏信息。");

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

            try
            {
                Console.WriteLine("正在请求产品{0}的详细信息。", requestProductsString);
                var response = await httpClient.SendAsync(request).ConfigureAwait(false);
                Console.WriteLine("请求{0}完成。", requestProductsString);

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
                    Console.WriteLine("结束了一个请求");
                }
                else
                {
                    Console.WriteLine("Fuck, 错了。");
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("Program.cs GetGamesInfoSingleTime");
                Console.WriteLine(exception.Message);
                Console.WriteLine(exception.StackTrace);
            }

            Console.WriteLine("请求{0}线程完成。", requestProductsString);
            var semaResult = semaphore.Release();
            --totalThread;
            //Console.WriteLine("{0} done!", gameCode);
        }
        static async Task<string[]> GetGameList(string requestUrl)
        {
            HttpClient httpClient = new HttpClient();
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUrl);
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

            Console.WriteLine("成功获取游戏列表。");
            return gameCodes.ToArray();
        }

        static async Task GetMetacriticScoresAsync(List<Game> games)
        {
            Semaphore semaphore = new Semaphore(3, 3);
            await Task.Run(async () =>
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

                    await GetMetacriticScore(game, platform, requestUrlString, semaphore);
                }
            }

            while (abcde != 0) ;

            Console.WriteLine("成功更新Metacritic信息。");
        }

        static async Task<List<Game>> UpdateGamesList(List<Game> games)
        {
            var gamelistInfo = await GetGameList(consoleGameListInfoUrl);
            var recentlyAddedList = await GetGameList(recentlyAddConsoleGameListInfo);
            var leavingSoonList = await GetGameList(leavingSoonConsoleGameListInfo);

            Console.WriteLine("开始更新游戏信息");
            var infos = await GetGamesInfo(gamelistInfo);

            List<Game> newGames = new List<Game>();
            var allGames = await ConvertToGames(infos, recentlyAddedList, leavingSoonList);

            for (int i = 0; i < allGames.Count; ++i)
            {
                var game = allGames[i];
                var oldGameIndex = games.FindIndex(g => g.ID == game.ID);
                if (oldGameIndex != -1)
                {
                    var oldGame = games[oldGameIndex];
                    game.MetaCriticPathName = oldGame.MetaCriticPathName;
                    game.IsMetacriticInfoExist = oldGame.IsMetacriticInfoCorrect;
                    game.MetaScore = oldGame.MetaScore;
                    game.MetacriticUrls = oldGame.MetacriticUrls;
                    game.IsMetacriticInfoCorrect = oldGame.IsMetacriticInfoCorrect;
                    game.OriginalPlatforms = oldGame.OriginalPlatforms;
                    games[oldGameIndex] = game;
                    newGames.Add(game);
                }
                else
                {
                    newGames.Add(game);
                }
            }

            await UpdateMetacriticScoresAsync(newGames, Platform.PC);
            await UpdateMetacriticScoresAsync(newGames);
            await UpdateMetacriticScoresAsync(newGames, Platform.XboxSeriesX);

            return newGames;
        }

        static void HandleError(Exception exception)
        {
            Console.WriteLine("Error occurd!");
            Console.WriteLine(exception);
            Console.WriteLine();
        }
    }
}
