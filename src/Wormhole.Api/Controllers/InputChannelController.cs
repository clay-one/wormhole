using System.Threading.Tasks;
using hydrogen.General.Validation;
using Microsoft.AspNetCore.Mvc;
using Wormhole.Api.Model;
using Wormhole.DataImplementation;
using Wormhole.DomainModel;

namespace Wormhole.Api.Controllers
{
    [Route("input-channels")]
    [ApiController]
    public class InputChannelController : ControllerBase
    {
        private readonly IInputChannelDa _inputChannelDa;

        public InputChannelController(IInputChannelDa inputChannelDa)
        {
            _inputChannelDa = inputChannelDa;
        }

        [HttpPost("http-push")]
        public async Task<IActionResult> AddHttpPushInputChannel(HttpPushInputputChannelAddRequest input)
        {
            var channel = Mapping.AutoMapper.Mapper.Map<InputChannel>(input);
            await _inputChannelDa.AddInputChannelAsync(channel);
            return Ok(ApiValidatedResult<InputChannelAddResponse>.Ok(Mapping.AutoMapper.Mapper.Map<InputChannelAddResponse>(channel)));

        }
    }
}