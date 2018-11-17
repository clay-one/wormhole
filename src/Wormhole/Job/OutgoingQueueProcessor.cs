using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nebula;
using Nebula.Queue;
using Nebula.Queue.Implementation;
using Nebula.Storage.Model;
using Newtonsoft.Json;
using Wormhole.Interface;

namespace Wormhole.Job
{
    public class OutgoingQueueProcessor : IFinalizableJobProcessor<OutgoingQueueStep>
    {
        private readonly IPublishMessageLogic _publishMessageLogic;
        private string _jobId;
        private IDelayedJobQueue<OutgoingQueueStep> _jobQueue;
        private OutgoingQueueParameters _parameters;

        private ILogger<OutgoingQueueProcessor> Logger { get; set; }

        public OutgoingQueueProcessor(IPublishMessageLogic publishMessageLogic, ILogger<OutgoingQueueProcessor> logger)
        {
            _publishMessageLogic = publishMessageLogic;
            Logger = logger;
        }

        public Task<long> GetTargetQueueLength()
        {
            return Task.FromResult(0L);
        }

        public void Initialize(JobData jobData, NebulaContext nebulaContext)
        {
            if (jobData == null)
                throw new ArgumentNullException(nameof(jobData), ErrorKeys.ParameterNull);


            if (nebulaContext == null)
                throw new ArgumentNullException(nameof(nebulaContext), ErrorKeys.ParameterNull);

            _jobQueue = nebulaContext.GetDelayedJobQueue<OutgoingQueueStep>(QueueType.Delayed);
            _jobId = jobData.JobId;

            var parametersString = jobData.Configuration?.Parameters;
            if (string.IsNullOrWhiteSpace(parametersString))
                throw new ArgumentNullException(nameof(jobData.Configuration.Parameters), ErrorKeys.ParameterNull);

            _parameters = JsonConvert.DeserializeObject<OutgoingQueueParameters>(parametersString);
        }

        public Task JobCompleted()
        {
            return Task.CompletedTask;
        }

        public async Task<JobProcessingResult> Process(List<OutgoingQueueStep> items)
        {
            return JobProcessingResult.Combine(
                await Task.WhenAll(items.Select(ProcessOne).ToArray()));
        }

        private async Task<JobProcessingResult> ProcessOne(OutgoingQueueStep item)
        {
            var result = new JobProcessingResult();
            var publishResult = await _publishMessageLogic.SendMessage(item);
            if (publishResult.Success)
                return result;

            item.FailCount += 1;
            if (item.FailCount<_parameters.RetryCount)
            {
                result.ItemsRequeued += 1;
                await _jobQueue.Enqueue(item, DateTime.UtcNow.AddMinutes(_parameters.RetryInterval), _jobId);
                return result;
            }

            Logger.LogInformation($"OutgoingQueueProcessor - Process FailCount: {item.FailCount}");

            result.ItemsFailed++;
            return result;
        }
    }
}