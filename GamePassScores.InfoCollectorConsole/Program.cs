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
using GamePassScores.InfoCollectorConsole.Config;

namespace GamePassScores.InfoCollectorConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            ConsoleGameDataFetcher consoleGameFetcher = new ConsoleGameDataFetcher(20);
            PCGameDataFetcher pcGameFetcher = new PCGameDataFetcher(20);
            GameScoreDataFetcher scoreDataFetcher = new GameScoreDataFetcher();
            string serializeGames = string.Empty;
            try
            {
                string arg = args[0];
                var options = await ArgParser.ParseJsonAsync(arg);
                IConfigBuilder consoleGamesConfigBuilder = new ConsoleGameConfigBuilder(arg);
                IConfigBuilder pcGameConfigBuilder = new PCGameConfigBuilder(arg);
                Console.WriteLine("There are {0} repos in the config files", options.RepoOptions.Count);

                if (args.Length == 2)
                {
                    if (args[1] == "debug")
                    {
                        serializeGames = System.IO.File.ReadAllText(options.OldConsoleGameInfoPath);
                    }
                }
                else
                {
                    List<Game> newConsoleGames = null;
                    List<Game> newPCGames = null;
                    if(options.OldConsoleGameInfoPath == null || options.OldConsoleGameInfoPath == string.Empty)
                    {

                    }
                    else
                    {
                        string oldConsoleGameInfo = options.OldConsoleGameInfoPath;
                        var jsonFile = File.ReadAllText(oldConsoleGameInfo);
                        List<Game> games = JsonConvert.DeserializeObject<List<Game>>(jsonFile);
                        //newConsoleGames = await consoleGameFetcher.GetGamesAsync(games);
                        //await scoreDataFetcher.FetchScoresAsync(newConsoleGames);

                        newPCGames = await pcGameFetcher.GetGamesAsync(new List<Game>());
                        await scoreDataFetcher.FetchScoresAsync(newPCGames);
                    }

                    // await consoleGamesConfigBuilder.SaveAsync(newConsoleGames);
                    await pcGameConfigBuilder.SaveAsync(newPCGames);
                    if(!options.NoCommit)
                    {
                        await consoleGamesConfigBuilder.PublishAsync();
                        await pcGameConfigBuilder.PublishAsync();
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
