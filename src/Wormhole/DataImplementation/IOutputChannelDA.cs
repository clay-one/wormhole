using System.Collections.Generic;
using System.Threading.Tasks;
using Wormhole.DomainModel;

namespace Wormhole.DataImplementation
{
    public interface IOutputChannelDa
    {
        Task AddOutputChannel(OutputChannel channel);
        Task<List<OutputChannel>> FindOutputChannels();
        Task SetJobId(string id, string jobId);
    }
}