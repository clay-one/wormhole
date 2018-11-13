using System.Threading.Tasks;
using hydrogen.General.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Wormhole.Api.Model;
using Wormhole.DataImplementation;
using Wormhole.DomainModel;

namespace Wormhole.Api.Controllers
{
    [Route("output-channels")]
    [ApiController]
    public class OutputChannelController : ControllerBase
    {
        private ILogger<OutputChannelController> Logger { get; set; }

        private readonly IOutputChannelDa _outputChannelDa;

        public OutputChannelController(IOutputChannelDa outputChannelDa, ILogger<OutputChannelController> logger)
        {
            _outputChannelDa = outputChannelDa;
            Logger = logger;
        }

        [HttpPost("http-push")]
        public async Task<IActionResult> AddHttpPushOutputChannel(HttpPushOutputChannelAddRequest input)
        {
            Logger.LogDebug($"OutputChannelController - AddHttpPushOutputChannel method called with this input: {input}");

            var channel = Mapping.AutoMapper.Mapper.Map<OutputChannel>(input);

            await _outputChannelDa.AddOutputChannel(channel);

            var output = Mapping.AutoMapper.Mapper.Map<HttpPushOutputChannelAddResponse>(channel);

            return Ok(ApiValidatedResult<HttpPushOutputChannelAddResponse>.Ok(output));
        }

        [HttpPost("kafka")]
        public async Task<IActionResult> AddKafkaOutputChannel(KafkaOutputChannelAddRequest input)
        {
            Logger.LogDebug($"OutputChannelController - AddKafkaOutputChannel method called with this input: {input}");

            var channel = Mapping.AutoMapper.Mapper.Map<OutputChannel>(input);

            await _outputChannelDa.AddOutputChannel(channel);

            var output = Mapping.AutoMapper.Mapper.Map<KafkaOutputChannelAddResponse>(channel);

            return Ok(ApiValidatedResult<KafkaOutputChannelAddResponse>.Ok(output));
        }
    }
}