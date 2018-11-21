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
        public Task EnsureJobSourceExists(string jobId = null)
        {
            return Task.CompletedTask;
        }

        public Task<bool> Any(string jobId = null)
        {
            return Task.FromResult(false);
        }

        public Task Purge(string jobId = null)
        {
            return Task.CompletedTask;
        }

        public Task<HttpPushOutgoingQueueStep> GetNext(string jobId = null)
        {
            return Task.FromResult(new HttpPushOutgoingQueueStep());
        }

        public Task<IEnumerable<HttpPushOutgoingQueueStep>> GetNextBatch(int maxBatchSize, string jobId = null)
        {
            return Task.FromResult(new List<HttpPushOutgoingQueueStep>().AsEnumerable());
        }

        public Task EnqueueBatch(IEnumerable<Tuple<HttpPushOutgoingQueueStep, DateTime>> items, string jobId = null)
        {
            return Task.CompletedTask;
        }

        public Task EnqueueBatch(IEnumerable<Tuple<HttpPushOutgoingQueueStep, TimeSpan>> items, string jobId = null)
        {
            return Task.CompletedTask;
        }
    }
}