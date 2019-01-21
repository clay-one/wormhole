using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using hydrogen.General.Collections;
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
        private readonly IOutputChannelDa _outputChannelDa;
        private readonly NebulaContext _nebulaContext;
        private readonly List<OutputChannel> OutputChannels = new List<OutputChannel>();

        public NebulaContext NebulaContext => _nebulaContext;

        public NebulaService(IOptions<NebulaConfig> options, IJobProcessor<HttpPushOutgoingQueueStep> jobProcessor,
            IOutputChannelDa outputChannelDa)
        {
            var nebulaConfig = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _outputChannelDa = outputChannelDa;
            _nebulaContext = new NebulaContext();
            ConfigureNebulaContext(jobProcessor, nebulaConfig);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _nebulaContext.StartWorkerService();
            await StartJobs();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _nebulaContext.StopWorkerService();
            return Task.CompletedTask;
        }

        public IEnumerable<KeyValuePair<string, string>> GetJobIdTagPairs(string tenant, string category,
            IList<string> tags)
        {
            var list = new List<KeyValuePair<string, string>>();
            foreach (var tag in tags)
            {
                list.Add(OutputChannels.Where(o =>
                        o.FilterCriteria.Category == category && o.TenantId == tenant && o.FilterCriteria.Tag == tag)
                    .Select(o => new KeyValuePair<string, string>(o.JobId, o.FilterCriteria.Tag)).FirstOrDefault());
            }

            return list;
        }

        private void ConfigureNebulaContext(IJobProcessor<HttpPushOutgoingQueueStep> jobProcessor,
            NebulaConfig nebulaConfig)
        {
            _nebulaContext.MongoConnectionString = nebulaConfig.MongoConnectionString;
            _nebulaContext.RedisConnectionString = nebulaConfig.RedisConnectionString;

            _nebulaContext.RegisterJobQueue(typeof(DelayedJobQueue<>), QueueType.Delayed);

            _nebulaContext.RegisterJobProcessor(jobProcessor, typeof(HttpPushOutgoingQueueStep));
        }

        private async Task StartJobs()
        {
            OutputChannels.AddAll(await GetOutputChannels());
            await CreateHttpPushOutgoingQueueJobsAsync(OutputChannels.Where(o => o.ChannelType == ChannelType.HttpPush)
                .ToList());
        }

        private async Task<List<OutputChannel>> GetOutputChannels()
        {
            return await _outputChannelDa.FindAsync();
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
                    outputChannel.JobId = await _nebulaContext.GetJobManager()
                        .CreateNewJobOrUpdateDefinition<HttpPushOutgoingQueueStep>(
                            "Fanap-plus",
                            $"Wormhole_{outputChannel.JobId}",
                            configuration: new JobConfigurationData
                            {
                                MaxBatchSize = 128,
                                MaxConcurrentBatchesPerWorker = 8,
                                IdleSecondsToCompletion = 30,
                                MaxBlockedSecondsPerCycle = 60,
                                MaxTargetQueueLength = 100000,
                                Parameters = JsonConvert.SerializeObject(parameters),
                                QueueTypeName = QueueType.Delayed,
                                IsIndefinite = true
                            }, jobId: $"Wormhole_{outputChannel.ExternalKey}");
                    await _outputChannelDa.SetJobId(outputChannel.Id.ToString(), outputChannel.JobId);
                }
                await _nebulaContext.GetJobManager().StartJobIfNotStarted("Fanap-plus", outputChannel.JobId);

            }
        }
    }
}