using System;
using System.Threading.Tasks;
using Wormhole.DomainModel;

namespace Wormhole.Worker.EventNotification
{
    public class DeleteOutputChannelEventSubscriber : IOutputChannelEventSubscriber
    {
        public async Task Subscribe(OutputChannelModificationInfo modificationInfo, NebulaService nebulaService)
        {
            nebulaService.RemoveInMemoryOutputChannels(modificationInfo.OutputChannel);
            await nebulaService.StopJob(modificationInfo.OutputChannel.JobId);
        }
    }
}