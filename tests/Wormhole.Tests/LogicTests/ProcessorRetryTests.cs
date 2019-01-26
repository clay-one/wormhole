using System.Collections.Generic;
using System.Net;
using Wormhole.Configurations;
using Wormhole.Job;
using Xunit;

namespace Wormhole.Tests.LogicTests
{
    public partial class ProcessorRetryTests : HttpOutgoingQueueProcessorTests
    {
        public ProcessorRetryTests()
        {
            Options = Microsoft.Extensions.Options.Options.Create(retryConfiguration);
            Processor = new HttpPushOutgoingQueueProcessor(MockLogger.Object, MockMessageDa.Object, Options);
            NebulaContext.RegisterJobProcessor(Processor, typeof(HttpPushOutgoingQueueStep));
            _step = new HttpPushOutgoingQueueStep
            {
                Category = "test_category",
                FailCount = 0,
                Payload = "test_message"
            };

        }

        private readonly HttpPushOutgoingQueueStep _step;
        private readonly RetryConfiguration retryConfiguration = new RetryConfiguration { Count = 3, Interval = 1 };

        [Theory]
        [InlineData(HttpStatusCode.NotFound)]
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.BadRequest)]
        public void Process_NoRetriableResults_FailAndNoRetry(HttpStatusCode responseCode)
        {
            StubHttp.Stub(x => x.Post("/endpoint"))
                .Return("")
                .WithStatus(responseCode);

            Processor.Initialize(JobData, NebulaContext);
            var result = Processor.Process(new List<HttpPushOutgoingQueueStep> { _step }).GetAwaiter().GetResult();
            Assert.Equal(0, result.ItemsRequeued);
            Assert.Equal(1, result.ItemsFailed);
        }

        [Theory]
        [InlineData(HttpStatusCode.BadGateway)]
        [InlineData(HttpStatusCode.GatewayTimeout)]
        [InlineData(HttpStatusCode.ServiceUnavailable)]
        public void Process_RetriableResults_FailAndRetry(HttpStatusCode responseCode)
        {
            StubHttp.Stub(x => x.Post("/endpoint"))
                .WithStatus(responseCode);

            Processor.Initialize(JobData, NebulaContext);
            var result = Processor.Process(new List<HttpPushOutgoingQueueStep> { _step }).GetAwaiter().GetResult();
            Assert.Equal(1, result.ItemsRequeued);
            Assert.Equal(1, result.ItemsFailed);
        }

        [Fact]
        public void Process_FailedMoreThanRetryCount_FailAndNoRetry()
        {
            var failCount = retryConfiguration.Count;
            _step.FailCount = failCount;
            StubHttp.Stub(x => x.Post("/endpoint"))
                .Return("")
                .WithStatus(HttpStatusCode.BadGateway);
            Processor.Initialize(JobData, NebulaContext);
            var result = Processor.Process(new List<HttpPushOutgoingQueueStep> { _step }).GetAwaiter().GetResult();
            Assert.Equal(failCount + 1, result.ItemsFailed);
            Assert.Equal(0, result.ItemsRequeued);
        }
    }
}