using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Wormhole.Api.Model.PublishModel;
using Wormhole.DataImplementation;
using Wormhole.DomainModel;
using Wormhole.DomainModel.OutputChannel;
using Wormhole.Interface;
using Wormhole.Kafka;
using Wormhole.Utils;

namespace Wormhole.Logic
{
    public class OutputChannelLogic : IOutputChannelLogic
    {
        private readonly IKafkaProducer _kafkaProducer;
        private readonly IOutputChannelDa _outputChannelDa;

        public OutputChannelLogic(IKafkaProducer producer, IOutputChannelDa outputChannelDa)
        {
            _kafkaProducer = producer;
            _outputChannelDa = outputChannelDa;
        }
        

        public async Task Create(OutputChannel channel)
        {
            await _outputChannelDa.AddOutputChannel(channel);
            _kafkaProducer.Produce(Constants.EventSourcingTopicName,
                new OutputChannelModificationInfo
                {
                    ModificationType = OutputChannelModificationType.ADD,
                    OutputChannel = channel
                }
            );
        }
    }
}