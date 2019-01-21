using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nebula;
using Nebula.Queue;
using Nebula.Queue.Implementation;
using Nebula.Storage.Model;
using Newtonsoft.Json;
using Wormhole.Configurations;
using Wormhole.DataImplementation;
using Wormhole.DomainModel;
using Wormhole.DTO;

namespace Wormhole.Job
{
    public class HttpPushOutgoingQueueProcessor : IJobProcessor<HttpPushOutgoingQueueStep>
    {
        private static readonly List<HttpStatusCode> RetriableHttpResponses = new List<HttpStatusCode>
        {
            HttpStatusCode.BadGateway,HttpStatusCode.GatewayTimeout,HttpStatusCode.ServiceUnavailable
        };

        private readonly HttpClient _httpClient;
        private readonly RetryConfiguration _retryConfig;
        private string _jobId;
        private IDelayedJobQueue<HttpPushOutgoingQueueStep> _jobQueue;
        private HttpPushOutgoingQueueParameters _parameters;

        public HttpPushOutgoingQueueProcessor(ILogger<HttpPushOutgoingQueueProcessor> logger,
            IMessageLogDa messageLogDa, IOptions<RetryConfiguration> options)
        {
            Logger = logger;
            MessageLogDa = messageLogDa;
            _httpClient = new HttpClient();
            _retryConfig = options.Value;
        }

        private ILogger<HttpPushOutgoingQueueProcessor> Logger { get; }
        private IMessageLogDa MessageLogDa { get; }

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

        public async Task<JobProcessingResult> Process(List<HttpPushOutgoingQueueStep> items)
        {
            return JobProcessingResult.Combine(
                await Task.WhenAll(items.Select(item =>
                    SendMessage(item).ContinueWith(sendMessageOutputTask => LogMessage(sendMessageOutputTask, item)))));
        }

        private JobProcessingResult LogMessage(Task<SendMessageOutput> sendMessageOutputTask, HttpPushOutgoingQueueStep item)
        {
            var jobProcessingResult = new JobProcessingResult();

            var publishResult = sendMessageOutputTask.Result;
            if (publishResult.Success)
            {
                InsertMessageLog(item, publishResult).GetAwaiter().GetResult();
                return jobProcessingResult; 
            }

            item.FailCount += 1;
            jobProcessingResult.ItemsFailed = item.FailCount;
            InsertMessageLog(item, publishResult).GetAwaiter().GetResult();
            jobProcessingResult.FailureMessages = new[]
            {
                publishResult.Error
            };
            Logger.LogInformation(
                $"HttpPushOutgoingQueueProcessor - Process FailCount: {item.FailCount} with {_jobId} job Id.");

            if (!RetryPolicyMeets(publishResult.HttpResponseCode))
                return jobProcessingResult; 

            if (item.FailCount > _retryConfig.Count)
                return jobProcessingResult; 

            jobProcessingResult.ItemsRequeued = item.FailCount;
            _jobQueue.Enqueue(item, publishResult.ResponseTime.AddMinutes(_retryConfig.Interval)).GetAwaiter().GetResult();

            return jobProcessingResult; 
        }

        private bool RetryPolicyMeets(HttpStatusCode responseCode)
        {
            return RetriableHttpResponses.Contains(responseCode);
        }

        private async Task InsertMessageLog(HttpPushOutgoingQueueStep item, SendMessageOutput publishResult)
        {
            var messageLog = new OutgoingMessageLog
            {
                JobId = _jobId,
                JobStepIdentifier = item.StepId,
                Category = item.Category,
                Tag = item.Tag,
                ResponseTime = publishResult.ResponseTime,
                Payload = item.Payload,
                PublishOutput = new PublishMessageOutput
                {
                    ErrorMessage = publishResult.Error,
                    HttpResultCode = publishResult.HttpResponseCode
                },
                FailCount = item.FailCount
            };
            await MessageLogDa.AddAsync(messageLog);
        }

        public async Task<SendMessageOutput> SendMessage(HttpPushOutgoingQueueStep input)
        {
            var httpContent = CreateContent(input.Payload);
            var output = new SendMessageOutput();
            try
            {
                var response = await _httpClient.PostAsync(_parameters.TargetUrl, httpContent);
                output.ResponseTime = DateTime.UtcNow;
                output.Success = response.IsSuccessStatusCode;
                output.HttpResponseCode = response.StatusCode;
                output.Error = response.ReasonPhrase;
                return output;
            }
            catch (Exception e)
            {
                output.Error = e.Message;
                output.ResponseTime = DateTime.UtcNow;
                return output;
            }
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