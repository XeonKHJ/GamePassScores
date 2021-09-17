using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamePassScores.InfoCollectorConsole
{
    internal class RepoOption
    {
        public string RepoPath { set; get; }

        /// <summary>
        /// Relative path.
        /// </summary>

        public string NewInfoFilePath { set; get; }

        public string NewCompressedInfoFilePath { set; get; }

        //public string AuthenticationMethod { set; get; }
        //public string PrivateKey { set; get; }
        //public string Username { set; get; }
        //public string Password { set; get; }

        public string Branch { set; get; }

    }
}
