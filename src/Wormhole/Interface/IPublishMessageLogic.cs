using System.Threading.Tasks;
using Wormhole.Api.Model;
using Wormhole.DTO;
using Wormhole.Job;

namespace Wormhole.Interface
{
    public interface IPublishMessageLogic
    {
        ProduceMessageOutput ProduceMessage(PublishInput input);
    }
}
