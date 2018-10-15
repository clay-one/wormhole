using ComposerCore.Attributes;
using Wormhole.Api.Model;
using Wormhole.Models;

namespace Wormhole.Logic
{
    public interface IPublishMessageLogic
    {
        ProduceMessageOutput ProduceMessage(PublishInput input);
    }
}
