using System;
using System.Collections.Generic;
using Moq;
using Nebula;
using Nebula.Queue;
using Nebula.Storage.Model;
using Microsoft.Extensions.Logging;
using Nebula.Queue.Implementation;
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
        private readonly IJobProcessor<HttpPushOutgoingQueueStep> _processor;
        private readonly NebulaContext _nebulaContext;
        private readonly JobConfigurationData _jobConfiguration;
        private readonly HttpPushOutgoingQueueParameters _parameters;
        public HttpOutgoingQueueProcessorTests()
        {
            var mockLogger = new Mock<ILogger<HttpPushOutgoingQueueProcessor>>();
            var mockMessageDa = new Mock<IMessageLogDa>();
            _processor = new HttpPushOutgoingQueueProcessor(mockLogger.Object, mockMessageDa.Object);
            _nebulaContext = new NebulaContext();

            _nebulaContext.RegisterJobQueue(typeof(MockDelayedQueue), QueueType.Delayed);
            _nebulaContext.RegisterJobProcessor(_processor, typeof(HttpPushOutgoingQueueStep));
            
            _parameters = new HttpPushOutgoingQueueParameters
            {
                RetryCount = 0,
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
        public void Process_RetryWithWrongTargetUrl_Fail()
        {
            _parameters.TargetUrl += "/fake";
            _parameters.RetryCount = 3;
            _parameters.RetryInterval = 1;
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
            Assert.Equal(1, result.ItemsRequeued);
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
            var jobData = new JobData()
            {
                JobId = "test_job_id",
                Configuration = _jobConfiguration,
                TenantId = TenantId
            };
            _processor.Initialize(jobData,_nebulaContext);
            var result = _processor.Process(new List<HttpPushOutgoingQueueStep> {step}).GetAwaiter().GetResult();
            Assert.Equal(0, result.ItemsFailed);
            Assert.Equal(0, result.ItemsRequeued);
        }
    }
}