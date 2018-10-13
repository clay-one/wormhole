using ComposerCore.Attributes;
using Wormhole.Api.Model;

namespace Wormhole.Logic
{
    public interface IPublishMessage
    {
        void ProduceMessage(PublishInput input);
    }
}
