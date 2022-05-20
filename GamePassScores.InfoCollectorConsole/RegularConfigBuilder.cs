﻿using GamePassScores.InfoCollectorConsole.DataPublisher;
using GamePassScores.InfoCollectorConsole.DataSaver;
using GamePassScores.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamePassScores.InfoCollectorConsole
{
    internal class RegularConfigBuilder : IConfigBuilder
    {

        List<IDataPublisher> _publishers = new List<IDataPublisher>();
        List<IDataSaver> _savers = new List<IDataSaver>();
        public RegularConfigBuilder(string args)
        {
            var option = ArgParser.ParseJsonAsync(args).GetAwaiter().GetResult();

            foreach(var repoOption in option.RepoOptions)
            {
                _publishers.Add(new GitDataPublisher(repoOption.RepoPath, new List<string> { repoOption.NewInfoFilePath, repoOption.NewCompressedInfoFilePath }));
                _savers.Add(new JsonThenZipDataSaver(repoOption.NewInfoFilePath, repoOption.NewCompressedInfoFilePath));
            }
        }
        public async Task SaveAndPublishAsync(IList<Game> games)
        {
            foreach(var saver in _savers)
            {
                await saver.SaveAsync(games);
            }

            foreach(var publisher in _publishers)
            {
                await publisher.PublishAsync();
            }
        }
    }
}
