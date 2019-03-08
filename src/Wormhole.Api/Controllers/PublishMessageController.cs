using Hydrogen.General.Collections;
using Hydrogen.General.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Wormhole.Api.Attributes;
using Wormhole.Api.Model.PublishModel;
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
        [ClientAuthentication]
        public IActionResult Publish([FromBody] PublishInput input)
        {
            Logger.LogDebug($"PublishMessageController - Publish method called with this input: {input}");
            if (input == null)
            {
                return BadRequest(new ApiValidationError(nameof(input), ErrorKeys.ParameterNull));
            }

            if (input.Payload == null)
            {
                return BadRequest(new ApiValidationError(nameof(PublishInput.Payload), ErrorKeys.ParameterNull));
            }

            if (string.IsNullOrWhiteSpace(input.Category))
            {
                return BadRequest(new ApiValidationError(nameof(PublishInput.Category), ErrorKeys.ParameterNull));
            }

            if (!input.ValidateTags())
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