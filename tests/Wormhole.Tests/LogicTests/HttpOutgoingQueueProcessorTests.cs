using System;
using System.Threading;
using HttpMock;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Nebula;
using Nebula.Queue;
using Nebula.Storage.Model;
using Newtonsoft.Json;
using Wormhole.Configurations;
using Wormhole.DataImplementation;
using Wormhole.Job;
using Wormhole.Tests.Mocks;

namespace Wormhole.Tests.LogicTests
{
    public class HttpOutgoingQueueProcessorTests
    {
        protected const string TenantId = "test_tenant";
        protected const int Score = 100;
        protected readonly JobConfigurationData JobConfiguration;
        protected readonly NebulaContext NebulaContext;
        protected readonly HttpPushOutgoingQueueParameters Parameters;
        protected IJobProcessor<HttpPushOutgoingQueueStep> Processor;
        protected readonly IHttpServer StubHttp;
        private static int _portNumber= 9090;
        protected readonly JobData JobData;

        public HttpOutgoingQueueProcessorTests()
        {
            MockLogger = new Mock<ILogger<HttpPushOutgoingQueueProcessor>>();
            MockMessageDa = new Mock<IMessageLogDa>();
            NebulaContext = new NebulaContext();
            NebulaContext.RegisterJobQueue(typeof(MockDelayedQueue), QueueType.Delayed);
            Interlocked.Increment(ref _portNumber);
            var baseAddress = $"http://localhost:{_portNumber}";
            Parameters = new HttpPushOutgoingQueueParameters
            {
                TargetUrl = $"{baseAddress}/endpoint"
            };
            StubHttp = HttpMockRepository.At(baseAddress);
            JobConfiguration = new JobConfigurationData
            {
                MaxBatchSize = 1,
                MaxConcurrentBatchesPerWorker = 5,
                IdleSecondsToCompletion = 30,
                MaxBlockedSecondsPerCycle = 60,
                MaxTargetQueueLength = 100000,
                QueueTypeName = QueueType.Redis,
                Parameters = JsonConvert.SerializeObject(Parameters)
            };

            JobData = new JobData
            {
                JobId = "test_job_id",
                Configuration = JobConfiguration,
                TenantId = TenantId
            };
        }

        protected Mock<ILogger<HttpPushOutgoingQueueProcessor>> MockLogger { get; }
        protected Mock<IMessageLogDa> MockMessageDa { get; }
        protected IOptions<RetryConfiguration> Options { get; set; }
        
    }
}