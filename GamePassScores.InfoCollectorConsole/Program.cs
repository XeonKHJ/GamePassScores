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
            Console.WriteLine("GamePass Scores Info Collector ver. {0}", AppInfo.Version);
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

                    string oldConsoleGameInfo = options.OldConsoleGameInfoPath;
                    List<Game> consoleGames = new List<Game>();
                    try
                    {
                        string jsonFile = File.ReadAllText(oldConsoleGameInfo);
                        consoleGames = JsonConvert.DeserializeObject<List<Game>>(jsonFile);
                    }
                    catch(FileNotFoundException ex)
                    {
                        Console.Error.WriteLine("Old console games' information file not found. New file will be created to save the new information.");
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine(ex.Message);
                        Console.Error.WriteLine("Unknown error occured when reading old console games' information file. New file will be created to save the new information.");
                    }

                    newConsoleGames = await consoleGameFetcher.GetGamesAsync(consoleGames);
                    await scoreDataFetcher.FetchScoresAsync(newConsoleGames);

                    List<Game> pcGames = new List<Game>();
                    try
                    {
                        string jsonFile = File.ReadAllText(options.OldPCGameInfoPath);
                        pcGames = JsonConvert.DeserializeObject<List<Game>>(jsonFile);
                    }
                    catch (System.IO.FileNotFoundException ex)
                    {
                        Console.Error.WriteLine("Unknown error occured when reading old console games' information file. New file will be created to save the new information.");
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine(ex.Message);
                    }

                    newPCGames = await pcGameFetcher.GetGamesAsync(pcGames);
                    await scoreDataFetcher.FetchScoresAsync(newPCGames);

                    await consoleGamesConfigBuilder.SaveAsync(newConsoleGames);
                    await pcGameConfigBuilder.SaveAsync(newPCGames);
                    if (!options.NoCommit)
                    {
                        await consoleGamesConfigBuilder.PublishAsync();
                        await pcGameConfigBuilder.PublishAsync();
                    }
                }
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
