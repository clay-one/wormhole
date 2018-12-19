using System;
using System.Collections.Generic;
using Moq;
using Nebula;
using Nebula.Queue;
using Nebula.Storage.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Wormhole.DataImplementation;
using Wormhole.Job;
using Xunit;

namespace Wormhole.Tests.LogicTests
{
    public class HttpOutgoingQueueProcessorTests
    {
        private const string TenantId = "test_tenant";
        private const int Score = 100;
        private Mock<ILogger<HttpPushOutgoingQueueProcessor>> MockLogger { get; set; }
        private Mock<IMessageLogDa> MockMessageDa { get; set; }
        private IOptions<RetryConfiguration> Options { get; set; }
        private IJobProcessor<HttpPushOutgoingQueueStep> _processor;
        private readonly NebulaContext _nebulaContext;
        private readonly JobConfigurationData _jobConfiguration;
        private readonly HttpPushOutgoingQueueParameters _parameters;
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
        
        

        public class RetryalbeProcessorTests : HttpOutgoingQueueProcessorTests
        {
            public RetryalbeProcessorTests()
            {
                Options = Microsoft.Extensions.Options.Options.Create(new RetryConfiguration { Count = 3, Interval = 1 });
                _processor = new HttpPushOutgoingQueueProcessor(MockLogger.Object, MockMessageDa.Object, Options);
                _nebulaContext.RegisterJobProcessor(_processor, typeof(HttpPushOutgoingQueueStep));
            }

            [Fact]
            public void Process_RetryWithWrongTargetUrl_Fail()
            {
                _parameters.TargetUrl += "/fake";

                _jobConfiguration.Parameters = JsonConvert.SerializeObject(_parameters);

                var step = new HttpPushOutgoingQueueStep
                {
                    Category = "test_category",
                    FailCount = 0,
                    Payload = "test_message"
                };
                var jobData = new JobData()
                {
                    JobId = "test_job_id",
                    Configuration = _jobConfiguration,
                    TenantId = TenantId
                };
                _processor.Initialize(jobData, _nebulaContext);
                var result = _processor.Process(new List<HttpPushOutgoingQueueStep> { step }).GetAwaiter().GetResult();
                Assert.Equal(1, result.ItemsRequeued);
                Assert.Equal(1, result.ItemsFailed);
            }
        }

        public class UnretryableProcessorTests : HttpOutgoingQueueProcessorTests
        {

            public UnretryableProcessorTests()
            {
                Options = Microsoft.Extensions.Options.Options.Create(new RetryConfiguration { Count = 0, Interval = 0 });
                _processor = new HttpPushOutgoingQueueProcessor(MockLogger.Object, MockMessageDa.Object, Options);
                _nebulaContext.RegisterJobProcessor(_processor, typeof(HttpPushOutgoingQueueStep));
            }

            [Fact]
            public void Initialize_NullJobData_ExceptionThrown()
            {
                Assert.Throws<ArgumentNullException>(() => { _processor.Initialize(null, _nebulaContext); });
            }

            [Fact]
            public void Initialize_NullNebulaContext_ExceptionThrown()
            {
                Assert.Throws<ArgumentNullException>(() => { _processor.Initialize(new JobData(), null); });
            }

            [Fact]
            public void Process_WrongTargetUrl_Fail()
            {
                _parameters.TargetUrl += "/fake";
                _jobConfiguration.Parameters = JsonConvert.SerializeObject(_parameters);

                var step = new HttpPushOutgoingQueueStep()
                {
                    Category = "test_category",
                    FailCount = 0,
                    Payload = "test_message"
                };
                var jobData = new JobData()
                {
                    JobId = "test_job_id",
                    Configuration = _jobConfiguration,
                    TenantId = TenantId
                };
                _processor.Initialize(jobData, _nebulaContext);
                var result = _processor.Process(new List<HttpPushOutgoingQueueStep> { step }).GetAwaiter().GetResult();
                Assert.Equal(1, result.ItemsFailed);
            }

            [Fact]
            public void Process_AllDataSet_Success()
            {
                var step = new HttpPushOutgoingQueueStep()
                {
                    Category = "test_category",
                    FailCount = 0,
                    Payload = "test_message"
                };
                var jobData = new JobData
                {
                    JobId = "test_job_id",
                    Configuration = _jobConfiguration,
                    TenantId = TenantId
                };
                _processor.Initialize(jobData, _nebulaContext);
                var result = _processor.Process(new List<HttpPushOutgoingQueueStep> { step }).GetAwaiter().GetResult();
                Assert.Equal(0, result.ItemsFailed);
                Assert.Equal(0, result.ItemsRequeued);
            }
        }
    }
}