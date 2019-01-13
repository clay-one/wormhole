using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nebula;
using Nebula.Queue;
using Nebula.Storage.Model;

namespace Wormhole.Job
{
    public class SampleProcessor : IJobProcessor<HttpPushOutgoingQueueStep>
    {
        public SampleProcessor(ILogger<SampleProcessor> logger)
        {
            Logger = logger;
        }

        private long count = 0;

        private ILogger<SampleProcessor> Logger { get; }

        public void Initialize(JobData jobData, NebulaContext nebulaContext)
        {
            var timer = new System.Threading.Timer(state =>
            {
                Console.WriteLine($"{count}");

                Interlocked.Exchange(ref count, 0);
            },null,0,1000);
        }

        public async Task<JobProcessingResult> Process(List<HttpPushOutgoingQueueStep> items)
        {
            Interlocked.Add(ref count, items.Count);
            Logger.LogDebug($"{DateTime.Now}__items got processed!");

            var res = new JobProcessingResult();
            return res;
        }

        public Task<long> GetTargetQueueLength()
        {
            return Task.FromResult(0L);
        }
    }
}
