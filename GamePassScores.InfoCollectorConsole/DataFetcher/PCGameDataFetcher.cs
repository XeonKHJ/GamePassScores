using GamePassScores.InfoCollectorConsole.AppException;
using GamePassScores.InfoCollectorConsole.RawModel;
using GamePassScores.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GamePassScores.InfoCollectorConsole.DataFetcher
{
    internal class PCGameDataFetcher
    {
        private static string consoleGameListInfoUrl = "https://catalog.gamepass.com/sigls/v2?id=f6f1f99f-9b49-4ccd-b3bf-4d9767a77f5e&language=en-us&market=US";
        private static string pcGameListInfoUrl = "https://catalog.gamepass.com/sigls/v2?id=fdd9e2a7-0fee-49f6-ad69-4354098401ff&language=en-us&market=US";
        private static string recentlyAddConsoleGameListInfo = "https://catalog.gamepass.com/sigls/v2?id=f13cf6b4-57e6-4459-89df-6aec18cf0538&language=en-us&market=US";
        private static string leavingSoonConsoleGameListInfo = "https://catalog.gamepass.com/sigls/v2?id=393f05bf-e596-4ef6-9487-6d4fa0eab987&language=en-us&market=US";

        private int _expectedParallelNum = 20;
        public PCGameDataFetcher(int expectedParallelNum = 20)
        {
            _expectedParallelNum = expectedParallelNum;
        }

        /// <summary>
        /// Fetch game datas
        /// </summary>
        /// <param name="existedGames">Optional. If the param is provided an non-null value, then the basic info in exsitedGames will not be changed.</param>
        /// <returns></returns>
        public async Task<List<Game>> GetGamesAsync(List<Game> existedGames = null)
        {
            var allGameList = await GetGameList(pcGameListInfoUrl);
            Console.WriteLine("All game list is collected.");
            var recentlyAddedList = await GetGameList(recentlyAddConsoleGameListInfo);
            Console.WriteLine("Recently-added list is collected.");
            var leavingSoonList = await GetGameList(leavingSoonConsoleGameListInfo);
            Console.WriteLine("Leaving-soon list is collected.");
            var infos = await FetchGameInfos(allGameList);

            Console.WriteLine("Collecting games' detail information...");
            List<Game> newGames = new List<Game>();
            var allGames = await RawModelsToGameModelsAsync(infos, recentlyAddedList, leavingSoonList);
            Console.WriteLine("Games' detail informations are collected.");
            // Keep existed games' basic info.
            if (existedGames != null)
            {
                for (int i = 0; i < allGames.Count; ++i)
                {
                    var game = allGames[i];
                    var oldGameIndex = existedGames.FindIndex(g => g.ID == game.ID);
                    if (oldGameIndex != -1)
                    {
                        var oldGame = existedGames[oldGameIndex];
                        game.MetaCriticPathName = oldGame.MetaCriticPathName;
                        game.IsMetacriticInfoExist = oldGame.IsMetacriticInfoCorrect;
                        //game.MetaScore = oldGame.MetaScore;
                        //game.MetacriticUrls = oldGame.MetacriticUrls;
                        game.IsMetacriticInfoCorrect = oldGame.IsMetacriticInfoCorrect;
                        game.OriginalPlatforms = oldGame.OriginalPlatforms;
                        existedGames[oldGameIndex] = game;
                        newGames.Add(game);
                    }
                    else
                    {
                        newGames.Add(game);
                    }
                }
            }
            else
            {
                newGames = allGames;
            }
            return newGames;
        }
        private static async Task<string[]> GetGameList(string requestUrl)
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
            return gameCodes.ToArray();
        }

        /// <summary>
        /// Convert raw data model into Game model.
        /// </summary>
        /// <param name="gameInfos">All games' raw models.</param>
        /// <param name="recentlyAddedList">RecentlyAdded product id array</param>
        /// <param name="leavingSoonList">LeavingSoon product id array</param>
        /// <returns>List of game models.</returns>
        private async Task<List<Game>> RawModelsToGameModelsAsync(ProductsModel gameInfos, string[] recentlyAddedList = null, string[] leavingSoonList = null)
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
                        var bundleProducts = await FetchGameInfos(bundleIds.ToArray());

                        long maxBytes = 0;
                        foreach (var bp in bundleProducts.Products)
                        {
                            var bytes = (long)bp.DisplaySkuAvailabilities.First().Sku.Properties.Packages.First().MaxDownloadSizeInBytes;
                            maxBytes = bytes > maxBytes ? bytes : maxBytes;
                        }

                        game.DownloadSize.Add("US", maxBytes);
                    }
                    catch (ArgumentNullException)
                    {
                        Console.Error.WriteLine("{0} bundle doesn't have size information.", game.Title.First().Value);
                    }
                    catch (InvalidOperationException)
                    {
                        Console.Error.WriteLine("{0} bundle doesn't have size information.", game.Title.First().Value);
                    }
                }

                game.MetaCriticPathName = GenerateMetacriticPathName(game.Title.First().Value);
                game.OriginalPlatforms.Add(Platform.PC);
                games.Add(game);
            }
            return games;
        }

        private async Task<ProductsModel> FetchGameInfos(string[] gamelistInfo)
        {
            ProductsModel gamePassProductsModel = new ProductsModel();
            ProductsModel eaPlayProductsModel = new ProductsModel();

            for (int i = 0; i < gamelistInfo.Length; i = i + _expectedParallelNum)
            {
                int requestNum = _expectedParallelNum;
                if (i + _expectedParallelNum > gamelistInfo.Length)
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
                await RequestGameInfosAsync(requestProductsString, gamePassProductsModel);
            }
            return gamePassProductsModel;
        }

        private static async Task RequestGameInfosAsync(string requestProductsString, ProductsModel gamePassProductsModel)
        {
            string requestUriString = "https://displaycatalog.mp.microsoft.com/v7.0/products?";

            requestUriString += "bigIds=" + requestProductsString + "&" +
                            "market=US&" +
                            "languages=en-us&" +
                            "MS-CV=DGU1mcuYo0WMMp+F.1";

            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(requestUriString));
            var httpClient = new HttpClient();

            try
            {
                var response = await httpClient.SendAsync(request).ConfigureAwait(false);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var productsModel = JsonConvert.DeserializeObject<ProductsModel>(responseString);

                    foreach (var p in productsModel.Products)
                    {
                        if (IsGamePassProduct(p) || IsEAPlayProduct(p))
                        {
                            lock (gamePassProductsModel)
                            {
                                gamePassProductsModel.Products.Add(p);
                            }
                        }
                    }
                }
                else
                {
                    throw new FatalException();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("An fatal occurs in function RequestGameInfosAsync(string, ProductModel).");
                Console.WriteLine(exception.Message);
                Console.WriteLine(exception.StackTrace);
                throw new FatalException();
            }
        }

        private static bool IsGamePassProduct(Product product)
        {
            bool isGamePassProduct = false;
            var localizeProperties = product.LocalizedProperties;
            if (localizeProperties != null)
            {
                foreach (var l in localizeProperties)
                {
                    if(l.EligibilityProperties == null)
                    {
                        return true;
                    }
                    else if (l.EligibilityProperties.Affirmations != null)
                    {
                        var fs = l.EligibilityProperties.Affirmations;
                        foreach (var f in fs)
                        {
                            if (f.AffirmationId == "9WNZS2ZC9L74" || f.AffirmationId == "9NC1XH2KD60Z" || f.AffirmationId == "9VP428G6BQ82")
                            {
                                isGamePassProduct = true;
                                break;
                            }
                        }
                    }
                }
            }
            return isGamePassProduct;
        }

        private static bool IsEAPlayProduct(Product product)
        {
            bool isEaPlayProduct = false;
            var localizeProperties = product.LocalizedProperties;
            if (localizeProperties != null)
            {
                foreach (var l in localizeProperties)
                {
                    if (l.EligibilityProperties.Affirmations != null)
                    {
                        var fs = l.EligibilityProperties.Affirmations;
                        foreach (var f in fs)
                        {
                            if (f.AffirmationId == "B0HFJ7PW900M")
                            {
                                isEaPlayProduct = true;
                                break;
                            }
                        }
                    }
                }
            }
            return isEaPlayProduct;
        }

        private static string GenerateMetacriticPathName(string gameTitle)
        {
            var metaCriticPathName = gameTitle.ToLower().ToCharArray();
            for (int i = 0; i < metaCriticPathName.Length; ++i)
            {
                var c = metaCriticPathName[i];
                string matchString = "0123456789qwertyuiopasdfghjklzxcvbnm-!'.";
                if (!matchString.Contains(c))
                {
                    metaCriticPathName[i] = ' ';
                }
            }

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

            return finalMetaCriticPathName;
        }
    }
}
