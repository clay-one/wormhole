using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nebula;
using Nebula.Controllers.Dto;
using Nebula.Queue;
using Nebula.Queue.Implementation;
using Nebula.Storage.Model;
using Newtonsoft.Json;
using Wormhole.DataImplementation;
using Wormhole.DomainModel;
using Wormhole.DTO;

namespace Wormhole.Job
{
    public class HttpPushOutgoingQueueProcessor : IFinalizableJobProcessor<HttpPushOutgoingQueueStep>
    {
        private string _jobId;
        private IDelayedJobQueue<HttpPushOutgoingQueueStep> _jobQueue;
        private HttpPushOutgoingQueueParameters _parameters;

        private ILogger<HttpPushOutgoingQueueProcessor> Logger { get; set; }
        private IMessageLogDa MessageLogDa { get; set; }
        private readonly HttpClient _httpClient;


        public HttpPushOutgoingQueueProcessor(ILogger<HttpPushOutgoingQueueProcessor> logger, IMessageLogDa messageLogDa)
        {
            Logger = logger;
            MessageLogDa = messageLogDa;
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
        }

        public Task<long> GetTargetQueueLength()
        {
            return Task.FromResult(0L);
        }

        public void Initialize(JobData jobData, NebulaContext nebulaContext)
        {
            if (jobData == null)
                throw new ArgumentNullException(nameof(jobData), ErrorKeys.ParameterNull);


            if (nebulaContext == null)
                throw new ArgumentNullException(nameof(nebulaContext), ErrorKeys.ParameterNull);

            _jobId = jobData.JobId;
            _jobQueue = nebulaContext.JobStepSourceBuilder.BuildDelayedJobQueue<HttpPushOutgoingQueueStep>(_jobId);
            var parametersString = jobData.Configuration?.Parameters;
            if (string.IsNullOrWhiteSpace(parametersString))
                throw new ArgumentNullException(nameof(jobData.Configuration.Parameters), ErrorKeys.ParameterNull);

            _parameters = JsonConvert.DeserializeObject<HttpPushOutgoingQueueParameters>(parametersString);
        }

        public Task JobCompleted()
        {
            return Task.CompletedTask;
        }

        public async Task<JobProcessingResult> Process(List<HttpPushOutgoingQueueStep> items)
        {
            return JobProcessingResult.Combine(
                await Task.WhenAll(items.Select(ProcessOne).ToArray()));
        }

        private async Task<JobProcessingResult> ProcessOne(HttpPushOutgoingQueueStep item)
        {
            var result = new JobProcessingResult();
            var publishResult = await SendMessage(item);
            item.PublishOutputs.Add(new PublishMessageOutput
            {
                ErrorMessage = publishResult.Error,
                HttpResultCode = publishResult.HttpResponseCode
            });
            if (!publishResult.Success)
            {
                item.FailCount += 1;
                result.ItemsFailed = item.FailCount;
                result.FailureMessages = new[]
                {
                    publishResult.Error
                };
                Logger.LogInformation($"HttpPushOutgoingQueueProcessor - Process FailCount: {item.FailCount}");

                if (item.FailCount < _parameters.RetryCount)
                {
                    result.ItemsRequeued = item.FailCount - 1;
                    await _jobQueue.Enqueue(item, DateTime.UtcNow.AddMinutes(_parameters.RetryInterval));
                    return result;
                }
            }

            await InsertMessageLog(item, publishResult);
            return result;
        }

        private async Task InsertMessageLog(HttpPushOutgoingQueueStep item, SendMessageOutput publishResult)
        {
            var messageLog = new MessageLog()
            {
                Category = item.Category,
                Tag = item.Tag,
                CreatedOn = DateTime.Now,
                Payload = item.Payload,
                PublishMessageOutputs = item.PublishOutputs,
                FailCount = item.FailCount
            };
            await MessageLogDa.AddAsync(messageLog);
        }

        public async Task<SendMessageOutput> SendMessage(HttpPushOutgoingQueueStep input)
        {
            var httpContent = CreateContent(input.Payload);
            var response = await _httpClient.PostAsync(_parameters.TargetUrl, httpContent);
            if (response.IsSuccessStatusCode)
                return new SendMessageOutput
                {
                    Success = true,
                    HttpResponseCode = response.StatusCode
                };

            return new SendMessageOutput
            {
                Success = false,
                HttpResponseCode = response.StatusCode,
                Error = response.ReasonPhrase
            };
        }

        private static HttpContent CreateContent<T>(T body, Dictionary<string, string> headers = null)
        {
            var stringified = JsonConvert.SerializeObject(body);

            var content = new StringContent(stringified, Encoding.UTF8, "application/json");

            if (headers != null)
                foreach (var header in headers)
                    content.Headers.Add(header.Key, header.Value);

            return content;
        }
    }
}