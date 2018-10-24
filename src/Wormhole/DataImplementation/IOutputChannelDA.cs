using System.Threading.Tasks;
using Wormhole.DomainModel;

namespace Wormhole.DataImplementation
{
    public interface IOutputChannelDA
    {
        Task AddOutputChannel(OutputChannel tenant);

    }
}