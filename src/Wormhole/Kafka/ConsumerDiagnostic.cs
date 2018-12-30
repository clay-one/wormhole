using System;
using System.Threading;

namespace Wormhole.Kafka
{
    public class ConsumerDiagnostic
    {
        private int _consumedMessageCount;
        private int _exceptionCount;

        public string Name { get; }

        public ConsumerDiagnostic(string name)
        {
            Name = name;
        }

        private ConsumerDiagnostic()
        {

        }

        public DateTime LastConsumeTime { get; private set; }

        public int ConsumedMessageCount
        {
            get => _consumedMessageCount;
            set => _consumedMessageCount = value;
        }

        public int ExceptionCount
        {
            get => _exceptionCount;
            set => _exceptionCount = value;
        }


        public void IncrementConsumedMessageCount()
        {
            Interlocked.Increment(ref _consumedMessageCount);
        }

        public void SetConsumerLastCallTime()
        {
            LastConsumeTime = DateTime.Now;
        }


        public void IncrementExceptionCount()
        {
            Interlocked.Increment(ref _exceptionCount);
        }
    }
}