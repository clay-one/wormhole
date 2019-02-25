using System;
using System.Threading.Tasks;
using Wormhole.DomainModel;

namespace Wormhole.Worker.EventNotification
{
    public class OutputChannelAddEventSubscriber : IOutputChannelEventSubscriber
    {
        public Task Subscribe(OutputChannelModificationInfo modificationInfo, NebulaService nebulaService)
        {
            var outputChannel = modificationInfo.OutputChannel;
            if (outputChannel == null)
                return Task.FromException(new ArgumentNullException());

            if (outputChannel.TypeSpecification is HttpPushOutputChannelSpecification channelSpecification)
                    return nebulaService?.CreateJobAsync(channelSpecification.TargetUrl, outputChannel.ExternalKey)
                        .ContinueWith(res => nebulaService.StartJob(res.Result));

            return Task.FromException(new ArgumentNullException($"OutputChannel Type is not supported"));
        }

    }
}