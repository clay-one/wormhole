using System.Threading.Tasks;
using Wormhole.DomainModel;

namespace Wormhole.Interface
{
    public interface IOutputChannelLogic
    {
        Task Create(OutputChannel channel);
    }
}