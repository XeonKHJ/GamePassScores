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
        List<Platform> _platformPriorities = new List<Platform> { Platform.Original, Platform.XboxSeriesX, Platform.XboxOne, Platform.Xbox360, Platform.Xbox, Platform.PC, Platform.PS5, Platform.PS4, Platform.PS3, Platform.PS2, Platform.PS1, Platform.Switch };
        public async Task FetchScoresAsync(IList<Game> games)
        {
            foreach(var game in games)
            {
                await UpdateMetascoreByPrioritesAsync(game);
            }
        }        
        private async Task UpdateMetascoreByPrioritesAsync(Game game)
        {
            string gameString = game.MetaCriticPathName;
            
            foreach (var platform in _platformPriorities)
            {
                int retryCount = 0;
                string platformString = PlatformToMetacriticPathString(platform, game);
                string requestUrlString = baseUrlString + platformString + "/" + gameString;
                try
                {
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrlString);
                    request.Headers.Add("User-Agent", "Edg/110.0.1587.50");
                    HttpClient httpClient = new HttpClient();
                    Console.WriteLine("Collecting {0} ({1}) score on {2}", game.Title.First().Value, game.MetaCriticPathName, platform == Platform.Original ? game.OriginalPlatforms.First().ToString() : platform.ToString());
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
                                    game.MetaScore[platform == Platform.Original ? game.OriginalPlatforms.First() : platform] = int.Parse(metacriticScoreModel.aggregateRating.ratingValue);
                                    game.MetacriticUrls[platform == Platform.Original ? game.OriginalPlatforms.First() : platform] = new Uri(requestUrlString);
                                    game.MetacriticPlatform = platformString;

                                    if (platform == Platform.XboxSeriesX || platform == Platform.XboxSeriesS || platform == Platform.XboxOne || platform == Platform.Xbox360 || platform == Platform.Xbox)
                                    {
                                        game.OriginalPlatforms[0] = platform;
                                    }
                                }
                                else
                                {
                                    Console.Error.WriteLine("{0} ({1})'s metacritic information on {2} is correct but the score is unavailable.", game.Title.First().Value, game.MetaCriticPathName, platform == Platform.Original ? game.OriginalPlatforms.First() : platform);
                                    throw new ScoreNotFoundException();
                                }
                            }
                            Console.WriteLine("{0} ({1}) score on {2} is collected.", game.Title.First().Value, game.MetaCriticPathName, platform == Platform.Original ? game.OriginalPlatforms.First() : platform);
                        }
                        else
                        {
                            Console.Error.WriteLine("{0} ({1})'s metacritic information on {2} is correct but the http response is empty.", game.Title.First().Value, game.MetaCriticPathName, platform == Platform.Original ? game.OriginalPlatforms.First() : platform);
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
                                throw new FatalException("Too many request. Lower the number of requests are send at the same time.");
                            case HttpStatusCode.NotFound:
                                Console.Error.WriteLine("{0}'s metacritic information on {1} is not found.", game.Title.First().Value, platform == Platform.Original ? game.OriginalPlatforms.First() : platform);
                                throw new MetacriticInfoIncorrectException();
                        }
                    }
                }
                catch (ScoreNotFoundException)
                {
                    // if error occurs, searching for the score for next platform.
                    continue;
                }
                catch(MetacriticInfoIncorrectException)
                {
                    continue;
                }
                catch (FatalException ex)
                {
                    throw ex;
                }
                catch(HttpRequestException)
                {
                    throw new FatalException("Network error. Please try again");
                }

                // If score is fetched or MetacriticInfoIncorrectException is thrown, finishing the search by breaking the loop.
                break;
            }
        }

        private static string PlatformToMetacriticPathString(Platform platform, Game game = null)
        {
            if(platform == Platform.Original && game != null)
            {
                platform = game.OriginalPlatforms.First();
            }
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
                case Platform.PS1:
                    platformString = "ps";
                    break;
                case Platform.PS2:
                    platformString = "playstation-2";
                    break;
                case Platform.PS3:
                    platformString = "ps3";
                    break;
                case Platform.PS4:
                    platformString = "playstation-4";
                    break;
                case Platform.PS5:
                    platformString = "playstation-5";
                    break;
                case Platform.Switch:
                    platformString = "switch";
                    break;
            }

            if(platformString == string.Empty)
            {
                throw new FatalException(String.Format("No platform specified in PlatformToMetacriticPathString(Platform, Game)! The desinated platform is {0}", platform.ToString()));
            }

            return platformString;
        }
    
    }
}
