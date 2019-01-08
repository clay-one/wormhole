using hydrogen.General.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Wormhole.Api.Model;
using Wormhole.Interface;

namespace Wormhole.Api.Controllers
{
    [Route("message")]
    [ApiController]
    public class PublishMessageController : ControllerBase
    {
        private readonly IPublishMessageLogic _publishMessageLogic;

        public PublishMessageController(IPublishMessageLogic publishMessageLogic,
            ILogger<PublishMessageController> logger)
        {
            _publishMessageLogic = publishMessageLogic;
            Logger = logger;
        }

        private ILogger<PublishMessageController> Logger { get; }

        [HttpPost("publish")]
        public IActionResult Publish([FromBody] PublishInput input)
        {
            Logger.LogDebug($"PublishMessageController - Publish method called with this input: {input}");
            if (input == null)
            {
                return BadRequest(new ApiValidationError("input", ErrorKeys.ParameterNull));
            }

            if (input.Payload == null)
            {
                return BadRequest(new ApiValidationError(nameof(PublishInput.Payload), ErrorKeys.ParameterNull));
            }

            if (string.IsNullOrWhiteSpace(input.Category))
            {
                return BadRequest(new ApiValidationError(nameof(PublishInput.Category), ErrorKeys.ParameterNull));
            }


            if (input.Tags == null || input.Tags.Count < 1)
            {
                return BadRequest(new ApiValidationError(nameof(PublishInput.Tags), ErrorKeys.ParameterNull));
            }

            var result = _publishMessageLogic.ProduceMessage(input);

            if (!string.IsNullOrEmpty(result.Error))
            {
                return BadRequest(new { Message = result.Error });
            }

            return Ok(ApiValidationResult.Ok());
        }
    }
}