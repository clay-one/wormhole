using hydrogen.General.Validation;
using Microsoft.AspNetCore.Mvc;
using Wormhole.Api.Model;
using Wormhole.Job;
using Wormhole.Logic;

namespace Wormhole.Api.Controllers
{
    [Route("api/message")]
    [ApiController]
    public class PublishMessageController : ControllerBase
    {
        private readonly IPublishMessageLogic _publishMessageLogic;

        public PublishMessageController(IPublishMessageLogic publishMessageLogic)
        {
            _publishMessageLogic = publishMessageLogic;
        }

        [HttpPost("publish")]
        public IActionResult Publish([FromBody] PublishInput input)
        {
            if (input?.Message == null || string.IsNullOrWhiteSpace(input.Tenant) || string.IsNullOrWhiteSpace(input.Tenant))
            {
                return BadRequest(new { Message = ErrorKeys.ParameterNull});
            }

            var result = _publishMessageLogic.ProduceMessage(input);

            if (result.Error != null)
                return BadRequest(new { Message = result.Error });

            return Ok(ApiValidationResult.Ok());
        }
    }
}