using Wormhole.Api.Model.PublishModel;
using Wormhole.DTO;

namespace Wormhole.Interface
{
    public interface IPublishMessageLogic
    {
        ProduceMessageOutput ProduceMessage(PublishInput input);
    }
}
