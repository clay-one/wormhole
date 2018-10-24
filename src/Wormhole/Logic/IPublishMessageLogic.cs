using System.Threading.Tasks;
using Wormhole.Api.Model;
using Wormhole.Job;
using Wormhole.Models;

namespace Wormhole.Logic
{
    public interface IPublishMessageLogic
    {
        Task<ProduceMessageOutput> ProduceMessage(PublishInput input);
        Task<SendMessageOutput> SendMessage(OutgoingQueueStep message);
    }
}
