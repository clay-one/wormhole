using System.Collections.Generic;
using System.Net;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Wormhole.Configurations;
using Wormhole.Job;
using Xunit;

namespace Wormhole.Tests.LogicTests
{
    public class ProcessorGeneralTests : HttpOutgoingQueueProcessorTests
    {
        public ProcessorGeneralTests()
        {
            Options = Microsoft.Extensions.Options.Options.Create(
                new RetryConfiguration { Count = 0, Interval = 0 });
            Processor = new HttpPushOutgoingQueueProcessor(MockLogger.Object, MockMessageDa.Object, Options);
            NebulaContext.RegisterJobProcessor(Processor, typeof(HttpPushOutgoingQueueStep));
        }

        //[Fact]
        //public void Initialize_NullJobData_ExceptionThrown()
        //{
        //    Assert.Throws<ArgumentNullException>(() => { _processor.Initialize(null, _nebulaContext); });
        //}

        //[Fact]
        //public void Initialize_NullNebulaContext_ExceptionThrown()
        //{
        //    Assert.Throws<ArgumentNullException>(() => { _processor.Initialize(new JobData(), null); });
        //}

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