using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamePassScores.InfoCollectorConsole.AppException
{
    internal class FatalException : ApplicationException
    {
        public FatalException(string message) : base(message) { }
        public FatalException() { }
    }
}
