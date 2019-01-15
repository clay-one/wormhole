using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public class HttpOutgoingQueueProcessorTests : IClassFixture<TestFixture>, IDisposable
    {
        public HttpOutgoingQueueProcessorTests(TestFixture fixture)
        {
            var mongoUtil = fixture.ServiceProvider.GetService(typeof(IMongoUtil)) as IMongoUtil;
            _outputChannelDataGenerator = new OutputChannelDataGenerator(mongoUtil);
            _outputChannelDa = fixture.ServiceProvider.GetService(typeof(IOutputChannelDa)) as IOutputChannelDa;
            _nebulaService = fixture.ServiceProvider.GetService(typeof(NebulaService)) as NebulaService;
            _messageLogDa = fixture.ServiceProvider.GetService(typeof(IMessageLogDa)) as IMessageLogDa;
            _retryConfiguration = fixture.ServiceProvider.GetService(typeof(IOptions<RetryConfiguration>)) as IOptions<RetryConfiguration>;
            _outputChannelDataGenerator.AddHttpPushOutputChannel(TestOutputChannelKey, "https://httpstat.us/503/",
                "incommingMessages", "SDP", "Fanap-plus");
            fixture.Start().GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            _outputChannelDataGenerator.RemoveGenerated(TestOutputChannelKey);
        }

        private readonly OutputChannelDataGenerator _outputChannelDataGenerator;
        private const string TestOutputChannelKey = "TestOutputChannel";
        private readonly IOutputChannelDa _outputChannelDa;
        private readonly NebulaService _nebulaService;
        private readonly IMessageLogDa _messageLogDa;
        private readonly IOptions<RetryConfiguration> _retryConfiguration;

        private async Task<IList<OutgoingMessageLog>> GetLogsWithDelay(string stepId)
        {
            await Task.Delay(TimeSpan.FromMinutes(_retryConfiguration.Value.Interval * _retryConfiguration.Value.Count + 1));
            return _messageLogDa.FindAsync(stepId).GetAwaiter().GetResult().OrderBy(l=>l.CreatedOn).ToList();
        }

        [Fact]
        public async Task Process_RetriableResults_FailAndRetry()
        {
            var channel = _outputChannelDa.FindAsync(TestOutputChannelKey).GetAwaiter().GetResult();
            var jobId = channel.JobId;
            var stepId = Guid.NewGuid().ToString();
            var queue = _nebulaService.NebulaContext
                .JobStepSourceBuilder
                .BuildDelayedJobQueue<HttpPushOutgoingQueueStep>(jobId);
            var step = new HttpPushOutgoingQueueStep
            {
                Category = channel.FilterCriteria.Category,
                Tag = channel.FilterCriteria.Tag,
                Payload = "test",
                StepId = stepId

            };
            queue.Enqueue(step, DateTime.UtcNow).GetAwaiter().GetResult();
            var outgoingMessageLogs = await GetLogsWithDelay(stepId);
            Assert.Equal(outgoingMessageLogs.Count, _retryConfiguration.Value.Count + 1);
            if (outgoingMessageLogs.Count > 1)
            {
                var diff = outgoingMessageLogs[1].CreatedOn.Subtract(outgoingMessageLogs[0].CreatedOn).TotalMinutes;
                Assert.True(Math.Abs(diff - _retryConfiguration.Value.Interval) < 0.05);
            }
        }
    }
}
