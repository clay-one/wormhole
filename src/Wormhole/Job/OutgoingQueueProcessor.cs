using System.Collections.Generic;
using System.Threading.Tasks;
using ComposerCore.Attributes;
using Nebula;
using Nebula.Queue;
using Nebula.Storage.Model;

namespace Wormhole.Job
{
    [Component]
    [Contract]
    public class OutgoingQueueProcessor : IFinalizableJobProcessor<OutgoingQueueStep>
    {
        public Task<long> GetTargetQueueLength()
        {
            throw new System.NotImplementedException();
        }

        public void Initialize(JobData jobData, NebulaContext nebulaContext)
        {
            throw new System.NotImplementedException();
        }

        public Task JobCompleted()
        {
            throw new System.NotImplementedException();
        }

        public Task<JobProcessingResult> Process(List<OutgoingQueueStep> items)
        {
            throw new System.NotImplementedException();
        }
    }
}