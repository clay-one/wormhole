using System;
using System.Threading.Tasks;
using Wormhole.DomainModel;

namespace Wormhole.Worker.EventNotification
{
    public class EditOutputChannelEventSubscriber : IOutputChannelEventSubscriber
    {
        public async Task Subscribe(OutputChannelModificationInfo modificationInfo, NebulaService nebulaService)
        {
            var outputChannelSpecification = modificationInfo.OutputChannel.TypeSpecification;
            if (outputChannelSpecification == null)
                return;

            nebulaService.ModifyInMemoryOutputChannels(modificationInfo.OutputChannel);

            if (outputChannelSpecification is HttpPushOutputChannelSpecification channelSpecification)
                    await nebulaService.CreateOrUpdateJobAsync(channelSpecification.TargetUrl, modificationInfo.OutputChannel.ExternalKey)
                        .ContinueWith(res => nebulaService.StartJobIfNotStarted(res.Result));
            else
                throw new ArgumentNullException($"OutputChannel Type is not supported");
        }

    }
}