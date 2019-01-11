using System;
using System.Threading;
using Microsoft.Extensions.Options;
using Nebula.Queue.Implementation;
using Wormhole.Configurations;
using Wormhole.DataImplementation;
using Wormhole.DomainModel;
using Wormhole.Job;
using Wormhole.Worker;
using Xunit;

namespace Wormhole.Integration.Tests
{
    public class HttpOutgoingQueueProcessorTests : IClassFixture<TestFixture>
    {
        private readonly NebulaService _nebulaService;
        private const string TestOutputChannelKey = "TestOutputChannel";
        private IOutputChannelDa OutputChannelDa { get; set; }
        private NebulaService NebulaService { get; set; }
        private IMessageLogDa MessageLogDa { get; set; }
        private IOptions<RetryConfiguration> RetryConfiguration { get; set; }
        public HttpOutgoingQueueProcessorTests(TestFixture fixture)
        {
            var outputChannel = new OutputChannel()
            {
                ChannelType = ChannelType.HttpPush,
                TypeSpecification =
                    new HttpPushOutputChannelSpecification()
                    {
                        PayloadOnly = true,
                        TargetUrl = "https://httpstat.us/503/"
                    },
                ExternalKey = TestOutputChannelKey,
                FilterCriteria = new MessageFilterCriteria() { Category = "IncommingMessages", Tag = "SDP"},
                TenantId = "Fanap-plus"
            };
            OutputChannelDa = fixture.ServiceProvider.GetService(typeof(IOutputChannelDa)) as IOutputChannelDa;
            OutputChannelDa.AddOutputChannel(outputChannel).GetAwaiter().GetResult();
            NebulaService = fixture.ServiceProvider.GetService(typeof(NebulaService)) as NebulaService;
            MessageLogDa = fixture.ServiceProvider.GetService(typeof(IMessageLogDa)) as IMessageLogDa;
            RetryConfiguration = fixture.ServiceProvider.GetService(typeof(IOptions<RetryConfiguration>)) as IOptions<RetryConfiguration>;
            fixture.Start().GetAwaiter().GetResult();
        }



        [Fact]
        public void Process_RetriableResults_FailAndRetry()
        {
            var channel = OutputChannelDa.FindAsync(TestOutputChannelKey).GetAwaiter().GetResult();
            var jobId = channel.JobId;
            var stepId = Guid.NewGuid().ToString();
            var queue = NebulaService.NebulaContext
                .JobStepSourceBuilder
                .BuildDelayedJobQueue<HttpPushOutgoingQueueStep>(jobId);
            var step = new HttpPushOutgoingQueueStep()
            {
                Category = channel.FilterCriteria.Category,
                Tag = channel.FilterCriteria.Tag,
                Payload = "test",
                StepId = stepId

            };
            queue.Enqueue(step, DateTime.UtcNow).GetAwaiter().GetResult();
            var waitTime = (int)(RetryConfiguration.Value.Interval * 60 * 1000);
            Thread.Sleep(waitTime);
            var logs = MessageLogDa.FindAsync(stepId).GetAwaiter().GetResult();
            Assert.Equal(logs.Count, RetryConfiguration.Value.Count + 1);
        }
    }
}
