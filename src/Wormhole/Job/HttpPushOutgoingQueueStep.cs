using Nebula.Queue;

namespace Wormhole.Job
{
    public class HttpPushOutgoingQueueStep : IJobStep
        {
            public object Payload { get; set; }
            public string Category { get; set; }
            public int FailCount { get; set; }
    }
}