using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamePassScores.UWP.InfoLoader
{
    static internal class LoaderFactory
    {
        private static Dictionary<Uri, InfoProviderContext> _consoleInfoSource = new Dictionary<Uri, InfoProviderContext>
        {
            { new Uri("https://github.com/XeonKHJ/GamePassScoresInfo/blob/main/ConsoleGames.json?raw=true"), new InfoProviderContext{IsCompressed = false} },
            { new Uri("https://gitee.com/xeonkhj/game-pass-scores-info/raw/master/CompressedConsoleGames.zip"),new InfoProviderContext { IsCompressed = true }}
        };

        private static Dictionary<Uri, InfoProviderContext> _pcInfoSource = new Dictionary<Uri, InfoProviderContext>
        {
            { new Uri("https://github.com/XeonKHJ/GamePassScoresInfo/blob/main/PCGames.json?raw=true"), new InfoProviderContext{IsCompressed = false} },
            { new Uri("https://gitee.com/xeonkhj/game-pass-scores-info/raw/master/CompressedPCGames.zip"),new InfoProviderContext { IsCompressed = true }}
        };

        public static IInfoLoader PCGameInfoLoader
        {
            get
            {
                return new MultiSourceInfoLoader("PCGames.json", _pcInfoSource);
            }
        }

        public static IInfoLoader ConsoleGameInfoLoader
        {
            get
            {
                return new MultiSourceInfoLoader("ConsoleGames.json", _consoleInfoSource);
            }
        }
    }
}
