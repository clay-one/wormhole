using System;
using System.Threading.Tasks;
using Wormhole.DomainModel;

namespace Wormhole.Worker.EventNotification
{
    public class OutputChannelAddEventSubscriber : IOutputChannelEventSubscriber
    {
        public async Task Subscribe(OutputChannelModificationInfo modificationInfo, NebulaService nebulaService)
        {
            var outputChannelSpecification = modificationInfo.OutputChannel.TypeSpecification;
            if (outputChannelSpecification == null)
                return;

            nebulaService.UpdateInMemoryOutputChannels(modificationInfo.OutputChannel, modificationInfo.ModificationType);

            if (outputChannelSpecification is HttpPushOutputChannelSpecification channelSpecification)
                    await nebulaService.CreateJobAsync(channelSpecification.TargetUrl, modificationInfo.OutputChannel.ExternalKey)
                        .ContinueWith(res => nebulaService.StartJob(res.Result));
            else
                throw new ArgumentNullException($"OutputChannel Type is not supported");
        }

    }
}