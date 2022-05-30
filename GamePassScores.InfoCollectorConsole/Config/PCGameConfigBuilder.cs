using GamePassScores.InfoCollectorConsole.DataPublisher;
using GamePassScores.InfoCollectorConsole.DataSaver;
using GamePassScores.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamePassScores.InfoCollectorConsole.Config
{
    internal class PCGameConfigBuilder : IConfigBuilder
    {

        List<IDataPublisher> _publishers = new List<IDataPublisher>();
        List<IDataSaver> _savers = new List<IDataSaver>();
        public PCGameConfigBuilder(string args)
        {
            var option = ArgParser.ParseJsonAsync(args).GetAwaiter().GetResult();

            foreach (var repoOption in option.RepoOptions)
            {
                _publishers.Add(new GitDataPublisher(repoOption.RepoPath, new List<string> { repoOption.NewPCGameInfoPath, repoOption.NewCompressedPCInfoPath }));
                _savers.Add(new JsonThenZipDataSaver(System.IO.Path.Combine(repoOption.RepoPath, repoOption.NewPCGameInfoPath), System.IO.Path.Combine(repoOption.RepoPath, repoOption.NewCompressedPCInfoPath)));
            }
        }

        public async Task SaveAsync(IList<Game> games)
        {
            foreach (var saver in _savers)
            {
                await saver.SaveAsync(games);
            }
        }

        public async Task SaveAndPublishAsync(IList<Game> games)
        {
            await SaveAsync(games);
            await PublishAsync();
        }

        public async Task PublishAsync()
        {
            foreach (var publisher in _publishers)
            {
                await publisher.PublishAsync();
            }
        }
    }
}
