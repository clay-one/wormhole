using System.Collections.Generic;
using System.Threading.Tasks;
using Wormhole.DomainModel;
using Wormhole.DomainModel.OutputChannel;

namespace Wormhole.DataImplementation
{
    public interface IOutputChannelDa
    {
        Task AddOutputChannel(OutputChannel channel);
        Task<List<OutputChannel>> FindAsync();
        Task<OutputChannel> FindAsync(string externalKey);
        Task<OutputChannel> SetJobId(string externalKey, string jobId);
    }
}