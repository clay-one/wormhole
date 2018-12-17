﻿using System;
using System.Collections.Generic;
using System.Linq;
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
using Wormhole.DataImplementation;
using Wormhole.DomainModel;
using Wormhole.DTO;

namespace Wormhole.Job
{
    public class HttpPushOutgoingQueueProcessor : IJobProcessor<HttpPushOutgoingQueueStep>
    {
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
                await Task.WhenAll(items.Select(ProcessOne).ToArray()));
        }

        private async Task<JobProcessingResult> ProcessOne(HttpPushOutgoingQueueStep item)
        {
            var result = new JobProcessingResult();
            var publishResult = await SendMessage(item);
            if (publishResult.Success)
            {
                await InsertMessageLog(item, publishResult);
                return result;
            }

            item.FailCount += 1;
            await InsertMessageLog(item, publishResult);
            result.ItemsFailed = item.FailCount;
            result.FailureMessages = new[]
            {
                publishResult.Error
            };
            Logger.LogInformation(
                $"HttpPushOutgoingQueueProcessor - Process FailCount: {item.FailCount} with {_jobId} job Id.");
            if (item.FailCount > _retryConfig.Count)
                return result;

            result.ItemsRequeued = item.FailCount;
            await _jobQueue.Enqueue(item, DateTime.UtcNow.AddMinutes(_retryConfig.Interval));

            return result;
        }

        private async Task InsertMessageLog(HttpPushOutgoingQueueStep item, SendMessageOutput publishResult)
        {
            var messageLog = new OutgoingMessageLog
            {
                JobStepIdentifier = item.StepId,
                Category = item.Category,
                Tag = item.Tag,
                CreatedOn = DateTime.Now,
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
            var response = new HttpResponseMessage();
            try
            {
                response = await _httpClient.PostAsync(_parameters.TargetUrl, httpContent);
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
            catch (Exception e)
            {
                return new SendMessageOutput
                {
                    Success = false,
                    Error = e.Message
                };
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