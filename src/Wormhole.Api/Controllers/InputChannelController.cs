using System;
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
        private readonly IInputChannelDA _inputChannelDA;

        public InputChannelController(IInputChannelDA inputChannelDA)
        {
            _inputChannelDA = inputChannelDA;
        }

        [HttpPost("http-push")]
        public async Task<IActionResult> AddHttpPushInputChannel(HttpPushInputputChannelAddRequest input)
        {
            var channel = InputChannelBuilder.CreateHttpPushInputChannel(input);
            await _inputChannelDA.AddInputChannelAsync(channel);
            return Ok(ApiValidatedResult<InputChannelAddResponse>.Ok(Mapping.AutoMapper.Mapper.Map<InputChannelAddResponse>(channel)));

        }
    }
}