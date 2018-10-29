using System.Threading.Tasks;
using Wormhole.DomainModel;

namespace Wormhole.DataImplementation
{
    public interface IOutputChannelDa
    {
        Task AddOutputChannel(OutputChannel channel);
    }
}