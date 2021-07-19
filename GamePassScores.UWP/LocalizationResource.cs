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
            string categoryName;
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

        public static string GetReleaseDateString()
        {
            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            string categoryName = "Release Date";
            try
            {
                categoryName = resourceLoader.GetString("DetailReleaseDate");
                if (string.IsNullOrEmpty(categoryName))
                {
                    categoryName = "Release Date";
                }
            }
            catch (Exception)
            {
            }
            return categoryName;
        }

        public static string GetCategoryString()
        {
            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            string categoryName = "Category";
            try
            {
                categoryName = resourceLoader.GetString("DetailCategory");
                if (string.IsNullOrEmpty(categoryName))
                {
                    categoryName = "Category";
                }
            }
            catch (Exception)
            {
            }
            return categoryName;
        }

        public static string GetCategoriesString()
        {
            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            string categoryName = "Categories";
            try
            {
                categoryName = resourceLoader.GetString("DetailCategories");
                if (string.IsNullOrEmpty(categoryName))
                {
                    categoryName = "Categories";
                }
            }
            catch (Exception)
            {
            }
            return categoryName;
        }

        public static string GetEstimatedDownloadSizeString()
        {
            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            string categoryName = "Estimated Download Size";
            try
            {
                categoryName = resourceLoader.GetString("DetailEstimatedDownloadSize");
                if (string.IsNullOrEmpty(categoryName))
                {
                    categoryName = "Estimated Download Size";
                }
            }
            catch (Exception)
            {
            }
            return categoryName;
        }
    }
}
