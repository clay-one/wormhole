using System;
using System.Collections.Generic;
using System.Net;
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
    public class HttpOutgoingQueueProcessorTests
    {
        private const string TenantId = "test_tenant";
        private const int Score = 100;
        private readonly JobConfigurationData _jobConfiguration;
        private readonly NebulaContext _nebulaContext;
        private readonly HttpPushOutgoingQueueParameters _parameters;
        private IJobProcessor<HttpPushOutgoingQueueStep> _processor;

        public HttpOutgoingQueueProcessorTests()
        {
            MockLogger = new Mock<ILogger<HttpPushOutgoingQueueProcessor>>();
            MockMessageDa = new Mock<IMessageLogDa>();
            _nebulaContext = new NebulaContext();
            _nebulaContext.RegisterJobQueue(typeof(MockDelayedQueue), QueueType.Delayed);

            _parameters = new HttpPushOutgoingQueueParameters
            {
                TargetUrl = "http://s1ghasedak10:8006/api/v2/receive/incoming"
            };

            _jobConfiguration = new JobConfigurationData
            {
                MaxBatchSize = 1,
                MaxConcurrentBatchesPerWorker = 5,
                IdleSecondsToCompletion = 30,
                MaxBlockedSecondsPerCycle = 60,
                MaxTargetQueueLength = 100000,
                QueueTypeName = QueueType.Redis,
                Parameters = JsonConvert.SerializeObject(_parameters)
            };
        }

        private Mock<ILogger<HttpPushOutgoingQueueProcessor>> MockLogger { get; }
        private Mock<IMessageLogDa> MockMessageDa { get; }
        private IOptions<RetryConfiguration> Options { get; set; }


        public class ProcessorRetryTests : HttpOutgoingQueueProcessorTests
        {
            public ProcessorRetryTests()
            {
                Options = Microsoft.Extensions.Options.Options.Create(retryConfiguration);
                _processor = new HttpPushOutgoingQueueProcessor(MockLogger.Object, MockMessageDa.Object, Options);
                _nebulaContext.RegisterJobProcessor(_processor, typeof(HttpPushOutgoingQueueStep));
                _stubHttp = HttpMockRepository.At("http://localhost:9191");
                _parameters.TargetUrl = "http://localhost:9191/endpoint";
                _step = new HttpPushOutgoingQueueStep
                {
                    Category = "test_category",
                    FailCount = 0,
                    Payload = "test_message"
                };

                _jobConfiguration.Parameters = JsonConvert.SerializeObject(_parameters);


                _jobData = new JobData
                {
                    JobId = "test_job_id",
                    Configuration = _jobConfiguration,
                    TenantId = TenantId
                };
            }

            private readonly IHttpServer _stubHttp;
            private readonly HttpPushOutgoingQueueStep _step;
            private readonly JobData _jobData;
            private readonly RetryConfiguration retryConfiguration = new RetryConfiguration { Count = 3, Interval = 1 };

            [Theory]
            [InlineData(HttpStatusCode.NotFound)]
            [InlineData(HttpStatusCode.Unauthorized)]
            [InlineData(HttpStatusCode.BadRequest)]
            public void Process_NoRetriableResults_FailAndNoRetry(HttpStatusCode responseCode)
            {
                _stubHttp.Stub(x => x.Post("/endpoint"))
                    .Return("")
                    .WithStatus(responseCode);

                _processor.Initialize(_jobData, _nebulaContext);
                var result = _processor.Process(new List<HttpPushOutgoingQueueStep> { _step }).GetAwaiter().GetResult();
                Assert.Equal(0, result.ItemsRequeued);
                Assert.Equal(1, result.ItemsFailed);
            }

            [Theory]
            [InlineData(HttpStatusCode.BadGateway)]
            [InlineData(HttpStatusCode.GatewayTimeout)]
            [InlineData(HttpStatusCode.ServiceUnavailable)]
            public void Process_RetriableResults_FailAndRetry(HttpStatusCode responseCode)
            {
                _stubHttp.Stub(x => x.Post("/endpoint"))
                    .WithStatus(responseCode);

                _jobConfiguration.Parameters = JsonConvert.SerializeObject(_parameters);
                _processor.Initialize(_jobData, _nebulaContext);
                var result = _processor.Process(new List<HttpPushOutgoingQueueStep> { _step }).GetAwaiter().GetResult();
                Assert.Equal(1, result.ItemsRequeued);
                Assert.Equal(1, result.ItemsFailed);
            }

            [Fact]
            public void Process_FailedMoreThanRetryCount_FailAndNoRetry()
            {
                _jobConfiguration.Parameters = JsonConvert.SerializeObject(_parameters);
                var failCount = retryConfiguration.Count;
                _step.FailCount = failCount;
                _stubHttp.Stub(x => x.Post("/endpoint"))
                    .Return("")
                    .WithStatus(HttpStatusCode.BadGateway);
                _processor.Initialize(_jobData, _nebulaContext);
                var result = _processor.Process(new List<HttpPushOutgoingQueueStep> { _step }).GetAwaiter().GetResult();
                Assert.Equal(failCount + 1, result.ItemsFailed);
                Assert.Equal(0, result.ItemsRequeued);
            }
        }

        //public class ProcessorGeneralTests : HttpOutgoingQueueProcessorTests
        //{
        //    public ProcessorGeneralTests()
        //    {
        //        Options = Microsoft.Extensions.Options.Options.Create(
        //            new RetryConfiguration { Count = 0, Interval = 0 });
        //        _processor = new HttpPushOutgoingQueueProcessor(MockLogger.Object, MockMessageDa.Object, Options);
        //        _nebulaContext.RegisterJobProcessor(_processor, typeof(HttpPushOutgoingQueueStep));
        //    }

        //    [Fact]
        //    public void Initialize_NullJobData_ExceptionThrown()
        //    {
        //        Assert.Throws<ArgumentNullException>(() => { _processor.Initialize(null, _nebulaContext); });
        //    }

        //    [Fact]
        //    public void Initialize_NullNebulaContext_ExceptionThrown()
        //    {
        //        Assert.Throws<ArgumentNullException>(() => { _processor.Initialize(new JobData(), null); });
        //    }

        //    [Fact]
        //    public void Process_AllDataSet_Success()
        //    {
        //        var step = new HttpPushOutgoingQueueStep
        //        {
        //            Category = "test_category",
        //            FailCount = 0,
        //            Payload = "test_message"
        //        };
        //        var jobData = new JobData
        //        {
        //            JobId = "test_job_id",
        //            Configuration = _jobConfiguration,
        //            TenantId = TenantId
        //        };
        //        _processor.Initialize(jobData, _nebulaContext);
        //        var result = _processor.Process(new List<HttpPushOutgoingQueueStep> { step }).GetAwaiter().GetResult();
        //        Assert.Equal(0, result.ItemsFailed);
        //        Assert.Equal(0, result.ItemsRequeued);
        //    }

        //    [Fact]
        //    public void Process_WrongTargetUrl_Fail()
        //    {
        //        _parameters.TargetUrl += "/fake";
        //        _jobConfiguration.Parameters = JsonConvert.SerializeObject(_parameters);

        //        var step = new HttpPushOutgoingQueueStep
        //        {
        //            Category = "test_category",
        //            FailCount = 0,
        //            Payload = "test_message"
        //        };
        //        var jobData = new JobData
        //        {
        //            JobId = "test_job_id",
        //            Configuration = _jobConfiguration,
        //            TenantId = TenantId
        //        };
        //        _processor.Initialize(jobData, _nebulaContext);
        //        var result = _processor.Process(new List<HttpPushOutgoingQueueStep> { step }).GetAwaiter().GetResult();
        //        Assert.Equal(1, result.ItemsFailed);
        //    }
        //}
    }
}