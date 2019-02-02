using System;
using System.Collections.Generic;
using System.Net;
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
using Xunit;

namespace Wormhole.Tests.LogicTests
{
    public class ProcessorGeneralTests 
    {
        protected const string TenantId = "test_tenant";
        protected readonly JobConfigurationData JobConfiguration;
        protected readonly NebulaContext NebulaContext;
        protected readonly HttpPushOutgoingQueueParameters Parameters;
        protected IJobProcessor<HttpPushOutgoingQueueStep> Processor;
        protected readonly IHttpServer StubHttp;
        private static int _portNumber = 9050;
        protected readonly JobData JobData;


        protected Mock<ILogger<HttpPushOutgoingQueueProcessor>> MockLogger { get; }
        protected Mock<IMessageLogDa> MockMessageDa { get; }
        protected IOptions<RetryConfiguration> Options { get; set; }
        public ProcessorGeneralTests()
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
            Options = Microsoft.Extensions.Options.Options.Create(
                new RetryConfiguration { Count = 0, Interval = 0 });
            Processor = new HttpPushOutgoingQueueProcessor(MockLogger.Object, MockMessageDa.Object, Options);
            NebulaContext.RegisterJobProcessor(Processor, typeof(HttpPushOutgoingQueueStep));
        }

        [Fact]
        public void Initialize_NullJobData_ExceptionThrown()
        {
            Assert.Throws<ArgumentNullException>(() => { Processor.Initialize(null, NebulaContext); });
        }

        [Fact]
        public void Initialize_NullNebulaContext_ExceptionThrown()
        {
            Assert.Throws<ArgumentNullException>(() => { Processor.Initialize(new JobData(), null); });
        }

        [Fact]
        public void Process_AllDataSet_Success()
        {
            StubHttp.Stub(x => x.Post("/endpoint"))
                .WithStatus(HttpStatusCode.OK);

            var step = new HttpPushOutgoingQueueStep
            {
                Category = "test_category",
                FailCount = 0,
                Payload = "test_message"
            };
            Processor.Initialize(JobData, NebulaContext);
            var result = Processor.Process(new List<HttpPushOutgoingQueueStep> { step }).GetAwaiter().GetResult();
            Assert.Equal(0, result.ItemsFailed);
            Assert.Equal(0, result.ItemsRequeued);
        }

        [Fact]
        public void Process_WrongTargetUrl_Fail()
        {
            StubHttp.Stub(x => x.Post("/endpoint"))
                .WithStatus(HttpStatusCode.NotFound);

            JobConfiguration.Parameters = JsonConvert.SerializeObject(Parameters);

            var step = new HttpPushOutgoingQueueStep
            {
                Category = "test_category",
                FailCount = 0,
                Payload = "test_message"
            };
            Processor.Initialize(JobData, NebulaContext);
            var result = Processor.Process(new List<HttpPushOutgoingQueueStep> { step }).GetAwaiter().GetResult();
            Assert.Equal(1, result.ItemsFailed);
        }
    }
}