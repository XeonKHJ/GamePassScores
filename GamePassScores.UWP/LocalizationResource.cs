using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamePassScores.UWP
{
    class LocalizationResource
    {
        private static Dictionary<string, string> _categoriesResourcePairs = new Dictionary<string, string>
        {
            { "Action & adventure", "GenreActionAndAdventure"},
            {"Card & board" ,"GenreCardAndBoard"},
            {"Classics", "GenreClassics" },
            {"Family & kids", "GenreFamilyAndKids" },
            {"Fighting", "GenreFighting" },
            {"Multi-player Online Battle Arena", "GenreMultiPlayerOnlineBattleArena" },
            {"Music", "GenreMusic" },
            {"Other", "GenreOther" },
            {"Puzzle & trivia", "GenrePuzzleAndTrivia" },
            {"Racing & flying", "GenreRacingAndFlying" },
            {"Role playing", "GenreRolePlaying" },
            {"Shooter","GenreShooter" },
            {"Simulation","GenreSimulation" },
            {"Sports","GenreSports" },
            {"Strategy", "GenreStrategy" },
            {"Platformer", "GenrePlatformer" }
        };

        public static string GetLocalizedCategoryName(string englishCategoryName)
        {
            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            string categoryName = "";
            try
            {
                categoryName = resourceLoader.GetString(_categoriesResourcePairs[englishCategoryName]);
                if (string.IsNullOrEmpty(categoryName))
                {
                    categoryName = englishCategoryName;
                }
            }
            catch (Exception)
            {
                categoryName = englishCategoryName;
            }
            return categoryName;
        }
    }
}
