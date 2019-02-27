using System.Threading.Tasks;
using Wormhole.DomainModel;
using Wormhole.DomainModel.OutputChannel;

namespace Wormhole.Interface
{
    public interface IOutputChannelLogic
    {
        Task Create(OutputChannel channel);
    }
}