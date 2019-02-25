using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hydrogen.General.Collections;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly List<OutputChannel> _outputChannels = new List<OutputChannel>();
        private readonly IServiceProvider _serviceProvider;
        private const string TenantId = "ir.fanap.plus";

        public NebulaService(IOptions<NebulaConfig> options, IServiceProvider serviceProvider,
            IOutputChannelDa outputChannelDa)
        {
            var nebulaConfig = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _serviceProvider = serviceProvider;
            _outputChannelDa = outputChannelDa;
            NebulaContext = new NebulaContext();
            ConfigureNebulaContext(nebulaConfig);
        }

        public NebulaContext NebulaContext { get; }

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

        public IEnumerable<KeyValuePair<string, string>> GetJobIdTagPairs(string tenant, string category,
            IList<string> tags)
        {
            var list = new List<KeyValuePair<string, string>>();
            foreach (var tag in tags)
            {
                list.Add(_outputChannels.Where(o =>
                        o.FilterCriteria.Category == category && o.TenantId == tenant && o.FilterCriteria.Tag == tag)
                    .Select(o => new KeyValuePair<string, string>(o.JobId, o.FilterCriteria.Tag)).FirstOrDefault());
            }

            return list;
        }

        private void ConfigureNebulaContext(NebulaConfig nebulaConfig)
        {
            NebulaContext.MongoConnectionString = nebulaConfig.MongoConnectionString;
            NebulaContext.RedisConnectionString = nebulaConfig.RedisConnectionString;

            NebulaContext.RegisterJobQueue(typeof(DelayedJobQueue<>), QueueType.Delayed);

            // registered HttpPushOutgoingQueueProcessor in host builder instead of IJobProcessor<HttpPushOutgoingQueueStep>
            NebulaContext.RegisterJobProcessor(
                () => _serviceProvider.GetService<HttpPushOutgoingQueueProcessor>(),
                typeof(HttpPushOutgoingQueueStep));
        }

        private async Task StartJobs()
        {
            _outputChannels.AddAll(await GetOutputChannels());
            await CreateHttpPushOutgoingQueueJobsAsync(_outputChannels.Where(o => o.ChannelType == ChannelType.HttpPush)
                .ToList());
        }

        private async Task<List<OutputChannel>> GetOutputChannels()
        {
            return await _outputChannelDa.FindAsync();
        }

        private async Task CreateHttpPushOutgoingQueueJobsAsync(List<OutputChannel> outputChannels)
        {

            foreach (var outputChannel in outputChannels)
            {
                var channelSpecification = outputChannel.TypeSpecification as HttpPushOutputChannelSpecification;
                var jobId = outputChannel.JobId;
                if (string.IsNullOrWhiteSpace(jobId))
                {
                    jobId = await CreateJobAsync(channelSpecification?.TargetUrl, outputChannel.ExternalKey);
                }
                await StartJob(jobId);
            }
        }


        public async Task<string> CreateJobAsync(string targetUrl, string outputChannelExternalKey)
        {
            var parameters = new HttpPushOutgoingQueueParameters
            {
                TargetUrl = targetUrl
            };
            var jobId = await NebulaContext.GetJobManager()
                .CreateNewJobOrUpdateDefinition<HttpPushOutgoingQueueStep>(
                    TenantId,
                    $"Wormhole_{outputChannelExternalKey}",
                    $"Wormhole_{outputChannelExternalKey}",
                    new JobConfigurationData
                    {
                        MaxBatchSize = 128,
                        MaxConcurrentBatchesPerWorker = 8,
                        MaxBlockedSecondsPerCycle = 60,
                        MaxTargetQueueLength = 100000,
                        Parameters = JsonConvert.SerializeObject(parameters),
                        QueueTypeName = QueueType.Delayed,
                        IsIndefinite = true
                    });

            await _outputChannelDa.SetJobId(outputChannelExternalKey, jobId);
            return jobId;
        }

        public async Task StartJob(string jobId)
        {
            await NebulaContext.GetJobManager().StartJobIfNotStarted(TenantId, jobId);
        }
    } 
}