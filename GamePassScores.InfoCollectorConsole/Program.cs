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
using System.IO;
using System.IO.Compression;
using GamePassScores.InfoCollectorConsole.RawModel;
using GamePassScores.InfoCollectorConsole.DataFetcher;
using GamePassScores.InfoCollectorConsole.AppException;

namespace GamePassScores.InfoCollectorConsole
{
    class Program
    {
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

                    await configBuilder.SaveAsync(newGames);
                    if(!options.NoCommit)
                    {
                        await configBuilder.PublishAsync();
                    }
                }

                
                // await UploadGameListAsync(options.RepoOptions, serializeGames);
            }
            catch (System.IO.DirectoryNotFoundException ex)
            {
                Console.Error.WriteLine("Fatal exception: {0}", ex.Message);
            }
            catch (FatalException exception)
            {
                Console.Error.WriteLine("Fatal exception: {0}", exception.Message);
            }
        }
    }
}
