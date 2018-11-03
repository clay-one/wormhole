using System.Threading.Tasks;
using hydrogen.General.Validation;
using Microsoft.AspNetCore.Mvc;
using Wormhole.Api.Model;
using Wormhole.Interface;

namespace Wormhole.Api.Controllers
{
    [Route("message")]
    [ApiController]
    public class PublishMessageController : ControllerBase
    {
        private readonly IPublishMessageLogic _publishMessageLogic;

        public PublishMessageController(IPublishMessageLogic publishMessageLogic)
        {
            _publishMessageLogic = publishMessageLogic;
        }

        [HttpPost("publish")]
        public async Task<IActionResult> Publish([FromBody] PublishInput input)
        {
            if (input?.Payload == null || string.IsNullOrWhiteSpace(input.Tenant) || string.IsNullOrWhiteSpace(input.Tenant))
            {
                return BadRequest(new { Message = ErrorKeys.ParameterNull});
            }

            var result = await _publishMessageLogic.ProduceMessage(input);

            if (result.Error != null)
                return BadRequest(new { Message = result.Error });

            return Ok(ApiValidationResult.Ok());
        }
    }
}