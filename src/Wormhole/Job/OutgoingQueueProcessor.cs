using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nebula;
using Nebula.Queue;
using Nebula.Storage.Model;
using Wormhole.Interface;

namespace Wormhole.Job
{
    public class OutgoingQueueProcessor : IFinalizableJobProcessor<OutgoingQueueStep>
    {
        private readonly IPublishMessageLogic _publishMessageLogic;

        private static ILogger<OutgoingQueueProcessor> Logger { get; set; }

        public OutgoingQueueProcessor(IPublishMessageLogic publishMessageLogic)
        {
            _publishMessageLogic = publishMessageLogic;
        }

        public Task<long> GetTargetQueueLength()
        {
            return Task.FromResult(0L);
        }

        public void Initialize(JobData jobData, NebulaContext nebulaContext)
        {
          //  throw new NotImplementedException();
        }

        public Task JobCompleted()
        {
            return Task.CompletedTask;
        }

        public async Task<JobProcessingResult> Process(List<OutgoingQueueStep> items)
        {
            var failCount = 0;

            foreach (var item in items)
            {
                var result = await _publishMessageLogic.SendMessage(item);

                if (!result.Success)
                    failCount++;
            }

            Logger.LogInformation($"OutgoingQueueProcessor - Process FailCount: {failCount}");

            return new JobProcessingResult
            {
                ItemsFailed = failCount
            };
        }
    }
}