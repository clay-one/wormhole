using Nebula.Queue;

namespace Wormhole.Job
{
    public class HttpPushOutgoingQueueStep : IJobStep
    {
        public string StepId { get; set; }
        public object Payload { get; set; }
        public string Category { get; set; }
        public string Tag { get; set; }
        public int FailCount { get; set; }
    }
}