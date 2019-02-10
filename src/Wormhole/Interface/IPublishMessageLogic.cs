using Wormhole.Api.Model.Publish;
using Wormhole.DTO;

namespace Wormhole.Interface
{
    public interface IPublishMessageLogic
    {
        ProduceMessageOutput ProduceMessage(PublishInput input);
    }
}
