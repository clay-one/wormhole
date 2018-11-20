using Wormhole.DomainModel;

namespace Wormhole.Job
{
    public class HttpPushOutgoingQueueParameters
    {
        public int RetryCount { get; set; }
        public int RetryInterval { get; set; }
        public string TargetUrl { get; set; }
    }
}