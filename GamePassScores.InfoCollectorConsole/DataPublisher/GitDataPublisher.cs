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
            try
            {
                string gitExe = "git";
                var commitInfo = new { Message = "docs: Update game's info", Name = "InfoCollectorBot", Email = "redalertkhj@live.cn" };

                // Format command line.
                string commitCmdLine = string.Format("-C \"{0}\" commit -a -m \"{1}\" --author \"{2} <{3}>\"", _repoFolder, commitInfo.Message, commitInfo.Name, commitInfo.Email);
                string pushCmdLine = string.Format("-C \"{0}\" push", _repoFolder);

                System.Diagnostics.Process gitCommitProcess = new System.Diagnostics.Process();
                gitCommitProcess.StartInfo.FileName = gitExe;
                gitCommitProcess.StartInfo.Arguments = commitCmdLine;

                System.Diagnostics.Process gitPushProcess = new System.Diagnostics.Process();
                gitPushProcess.StartInfo.FileName = gitExe;
                gitPushProcess.StartInfo.Arguments = pushCmdLine;

                // Git commit
                Console.WriteLine("Executing\t{0} {1}", gitExe, commitCmdLine);
                gitCommitProcess.Start();
                await gitCommitProcess.WaitForExitAsync();

                // git push
                Console.WriteLine("Executing\t{0} {1}", gitExe, pushCmdLine);
                gitPushProcess.Start();
                await gitPushProcess.WaitForExitAsync();

                Console.WriteLine("Status\tData published");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Status\tData published Failed.");
                                System.Diagnostics.Debug.WriteLine("Push failed:{0}", ex.Message);
            }
        }
    }
}
