using System.Threading.Tasks;
using Hydrogen.General.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Wormhole.Api.Model.InputChannels;
using Wormhole.DataImplementation;
using Wormhole.DomainModel;
using ChannelType = Wormhole.Api.Model.ChannelType;

namespace Wormhole.Api.Controllers
{
    [Route("input-channel")]
    [ApiController]
    public class InputChannelController : ControllerBase
    {
        private readonly IInputChannelDa _inputChannelDa;

        public InputChannelController(IInputChannelDa inputChannelDa, ILogger<InputChannelController> logger)
        {
            _inputChannelDa = inputChannelDa;
            Logger = logger;
        }

        private ILogger<InputChannelController> Logger { get; }

        [HttpPost("")]
        public async Task<IActionResult> AddInputChannel(InputChannelAddRequest input)
        {
            if (input?.ChannelType == ChannelType.Unknown)
            {
                return BadRequest(new ApiValidationError(ErrorKeys.UnknownInputChannelType));
            }

            Logger.LogDebug($"InputChannelController - AddInputChannel method called with this input: {input}");
            var channel = Mapping.AutoMapper.Mapper.Map<InputChannel>(input);

            await _inputChannelDa.AddInputChannelAsync(channel);

            return Ok(ApiValidatedResult<InputChannelAddResponse>.Ok(
                Mapping.AutoMapper.Mapper.Map<InputChannelAddResponse>(channel)));
        }
    }
}