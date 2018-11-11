using Nebula.Queue;

namespace Wormhole.Job
{
    public class OutgoingQueueStep : IJobStep
    {
        public object Payload { get; set; }
        public string Category { get; set; }
        public int FailCount { get; set; }
    }
}