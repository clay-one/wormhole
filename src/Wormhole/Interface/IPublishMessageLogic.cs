using System.Threading.Tasks;
using Wormhole.Api.Model;
using Wormhole.DTO;
using Wormhole.Job;

namespace Wormhole.Interface
{
    public interface IPublishMessageLogic
    {
        Task<ProduceMessageOutput> ProduceMessage(PublishInput input);
        Task<SendMessageOutput> SendMessage(OutgoingQueueStep message);
    }
}
