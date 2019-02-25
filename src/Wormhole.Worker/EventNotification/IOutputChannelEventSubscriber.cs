using System.Threading.Tasks;
using Wormhole.DomainModel;

namespace Wormhole.Worker
{
    public interface IOutputChannelEventSubscriber
    {
        Task Subscribe(OutputChannelModificationInfo outputChannelModificationInfo, NebulaService nebulaService);
    }
}