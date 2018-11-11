using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using ServiceStack.Text;
using Wormhole.Api.Model;
using Wormhole.DTO;
using Wormhole.Interface;
using Wormhole.Job;
using Wormhole.Kafka;

namespace Wormhole.Logic
{
    public class PublishMessageLogic : IPublishMessageLogic
    {
        private static HttpClient _httpClient;
        private readonly IKafkaProducer _producer;
        private readonly ClientConfig _clientConfig;

        public PublishMessageLogic(IKafkaProducer producer, IOptions<ClientConfig> clientConfig)
        {
            _producer = producer;
            _clientConfig = clientConfig.Value;
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
             
        }

        public async Task<ProduceMessageOutput> ProduceMessage(PublishInput input)
        {
            try
            {
                await _producer.ProduceAsync(input);

                return new ProduceMessageOutput
                {
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new ProduceMessageOutput
                {
                    Success = false,
                    Error = ErrorKeys.UnableProduceMessage
                };
            }
        }

        public async Task<SendMessageOutput> SendMessage(OutgoingQueueStep message)
        {
            var httpContent = CreateContent(message);
            var url = _clientConfig.Url;

                    var response =
                        await _httpClient.PostAsync(url, httpContent);

                    if (response.IsSuccessStatusCode)
                        return new SendMessageOutput
                        {
                            Success = true
                        };

                    return new SendMessageOutput
                    {
                        Success = false
                    };
        }

        private static HttpContent CreateContent<T>(T body, Dictionary<string, string> headers = null)
        {
            var serializer = new JsonSerializer<T>();
            var stringified = serializer.SerializeToString(body);

            var content = new StringContent(stringified, Encoding.UTF8, "application/json");

            if (headers != null)
                foreach (var header in headers)
                    content.Headers.Add(header.Key, header.Value);

            return content;
        }
    }
}