namespace Wormhole.Job
{
    public class OutgoingQueueParameters
    {
        public int RetryCount { get; set; }
        public int RetryInterval { get; set; }
    }
}