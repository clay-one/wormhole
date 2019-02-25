using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Wormhole.Api.Model.Publish;
using Wormhole.DataImplementation;
using Wormhole.DomainModel;
using Wormhole.Interface;
using Wormhole.Utils;

namespace Wormhole.Logic
{
    public class OutputChannelLogic : IOutputChannelLogic
    {
        private readonly IPublishMessageLogic _publishMessageLogic;
        private readonly IOutputChannelDa _outputChannelDa;

        public OutputChannelLogic(IPublishMessageLogic publishMessageLogic, IOutputChannelDa outputChannelDa)
        {
            _publishMessageLogic = publishMessageLogic;
            _outputChannelDa = outputChannelDa;
        }
        

        public async Task Create(OutputChannel channel)
        {
            await _outputChannelDa.AddOutputChannel(channel);
            _publishMessageLogic.ProduceMessage(new PublishInput()
            {
                Tenant = Constants.EventSourcingTopicName,
                Payload = new OutputChannelModificationInfo()
                {
                    ModificationType = OutputChannelModificationType.ADD,
                    OutputChannel = channel
                }
            });
        }
    }
}