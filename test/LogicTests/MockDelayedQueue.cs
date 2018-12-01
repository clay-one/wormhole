using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nebula.Queue;
using Wormhole.Job;

namespace Wormhole.Tests.LogicTests
{
    public class MockDelayedQueue : IDelayedJobQueue<HttpPushOutgoingQueueStep>
    {
        public void Initialize(string jobId = null)
        {
        }

        public Task EnsureJobSourceExists()
        {
            return Task.CompletedTask;
        }

        public Task<bool> Any()
        {
            return Task.FromResult(false);
        }

        public Task Purge()
        {
            return Task.CompletedTask;
        }

        public Task<HttpPushOutgoingQueueStep> GetNext()
        {
            return Task.FromResult(new HttpPushOutgoingQueueStep());
        }

        public Task<IEnumerable<HttpPushOutgoingQueueStep>> GetNextBatch(int maxBatchSize)
        {
            return Task.FromResult(new List<HttpPushOutgoingQueueStep>().AsEnumerable());
        }

        public Task EnqueueBatch(IEnumerable<Tuple<HttpPushOutgoingQueueStep, DateTime>> items)
        {
            return Task.CompletedTask;
        }

        public Task EnqueueBatch(IEnumerable<Tuple<HttpPushOutgoingQueueStep, TimeSpan>> items)
        {
            return Task.CompletedTask;
        }
    }
}