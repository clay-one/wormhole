using System.Threading.Tasks;
using Wormhole.DomainModel;

namespace Wormhole.DataImplementation
{
    public interface IInputChannelDa
    {
        Task AddInputChannelAsync(InputChannel channel);
    }
}