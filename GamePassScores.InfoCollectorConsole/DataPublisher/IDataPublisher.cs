using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamePassScores.InfoCollectorConsole.DataPublisher
{
    internal interface IDataPublisher
    {
        public Task PublishAsync();
    }
}
