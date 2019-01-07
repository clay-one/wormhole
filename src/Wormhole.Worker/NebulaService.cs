using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Nebula;
using Nebula.Queue;
using Nebula.Queue.Implementation;
using Nebula.Storage.Model;
using Newtonsoft.Json;
using Wormhole.Configurations;
using Wormhole.DataImplementation;
using Wormhole.DomainModel;
using Wormhole.Job;

namespace Wormhole.Worker
{
    public class NebulaService
    {
        public readonly NebulaContext NebulaContext;
        private readonly IOutputChannelDa _outputChannelDa;
        public NebulaService(IOptions<NebulaConfig> options, IJobProcessor<HttpPushOutgoingQueueStep> jobProcessor, IOutputChannelDa outputChannelDa)
        {
            var nebulaConfig = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _outputChannelDa = outputChannelDa;
            NebulaContext = new NebulaContext();
            ConfigureNebulaContext(jobProcessor, nebulaConfig);
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            NebulaContext.StartWorkerService();
            await StartJobs();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            NebulaContext.StopWorkerService();
            return Task.CompletedTask;
        }


        private void ConfigureNebulaContext(IJobProcessor<HttpPushOutgoingQueueStep> jobProcessor, NebulaConfig nebulaConfig)
        {
            NebulaContext.MongoConnectionString = nebulaConfig.MongoConnectionString;
            NebulaContext.RedisConnectionString = nebulaConfig.RedisConnectionString;

            NebulaContext.RegisterJobQueue(typeof(DelayedJobQueue<>), QueueType.Delayed);

            NebulaContext.RegisterJobProcessor(jobProcessor, typeof(HttpPushOutgoingQueueStep));
        }
        private async Task StartJobs()
        {
            var outputChannels = await GetOutputChannels();
            await CreateHttpPushOutgoingQueueJobsAsync(outputChannels.Where(o => o.ChannelType == ChannelType.HttpPush)
                .ToList());
        }


        private async Task<List<OutputChannel>> GetOutputChannels()
        {
            return await _outputChannelDa.FindOutputChannels();
        }


        private async Task CreateHttpPushOutgoingQueueJobsAsync(List<OutputChannel> outputChannels)
        {
            var parameters = new HttpPushOutgoingQueueParameters();

            foreach (var outputChannel in outputChannels)
            {
                var channelSpecification = outputChannel.TypeSpecification as HttpPushOutputChannelSpecification;
                if (string.IsNullOrWhiteSpace(outputChannel.JobId))
                {
                    parameters.TargetUrl = channelSpecification?.TargetUrl;

                    // todo: static job might be a better choice
                    outputChannel.JobId = await NebulaContext.GetJobManager()
                        .CreateNewJobOrUpdateDefinition<HttpPushOutgoingQueueStep>(
                            "Fanap-plus",
                            $"Wormhole_{outputChannel.JobId}",
                            configuration: new JobConfigurationData
                            {
                                MaxBatchSize = 1,
                                MaxConcurrentBatchesPerWorker = 5,
                                IdleSecondsToCompletion = 30,
                                MaxBlockedSecondsPerCycle = 60,
                                MaxTargetQueueLength = 100000,
                                Parameters = JsonConvert.SerializeObject(parameters),
                                QueueTypeName = QueueType.Delayed,
                                IsIndefinite = true
                            }, jobId: $"Wormhole_Job_{outputChannel.Id.ToString()}");
                    await _outputChannelDa.SetJobId(outputChannel.Id.ToString(), outputChannel.JobId);
                    await NebulaContext.GetJobManager().StartJob("__none__", outputChannel.JobId);
                }
            }
        }
    }
}