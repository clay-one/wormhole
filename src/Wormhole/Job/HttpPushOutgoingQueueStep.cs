using System.Collections.Generic;
using Nebula.Queue;
using Wormhole.DomainModel;

namespace Wormhole.Job
{
    public class HttpPushOutgoingQueueStep : IJobStep
        {
            public string StepId { get; set; }
            public object Payload { get; set; }
            public string Category { get; set; }
            public string Tag { get; set; }
            public int FailCount { get; set; }
            public List<PublishMessageOutput> PublishOutputs { get; set; } = new List<PublishMessageOutput>();
    }
}