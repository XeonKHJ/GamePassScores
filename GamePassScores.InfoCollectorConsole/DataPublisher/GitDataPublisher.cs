using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamePassScores.InfoCollectorConsole.DataPublisher
{
    internal class GitDataPublisher : IDataPublisher
    {
        private string _repoFolder = string.Empty;
        private IList<string> _commitFiles = new List<string>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="repoFolder"></param>
        /// <param name="commitFiles">The relative path of the files that to be commited</param>
        public GitDataPublisher(string repoFolder, IList<string> commitFiles)
        {
            _repoFolder = repoFolder; 
            _commitFiles = commitFiles;
        }
        public async Task PublishAsync()
        {
            using (var repo = new Repository(_repoFolder))
            {
                // Stage the file
                foreach(var filePath in _commitFiles)
                {
                    var fullFilePath = System.IO.Path.Combine(_repoFolder, filePath);
                    repo.Index.Add(fullFilePath);
                }
                repo.Index.Write();

                // Create the committer's signature and commit
                Signature author = new("GameInfo Collectors", "dont@me.please", DateTime.Now);
                Signature committer = author;

                // Commit to the repository
                try
                {
                    Commit commit = repo.Commit("Update games' info.", author, committer);
                    Console.WriteLine("Files commited.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
                try
                {
                    PushOptions options = new PushOptions();

                    options.CredentialsProvider = new CredentialsHandler(
                        (url, usernameFromUrl, types) =>
                            new DefaultCredentials()
                            );

                    Console.WriteLine("Pushing repo {0}...", _repoFolder);
                    // This part requires Git installed.
                    Console.WriteLine("Executing command: git -C \"{0}\" push", _repoFolder);
                    System.Diagnostics.Process.Start("git", string.Format("-C \"{0}\" push", _repoFolder));
                    Console.WriteLine("Repo {0} is pushed.", _repoFolder);

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    System.Diagnostics.Debug.WriteLine("Push failed:{0}", ex.Message);
                }
            }
        }
    }
}
