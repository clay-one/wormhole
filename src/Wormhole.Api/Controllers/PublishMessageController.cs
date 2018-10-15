using hydrogen.General.Validation;
using Microsoft.AspNetCore.Mvc;
using Wormhole.Api.Model;
using Wormhole.Logic;

namespace Wormhole.Api.Controllers
{
    [Route("message")]
    [ApiController]
    public class PublishMessageController : ControllerBase
    {
        private IPublishMessageLogic PublishMessageLogic;

        public PublishMessageController(IPublishMessageLogic publishMessageLogic)
        {
            PublishMessageLogic = publishMessageLogic;
        }

        [HttpPost("publish")]
        public IActionResult Publish([FromBody] PublishInput input)
        {
            if (input?.Message == null || string.IsNullOrWhiteSpace(input.Tenant) || string.IsNullOrWhiteSpace(input.Tenant))
            {
                return BadRequest(new { Message = ErrorKeys.ParameterNull});
            }

            var result = PublishMessageLogic.ProduceMessage(input);

            if (result.Error != null)
                return BadRequest(new { Message = result.Error });

            return Ok(ApiValidationResult.Ok());
        }
    }
}