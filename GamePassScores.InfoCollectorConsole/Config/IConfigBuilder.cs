﻿using GamePassScores.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamePassScores.InfoCollectorConsole.Config
{
    internal interface IConfigBuilder
    {
        public Task SaveAndPublishAsync(IList<Game> games);
        public Task PublishAsync();
        public Task SaveAsync(IList<Game> games);
    }
}
