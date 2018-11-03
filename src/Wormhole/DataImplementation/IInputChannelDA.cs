using System.Threading.Tasks;
using Wormhole.DomainModel.InputChannel;

namespace Wormhole.DataImplementation
{
    public interface IInputChannelDa
    {
        Task AddInputChannelAsync(InputChannel channel);
    }
}