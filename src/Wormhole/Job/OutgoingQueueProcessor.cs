using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nebula;
using Nebula.Queue;
using Nebula.Storage.Model;
using Wormhole.Logic;

namespace Wormhole.Job
{
    public class OutgoingQueueProcessor : IFinalizableJobProcessor<OutgoingQueueStep>
    {
        private readonly IPublishMessageLogic _publishMessageLogic;

        public OutgoingQueueProcessor(IPublishMessageLogic publishMessageLogic)
        {
            _publishMessageLogic = publishMessageLogic;
        }

        public Task<long> GetTargetQueueLength()
        {
            throw new NotImplementedException();
        }

        public void Initialize(JobData jobData, NebulaContext nebulaContext)
        {
            throw new NotImplementedException();
        }

        public Task JobCompleted()
        {
            throw new NotImplementedException();
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

            return new JobProcessingResult
            {
                ItemsFailed = failCount
            };
        }
    }
}