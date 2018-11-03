using System.Threading.Tasks;
using hydrogen.General.Validation;
using Microsoft.AspNetCore.Mvc;
using Wormhole.Api.Model;
using Wormhole.DataImplementation;
using Wormhole.DomainModel;

namespace Wormhole.Api.Controllers
{
    [Route("output-channels")]
    [ApiController]
    public class OutputChannelController : ControllerBase
    {
        private readonly IOutputChannelDa _outputChannelDa;

        public OutputChannelController(IOutputChannelDa outputChannelDa)
        {
            _outputChannelDa = outputChannelDa;
        }

        [HttpPost("http-push")]
        public async Task<IActionResult> AddHttpPushOutputChannel(HttpPushOutputChannelAddRequest input)
        {
            var channel = OutputChannelBuilder.CreateHttpPushOutputChannel(input.ExternalKey, input.TenantId, input.Category, input.Tag, input.TargetUrl, input.PayloadOnly);
            await _outputChannelDa.AddOutputChannel(channel);
            var output = MapToHttpOutput(channel);
            return Ok(ApiValidatedResult<HttpPushOutputChannelAddResponse>.Ok(output));
        }

        [HttpPost("kafka")]
        public async Task<IActionResult> AddKafkaOutputChannel(KafkaOutputChannelAddRequest input)
        {
            var channel = OutputChannelBuilder.CreateKafkaOutputChannel(input.ExternalKey,input.TenantId, input.Category, input.Tag, input.TopicId);
            await _outputChannelDa.AddOutputChannel(channel);
            var output = MapToKafkaOutput(channel);
            return Ok(ApiValidatedResult<KafkaOutputChannelAddResponse>.Ok(output));
        }

        private HttpPushOutputChannelAddResponse MapToHttpOutput(OutputChannel channel)
        {
            var outputChannel = new HttpPushOutputChannelAddResponse
            {
                ExternalKey = channel.ExternalKey,
                TenantId = channel.TenantId,
                Category = channel.FilterCriteria.Category,
                Tag = channel.FilterCriteria.Tag,
                TargetUrl = ((HttpPushOutputChannelSpecification)channel.TypeSpecification).TargetUrl
            };
            return outputChannel;
        }

        private KafkaOutputChannelAddResponse MapToKafkaOutput(OutputChannel channel)
        {
            var outputChannel = new KafkaOutputChannelAddResponse
            {
                ExternalKey = channel.ExternalKey,
                TenantId = channel.TenantId,
                Category = channel.FilterCriteria.Category,
                Tag = channel.FilterCriteria.Tag,
                TopicId = ((KafkaOutputChannelSpecification)channel.TypeSpecification).TargetTopic
            };
            return outputChannel;
        }
    }
}