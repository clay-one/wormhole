using hydrogen.General.Validation;
using Microsoft.AspNetCore.Mvc;
using Wormhole.Api.Model;
using Wormhole.Logic;

namespace Wormhole.Api.Controllers
{
    [Route("api/message")]
    [ApiController]
    public class PublishMessageController : ApiControllerBase
    {
        private IPublishMessage PublishMessageLogic;

        public PublishMessageController(IPublishMessage publishMessageLogic)
        {
            PublishMessageLogic = publishMessageLogic;
        }

        [HttpPost("publish")]
        public ApiValidationResult Publish([FromBody] PublishInput input)
        {
            if (input?.Message == null)
            {
                return ApiValidationResult.Failure(ErrorKeys.ParameterNull);
            }

            PublishMessageLogic.ProduceMessage(input);

            return ApiValidationResult.Ok();
        }
    }
}