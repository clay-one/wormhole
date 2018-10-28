using System.Threading.Tasks;
using Wormhole.DomainModel;

namespace Wormhole.DataImplementation
{
    public interface IInputChannelDA
    {
        Task AddInputChannelAsync(InputChannel channel);

    }
}