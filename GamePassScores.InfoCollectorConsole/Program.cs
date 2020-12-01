using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using GamePassScores.Models;
using System.Text;

namespace GamePassScores.InfoCollectorConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            List<Game> games = new List<Game>();
            //获取游戏列表
            var gamelistInfo = await GetGameList();

            //从游戏列表中获取游戏详细信息
            var gameInfos = await GetGamesInfo(gamelistInfo);

            //转换成为我们的对象
            foreach(var gameInfo in gameInfos.Products)
            {
                var game = new Game();
                game.ID = gameInfo.ProductId;
                
                foreach(var l in gameInfo.LocalizedProperties)
                {
                    if (l.Language != null)
                    {
                        game.Title.Add(l.Language, l.ProductTitle);
                        game.Description.Add(l.Language, l.ProductDescription);
                    }

                    var es = l.EligibilityProperties;
                    if(es.Affirmations != null)
                    {
                        foreach(var a in es.Affirmations)
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
                }

                var metaCriticPathName = game.Title.First().Value.ToLower().ToCharArray();
                for(int i = 0; i < metaCriticPathName.Length; ++i)
                {
                    var c = metaCriticPathName[i];
                    string matchString = "01234567890qwertyuiopasdfghjklzxcvbnm";
                    if(!matchString.Contains(c))
                    {
                        metaCriticPathName[i] = ' ';
                    }
                }
                //去空格
                var newMetaCriticPathName = new string(metaCriticPathName);
                newMetaCriticPathName = newMetaCriticPathName.Trim();
                var newMetaCriticPathNameArray = newMetaCriticPathName.ToCharArray().ToList();
                bool isFollowingSpace = false;

                bool isSpaceDetected = false;
                for (int i = 0; i < newMetaCriticPathNameArray.Count; ++i)
                {
                    if(isSpaceDetected)
                    {
                        if(newMetaCriticPathNameArray[i] == ' ')
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
                var finalMetaCriticPathName = new string(newMetaCriticPathNameArray.ToArray());
                game.MetaCriticPathName = finalMetaCriticPathName;

                //修改平台
                if(gameInfo.Properties.PackageFamilyName.Contains("Xbox360BackwardCompatibil"))
                {
                    game.OriginalPlatforms.Add(Platform.Xbox360);
                }
                else
                {
                    game.OriginalPlatforms.Add(Platform.XboxOne);
                }

                string metacritcBaseUrl = "https://www.metacritic.com/game/";
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

            //打印一下
            foreach(var game in games)
            {
                Console.WriteLine(game.MetaCriticPathName);
            }

            //序列化成底层数据模型
        }

        static async Task<ProductsModel> GetGamesInfo(string[] gamelistInfo)
        {
            ProductsModel gamePassProductsModel = new ProductsModel();
            ProductsModel eaPlayProductsModel = new ProductsModel();
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
                                            if (f.AffirmationId == "9WNZS2ZC9L74")
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
                            if (isGamePassProduct)
                            {
                                gamePassProductsModel.Products.Add(p);
                            }
                            if (isEaPlayProduct)
                            {
                                gamePassProductsModel.Products.Add(p);
                            }
                        }
                        //Console.WriteLine(responseString);
                    }
                    else
                    {
                        Console.WriteLine("Fuck, 错了。");
                    }

                    //Console.WriteLine("{0} done!", gameCode);

                }
                catch (Exception exception)
                {
                    Console.WriteLine("fucksdkfjalsdf");
                }
            }
            #endregion
            return gamePassProductsModel;
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

        static async Task GetMetacriticScores(List<Game> games)
        {
            foreach(var game in games)
            {
                HttpClient httpClient = new HttpClient();
                HttpRequestMessage httpRequestMessage = new HttpRequestMessage();

            }
        }
    }
}
