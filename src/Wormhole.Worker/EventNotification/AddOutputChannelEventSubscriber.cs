using System;
using System.Threading.Tasks;
using Wormhole.DomainModel;

namespace Wormhole.Worker
{
    public class OutputChannelAddEventSubscriber : IOutputChannelEventSubscriber
    {
        public Task Subscribe(OutputChannelModificationInfo modificationInfo, NebulaService nebulaService)
        {
            if (string.IsNullOrWhiteSpace(modificationInfo.TargetUrl) ||
                string.IsNullOrWhiteSpace(modificationInfo.ExternalKey))
                return Task.FromException(new ArgumentNullException());

            return nebulaService?.CreateJobAsync(modificationInfo.TargetUrl, modificationInfo.ExternalKey).ContinueWith(res => nebulaService.StartJob(res.Result));
        }
        
    }

    public interface IOutputChannelEventSubscriber
    {
        Task Subscribe(OutputChannelModificationInfo outputChannelModificationInfo, NebulaService nebulaService);
    }
}