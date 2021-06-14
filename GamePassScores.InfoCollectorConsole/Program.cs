using System;
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

namespace GamePassScores.InfoCollectorConsole
{
    class Program
    {
        static string consoleGameListInfoUrl = "https://catalog.gamepass.com/sigls/v2?id=f6f1f99f-9b49-4ccd-b3bf-4d9767a77f5e&language=en-us&market=US";
        static string pcGameListInfoUrl = "https://catalog.gamepass.com/sigls/v2?id=fdd9e2a7-0fee-49f6-ad69-4354098401ff&language=en-us&market=US";
        static string recentlyAddConsoleGameListInfo = "https://catalog.gamepass.com/sigls/v2?id=f13cf6b4-57e6-4459-89df-6aec18cf0538&language=en-us&market=US";
        static string leavingSoonConsoleGameListInfo = "https://catalog.gamepass.com/sigls/v2?id=393f05bf-e596-4ef6-9487-6d4fa0eab987&language=en-us&market=US";
        static string idAtXboxConsoleGameListInfo = "";
        static string repoLocalPath = "";
        static string repoUserName = "";
        static string repoPassword = "";
        static string oldGameInfoFiles = "";
        static string newGameInfoFileName = "";
        static async Task Main(string[] args)
        {
            Console.WriteLine("Recevie {0} args", args);

            for(int i = 0; i < args.Length; ++i)
            {
                Console.WriteLine("Arg {0}: {1}", i, args[i]);
            }
            // Get repo info.
            switch (args.Length)
            {
                case 0:
                    
                    break;
                case 1:
                    break;
                case 4:
                    break;
                case 5:
                    break;
                default:
                    break;
            }

            oldGameInfoFiles = args[0];
            newGameInfoFileName = args[1];
            repoLocalPath = args[2];
            repoUserName = args[3];
            repoPassword = args[4];

            //HttpClient.DefaultProxy = new WebProxy("127.0.0.1", 1080);
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

            #region 更新Metacritic
            //var jsonFile = System.IO.File.ReadAllText("./games.json");
            //var games = JsonConvert.DeserializeObject<List<Game>>(jsonFile);
            //var fileName = "newgames.json";
            //await UpdateMetacriticScoresAsync(games, Platform.XboxOne);
            #endregion

            #region 更新类别
            if (string.IsNullOrEmpty(oldGameInfoFiles))
            {
                oldGameInfoFiles = "./games.json";
            }
            var jsonFile = System.IO.File.ReadAllText(oldGameInfoFiles);
            var games = JsonConvert.DeserializeObject<List<Game>>(jsonFile);

            if (string.IsNullOrEmpty(newGameInfoFileName))
            {
                newGameInfoFileName = "newgames.json";
            }

            var fileName = newGameInfoFileName;
            var newGames = await UpdateGamesList(games);
            #endregion

            #region 获取类型列表
            //var gamelistInfo = await GetGameList(consoleGameListInfoUrl);
            //var gameInfos = await GetGamesInfo(gamelistInfo);

            //HashSet<string> genres = new HashSet<string>();
            //foreach(var game in gameInfos.Products)
            //{
            //    genres.Add(game.Properties.Category);
            //    if (game.Properties.Categories != null)
            //    {
            //        foreach (var c in game.Properties.Categories)
            //        {
            //            genres.Add(c);
            //        }
            //    }
            //}
            #endregion

            //序列化成底层数据模型
            var serializeGames = JsonConvert.SerializeObject(newGames);
            await System.IO.File.WriteAllTextAsync(fileName, serializeGames);
            UploadGameList(repoLocalPath, repoUserName, repoPassword);
        }

        static void UploadGameList(string repoLocalPath, string username, string password)
        {
            using (var repo = new Repository(repoLocalPath))
            {
                // Stage the file
                repo.Index.Add("ConsoleGames.json");
                repo.Index.Write();

                // Create the committer's signature and commit
                Signature author = new Signature("GameInfo Collectors", "dont@me.please", DateTime.Now);
                Signature committer = author;

                // Commit to the repository

                try
                {
                    Commit commit = repo.Commit("Update games' info.", author, committer);


                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }

                try
                {
                    PushOptions options = new PushOptions();

                    options.CredentialsProvider = new CredentialsHandler(
                        (url, usernameFromUrl, types) =>
                            new UsernamePasswordCredentials()
                            {
                                Username = username,
                                Password = password
                            });
                    repo.Network.Push(repo.Branches["GameInfos"], options);
                }
                catch(Exception ex)
                {
                    Console.WriteLine("ex.Message");
                    System.Diagnostics.Debug.WriteLine("Push fault:{0}", ex.Message);
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
        static int parallelNum = 5;
        static async Task<ProductsModel> GetGamesInfo(string[] gamelistInfo)
        {
            ProductsModel gamePassProductsModel = new ProductsModel();
            ProductsModel eaPlayProductsModel = new ProductsModel();
            await Task.Run(() =>
            {
                Semaphore semaphore = new Semaphore(parallelNum, parallelNum);
                #region 老老实实的并行请求
                for (int i = 0; i < gamelistInfo.Length; i = i + parallelNum)
                {
                    int requestNum = parallelNum;
                    if (i + parallelNum > gamelistInfo.Length)
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
                Console.WriteLine("发了一个请求");
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
            //await UpdateMetacriticScoresAsync(games);


            return newGames;
        }
    }
}
