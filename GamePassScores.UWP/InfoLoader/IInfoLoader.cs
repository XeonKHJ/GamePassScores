﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamePassScores.UWP.InfoLoader
{
    internal interface IInfoLoader
    {
        Task<List<Models.Game>> LoadAsync();
        Task<List<Models.Game>> RefreshAsync();
    }
}